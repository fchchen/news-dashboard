#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
API_URL="http://localhost:5000"
OUTPUT_DIR="$ROOT_DIR/frontend/public/data"

mkdir -p "$OUTPUT_DIR"

# Generate fresh Anthropic News RSS by scraping their news page
echo "==> Scraping Anthropic news page for fresh RSS..."
ANTHROPIC_RSS="$ROOT_DIR/frontend/public/anthropic-news.xml"
node -e "
const https = require('https');
const get = (url) => new Promise((resolve, reject) => {
  https.get(url, {headers: {'User-Agent': 'Mozilla/5.0'}}, res => {
    if (res.statusCode >= 300 && res.statusCode < 400 && res.headers.location) {
      return get(res.headers.location).then(resolve).catch(reject);
    }
    let d = ''; res.on('data', c => d += c); res.on('end', () => resolve(d));
  }).on('error', reject);
});
(async () => {
  const html = await get('https://www.anthropic.com/news');
  const links = [...new Set(html.match(/href=\"\/news\/[^\"]+/g))].map(l => l.replace('href=\"', ''));
  let items = '';
  for (const link of links.slice(0, 30)) {
    try {
      const page = await get('https://www.anthropic.com' + link);
      const title = (page.match(/og:title\" content=\"([^\"]+)/) || [])[1] || '';
      const desc = (page.match(/og:description\" content=\"([^\"]+)/) || [])[1] || '';
      const dateMatch = page.match(/body-3 agate\">([^<]+)</) || page.match(/(\w+ \d+,? 202[456])/);
      const dateStr = dateMatch ? dateMatch[1].trim() : '';
      const pubDate = dateStr ? new Date(dateStr).toUTCString() : '';
      if (!title) continue;
      const esc = s => s.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
      items += '<item><title>' + esc(title) + '</title><link>https://www.anthropic.com' + link +
        '</link><description>' + esc(desc) + '</description>' +
        (pubDate ? '<pubDate>' + pubDate + '</pubDate>' : '') + '</item>\n';
      console.error('    + ' + title.substring(0,60) + ' (' + dateStr + ')');
    } catch(e) { console.error('    Skip', link, e.message); }
  }
  const rss = '<?xml version=\"1.0\" encoding=\"UTF-8\"?><rss version=\"2.0\"><channel>' +
    '<title>Anthropic News</title><link>https://www.anthropic.com/news</link>' +
    '<description>Latest news from Anthropic</description>' + items + '</channel></rss>';
  require('fs').writeFileSync(process.argv[1], rss);
  console.log('    Generated RSS with ' + (items.match(/<item>/g)||[]).length + ' articles');
})();
" "$ANTHROPIC_RSS"

# Serve the scraped RSS locally so the .NET API can fetch it
python3 -m http.server 8888 --directory "$ROOT_DIR/frontend/public" &
LOCAL_SERVER_PID=$!

echo "==> Starting .NET API in background..."
export ANTHROPIC_RSS_URL="http://localhost:8888/anthropic-news.xml"
dotnet run --project "$ROOT_DIR/src/Api/NewsDashboard.Api.csproj" --no-launch-profile &
API_PID=$!

cleanup() {
  echo "==> Stopping API (PID $API_PID)..."
  kill "$API_PID" 2>/dev/null || true
  kill "$LOCAL_SERVER_PID" 2>/dev/null || true
  wait "$API_PID" 2>/dev/null || true
  wait "$LOCAL_SERVER_PID" 2>/dev/null || true
}
trap cleanup EXIT

# Wait for API to be healthy
echo "==> Waiting for API health check..."
for i in $(seq 1 60); do
  if curl -sf "$API_URL/health" > /dev/null 2>&1; then
    echo "    API is healthy after ${i}s"
    break
  fi
  if [ "$i" -eq 60 ]; then
    echo "ERROR: API did not become healthy within 60s"
    exit 1
  fi
  sleep 1
done

# Wait for all data sources to be populated (30s startup delay + fetch time)
echo "==> Waiting for all data sources..."
HN_READY=false
GH_READY=false
RSS_READY=false
for i in $(seq 1 180); do
  if [ "$HN_READY" = false ]; then
    RESULT=$(curl -sf "$API_URL/api/hackernews?page=1&pageSize=1" 2>/dev/null || echo '{}')
    TOTAL=$(echo "$RESULT" | grep -o '"totalCount":[0-9]*' | grep -o '[0-9]*' || echo "0")
    if [ "$TOTAL" -gt 0 ]; then
      echo "    HackerNews ready after ${i}s (totalCount=$TOTAL)"
      HN_READY=true
    fi
  fi
  if [ "$GH_READY" = false ]; then
    RESULT=$(curl -sf "$API_URL/api/github/releases?page=1&pageSize=1" 2>/dev/null || echo '{}')
    TOTAL=$(echo "$RESULT" | grep -o '"totalCount":[0-9]*' | grep -o '[0-9]*' || echo "0")
    if [ "$TOTAL" -gt 0 ]; then
      echo "    GitHub Releases ready after ${i}s (totalCount=$TOTAL)"
      GH_READY=true
    fi
  fi
  if [ "$RSS_READY" = false ]; then
    RESULT=$(curl -sf "$API_URL/api/rss?page=1&pageSize=1" 2>/dev/null || echo '{}')
    TOTAL=$(echo "$RESULT" | grep -o '"totalCount":[0-9]*' | grep -o '[0-9]*' || echo "0")
    if [ "$TOTAL" -gt 0 ]; then
      echo "    RSS ready after ${i}s (totalCount=$TOTAL)"
      RSS_READY=true
    fi
  fi
  if [ "$HN_READY" = true ] && [ "$GH_READY" = true ] && [ "$RSS_READY" = true ]; then
    echo "    All sources ready after ${i}s"
    break
  fi
  if [ "$i" -eq 180 ]; then
    echo "WARNING: Timeout after 180s. HN=$HN_READY GH=$GH_READY RSS=$RSS_READY"
    echo "         Proceeding with available data."
  fi
  sleep 2
done

echo "==> Capturing API responses..."

curl -sf "$API_URL/api/dashboard" -o "$OUTPUT_DIR/dashboard.json"
echo "    dashboard.json"

curl -sf "$API_URL/api/hackernews?page=1&pageSize=1000" -o "$OUTPUT_DIR/hackernews.json"
echo "    hackernews.json"

curl -sf "$API_URL/api/github/releases?page=1&pageSize=1000" -o "$OUTPUT_DIR/github-releases.json"
echo "    github-releases.json"

curl -sf "$API_URL/api/rss?page=1&pageSize=1000" -o "$OUTPUT_DIR/rss.json"
echo "    rss.json"

curl -sf "$API_URL/api/news/trends" -o "$OUTPUT_DIR/trends.json"
echo "    trends.json"

curl -sf "$API_URL/api/rss/sources" -o "$OUTPUT_DIR/rss-sources.json"
echo "    rss-sources.json"

echo "==> Static data captured successfully to $OUTPUT_DIR"
ls -la "$OUTPUT_DIR"
