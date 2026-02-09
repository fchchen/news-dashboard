import { chromium } from 'playwright';

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 1440, height: 900 } });

  page.on('console', msg => console.log('CONSOLE:', msg.type(), msg.text()));
  page.on('pageerror', err => console.log('PAGE ERROR:', err.message));

  await page.goto('http://localhost:4200/dashboard', { waitUntil: 'networkidle' });
  await page.waitForTimeout(3000);

  const html = await page.content();
  console.log('HTML length:', html.length);
  console.log('Body content:', (await page.$eval('body', el => el.innerHTML)).substring(0, 2000));

  await browser.close();
})();
