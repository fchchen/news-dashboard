import { chromium } from 'playwright';

const BASE = 'http://localhost:4200';
const DIR = '/home/fchch/dev/news-dashboard/screenshots';

const pages = [
  { path: '/dashboard', name: '01-dashboard', fullPage: true },
  { path: '/hacker-news', name: '02-hacker-news', fullPage: true },
  { path: '/github-releases', name: '03-github-releases', fullPage: true },
  { path: '/rss-feeds', name: '04-rss-feeds', fullPage: true },
];

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    deviceScaleFactor: 2,
  });

  for (const { path, name, fullPage } of pages) {
    const page = await context.newPage();
    console.log(`Navigating to ${path}...`);
    await page.goto(`${BASE}${path}`, { waitUntil: 'networkidle' });
    // Extra wait for animations and lazy loading
    await page.waitForTimeout(1500);
    await page.screenshot({
      path: `${DIR}/${name}.png`,
      fullPage,
    });
    console.log(`  Saved ${name}.png`);
    await page.close();
  }

  await browser.close();
  console.log('Done! All screenshots captured.');
})();
