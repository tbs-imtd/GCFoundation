import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test.describe('FDCP API documentation', () => {
  test('uses the page template guide with code and demo pages', async ({ page }) => {
    await page.goto('/en/template/api-docs');
    await expect(page.locator('gcds-side-nav')).toHaveCount(0);
    await expect(page.locator('gcds-heading[tag="h1"]')).toContainText('API documentation');
    await expect(page.locator('gcds-heading[id="preview"]')).toBeVisible();
    await page.locator('gcds-button[href="/en/template/api-docs/code"]').click();
    await expect(page.locator('.fdcp-table')).toContainText('fdcp-api-docs-section after-endpoints');
    await page.getByRole('link', { name: 'Open the API documentation demo', exact: true }).click();
    await expect(page).toHaveURL(/\/en\/template\/api-docs\/demo$/);
    await expect(page.locator('#requests-api')).toBeVisible();
  });

  test('keeps the standalone reference in Components and redirects former page links', async ({ page }) => {
    await page.goto('/en/components/api-reference');
    const reference = page.locator('#health-reference');
    await expect(reference).toBeVisible();
    await expect(reference).toHaveCSS('display', 'block');
    await expect(reference.locator('nav')).toHaveCount(0);
    await expect(reference.locator('.api-docs__endpoint')).toHaveCount(1);
    await page.goto('/en/components/api-docs');
    await expect(page).toHaveURL(/\/en\/template\/api-docs$/);
    await page.goto('/fr/composants/documentation-api/demo');
    await expect(page).toHaveURL(/\/fr\/modele\/documentation-api\/demo$/);
  });

  test('renders Razor sections, updates navigation and exposes schema links', async ({ page }, testInfo) => {
    await page.goto('/en/template/api-docs/demo');
    const docs = page.locator('#requests-api');
    await expect(docs).toHaveAttribute('data-navigation-ready', 'true');
    await expect(docs.getByRole('heading', { name: 'Client certificates' })).toBeVisible();
    const endpointLink = docs.locator('.api-docs__nav').getByRole('link', { name: 'GET /requests/{id}', exact: true });
    await endpointLink.click();
    await expect(endpointLink).toHaveAttribute('aria-current', 'location');
    await expect(docs.locator('.api-docs__endpoint')).toHaveCount(2);
    await expect(docs.locator('a[href="#requests-api-schema-Request"]').first()).toBeVisible();
    const download = await page.request.get('/api-spec/requests.json');
    expect(download.ok()).toBeTruthy();
    expect((await download.json()).openapi).toBe('3.1.0');
    await docs.scrollIntoViewIfNeeded();
    await page.screenshot({ path: testInfo.outputPath('desktop.png') });
  });

  test('stacks at narrow widths without page overflow and supports print', async ({ page }, testInfo) => {
    await page.setViewportSize({ width: 390, height: 844 });
    await page.goto('/en/template/api-docs/demo');
    const docs = page.locator('#requests-api');
    const nav = docs.locator('.api-docs__nav');
    await expect(nav).toHaveCSS('position', 'static');
    expect(await docs.evaluate(el => el.scrollWidth <= el.clientWidth + 1)).toBe(true);
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth + 1)).toBe(true);
    await docs.scrollIntoViewIfNeeded();
    await page.screenshot({ path: testInfo.outputPath('mobile.png') });
    await page.emulateMedia({ media: 'print' });
    await expect(nav).toBeHidden();
    await expect(docs.locator('pre').first()).toHaveCSS('white-space', 'pre-wrap');
  });

  test('keeps current and focused navigation links readable @a11y', async ({ page }) => {
    await page.goto('/en/template/api-docs/demo');
    const nav = page.locator('#requests-api .api-docs__nav');
    const overview = nav.getByRole('link', { name: 'Overview', exact: true });
    const certificates = nav.getByRole('link', { name: 'Client certificates', exact: true });
    const textColour = await page.locator('#requests-api').evaluate(el => getComputedStyle(el).color);
    await expect(overview).toHaveCSS('color', textColour);
    await certificates.hover();
    await expect(certificates).toHaveCSS('color', textColour);
    const contrast = async (link: typeof overview) => link.evaluate(el => {
      const styles = getComputedStyle(el);
      const luminance = (colour: string) => {
        const channels = colour.match(/[\d.]+/g)!.slice(0, 3).map(Number).map(channel => {
          const value = channel / 255;
          return value <= 0.04045 ? value / 12.92 : ((value + 0.055) / 1.055) ** 2.4;
        });
        return channels[0] * 0.2126 + channels[1] * 0.7152 + channels[2] * 0.0722;
      };
      const foreground = luminance(styles.color);
      const background = luminance(styles.backgroundColor);
      return (Math.max(foreground, background) + 0.05) / (Math.min(foreground, background) + 0.05);
    });

    // Clicking combines hover, focus and current-section styles (the original regression).
    await overview.click();
    await expect(overview).toBeFocused();
    await expect(overview).toHaveAttribute('aria-current', 'location');
    expect(await contrast(overview)).toBeGreaterThanOrEqual(4.5);

    // Keyboard focus must remain visible on another link while Overview stays current.
    await page.keyboard.press('Tab');
    await expect(certificates).toBeFocused();
    await expect(overview).toHaveCSS('color', textColour);
    expect(await contrast(certificates)).toBeGreaterThanOrEqual(4.5);
    expect(await contrast(overview)).toBeGreaterThanOrEqual(4.5);
    await expect(certificates).not.toHaveCSS('outline-style', 'none');
    await page.keyboard.press('Enter');
    await expect(certificates).toHaveAttribute('aria-current', 'location');
    expect(await contrast(certificates)).toBeGreaterThanOrEqual(4.5);
  });

  test('uses French labels and translated authored sections', async ({ page }) => {
    await page.goto('/fr/modele/documentation-api/demo');
    const docs = page.locator('#requests-api');
    await expect(docs.getByRole('heading', { name: 'Points de terminaison', exact: true })).toBeVisible();
    await expect(docs.getByRole('heading', { name: 'Certificats clients', exact: true })).toBeVisible();
  });

  test('remains readable with JavaScript disabled', async ({ browser }) => {
    const context = await browser.newContext({ javaScriptEnabled: false });
    try {
      const page = await context.newPage();
      await page.goto(`${test.info().project.use.baseURL}/en/template/api-docs/demo`);
      const docs = page.locator('#requests-api');
      await expect(docs.locator('.api-docs__endpoint')).toHaveCount(2);
      await expect(docs.getByRole('heading', { name: 'Client certificates', exact: true })).toBeVisible();
      await expect(docs.locator('pre').first()).toHaveAttribute('tabindex', '0');
    } finally { await context.close(); }
  });

  test('has no automated accessibility violations in the component @a11y', async ({ browser }) => {
    const context = await browser.newContext({ bypassCSP: true });
    try {
      const page = await context.newPage();
      await page.goto(`${test.info().project.use.baseURL}/en/template/api-docs/demo`);
      const results = await new AxeBuilder({ page }).include('#requests-api').analyze();
      expect(results.violations).toEqual([]);
    } finally { await context.close(); }
  });
});
