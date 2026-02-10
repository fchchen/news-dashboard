#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
API_URL="http://localhost:5000"
OUTPUT_DIR="$ROOT_DIR/frontend/public/data"

mkdir -p "$OUTPUT_DIR"

echo "==> Starting .NET API in background..."
dotnet run --project "$ROOT_DIR/src/Api/NewsDashboard.Api.csproj" --no-launch-profile &
API_PID=$!

cleanup() {
  echo "==> Stopping API (PID $API_PID)..."
  kill "$API_PID" 2>/dev/null || true
  wait "$API_PID" 2>/dev/null || true
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
