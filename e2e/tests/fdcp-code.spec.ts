import { test, expect, type Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

const WCAG_21_AA = ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'];

const pages = [
  { path: '/en/components/code', name: 'Code component (English)' },
  { path: '/fr/composants/code', name: 'Code component (French)' }
];

/** Highlights the active panel of every rendered component by bringing it into view. */
async function highlightActivePanels(page: Page) {
  const roots = page.locator('[data-fdcp-code-ready]');
  for (let i = 0; i < await roots.count(); i++) {
    const code = roots.nth(i).locator('[data-fdcp-code-active] code');
    // Blocks inside closed disclosures are highlighted when they are opened.
    if (!await code.isVisible()) continue;
    await code.scrollIntoViewIfNeeded();
    await expect(code).toHaveAttribute('data-fdcp-code-highlight', /.+/);
  }
}

/** Visits every language of every example, calling `check` once per combination. */
async function forEachLanguage(page: Page, check: (state: string) => Promise<void>) {
  const selects = page.locator('[data-fdcp-code-select] select');
  const optionCounts = await selects.evaluateAll(list => list.map(s => (s as HTMLSelectElement).options.length));
  const rounds = Math.max(1, ...optionCounts);
  for (let round = 0; round < rounds; round++) {
    for (let i = 0; i < optionCounts.length; i++) {
      await selects.nth(i).selectOption({ index: Math.min(round, optionCounts[i] - 1) });
    }
    await highlightActivePanels(page);
    await check(`language round ${round + 1}`);
  }
}

function lowContrastTokens(page: Page) {
  return page.evaluate(() => {
    const luminance = (colour: string) => {
      const [r, g, b] = colour.match(/[\d.]+/g)!.slice(0, 3).map(Number).map(channel => {
        const value = channel / 255;
        return value <= 0.04045 ? value / 12.92 : ((value + 0.055) / 1.055) ** 2.4;
      });
      return r * 0.2126 + g * 0.7152 + b * 0.0722;
    };
    const ratio = (a: string, b: string) => {
      const [x, y] = [luminance(a), luminance(b)].sort((m, n) => n - m);
      return (x + 0.05) / (y + 0.05);
    };
    return Array.from(document.querySelectorAll<HTMLElement>('[data-fdcp-code] [data-fdcp-code-active] .token, [data-fdcp-code] .fdcp-code__language, [data-fdcp-code] .fdcp-code__title'))
      .filter(el => el.getClientRects().length > 0 && el.textContent!.trim())
      .map(el => {
        const background = getComputedStyle(el.closest('[data-fdcp-code]')!).backgroundColor;
        return { text: el.textContent!.trim().slice(0, 40), className: el.className, ratio: ratio(getComputedStyle(el).color, background) };
      })
      .filter(result => result.ratio < 4.5);
  });
}

test.describe('FDCP code', () => {
  for (const target of pages) {
    test(`has no WCAG 2.1 AA violations in any language: ${target.name} @a11y`, async ({ browser }) => {
      const context = await browser.newContext({ bypassCSP: true });
      try {
        const page = await context.newPage();
        await page.goto(`${test.info().project.use.baseURL}${target.path}`);
        await expect(page.locator('[data-fdcp-code-ready]').first()).toBeAttached();
        await forEachLanguage(page, async state => {
          const results = await new AxeBuilder({ page }).include('[data-fdcp-code]').withTags(WCAG_21_AA).analyze();
          expect(results.violations, `${target.name}, ${state}`).toEqual([]);
          expect(await lowContrastTokens(page), `${target.name}, ${state}`).toEqual([]);
        });
      } finally { await context.close(); }
    });
  }

  test('is operable with the keyboard and keeps focus visible @a11y', async ({ page, context }) => {
    await context.grantPermissions(['clipboard-read', 'clipboard-write']);
    await page.goto('/en/components/code');
    const create = page.locator('#code-example-create');
    const read = page.locator('#code-example-read');
    const select = create.getByRole('combobox', { name: 'Code language' });

    await expect(select).toHaveAccessibleDescription('Create a request');
    await select.focus();
    await expect(select).not.toHaveCSS('outline-style', 'none');
    await page.keyboard.press('ArrowDown');
    await expect(create.locator('[data-fdcp-code-active]')).toHaveAttribute('data-fdcp-code-language', 'javascript');
    await expect(select).toBeFocused();
    await expect(read.locator('[data-fdcp-code-active]')).toHaveAttribute('data-fdcp-code-language', 'javascript');
    await expect(create.locator('[data-fdcp-code-panel]:visible')).toHaveCount(1);
    await expect(create.locator('[data-fdcp-code-active]')).toHaveCSS('border-top-style', 'none');

    const status = create.getByRole('status');
    await expect(status).toBeAttached();
    await expect(status).toHaveAttribute('aria-live', 'polite');
    await expect(status).toHaveAttribute('aria-atomic', 'true');
    expect(await status.evaluate(el => getComputedStyle(el).display)).not.toBe('none');
    await expect(page.locator('#code-block-response').getByRole('combobox')).toHaveCount(0);
    const dark = page.locator('#code-dark-sample');
    await expect(dark).toHaveAttribute('data-fdcp-code-theme', 'dark');
    await expect(dark).toHaveCSS('color-scheme', 'dark');
    await expect(dark.getByRole('combobox')).toHaveCount(0);

    await page.keyboard.press('Tab');
    const copy = create.getByRole('button', { name: 'Copy code' });
    await expect(copy).toBeFocused();
    await expect(copy).toHaveAccessibleDescription('Create a request');
    await expect(copy).not.toHaveCSS('outline-style', 'none');
    await page.keyboard.press('Enter');
    await expect(status).toHaveText('Code copied to the clipboard.');
    await expect(copy).toBeFocused();
    const source = await create.locator('[data-fdcp-code-active] code').textContent();
    // The Windows clipboard converts line feeds to CRLF.
    expect((await page.evaluate(() => navigator.clipboard.readText())).replace(/\r\n/g, '\n')).toBe(source);

    await page.keyboard.press('Tab');
    const pre = create.locator('[data-fdcp-code-active] pre');
    await expect(pre).toBeFocused();
    await expect(pre).toHaveAccessibleName('Create a request (JavaScript)');
    await expect(pre).not.toHaveCSS('outline-style', 'none');
    if (await pre.evaluate(el => el.scrollWidth > el.clientWidth)) {
      await page.keyboard.press('ArrowRight');
      await expect.poll(() => pre.evaluate(el => el.scrollLeft)).toBeGreaterThan(0);
    }
  });

  test('selects the code and explains how to copy when the clipboard fails @a11y', async ({ page }) => {
    await page.addInitScript(() => {
      Object.defineProperty(navigator, 'clipboard', { value: { writeText: () => Promise.reject(new Error('denied')) } });
    });
    await page.goto('/fr/composants/code');
    const read = page.locator('#code-example-read');
    await expect(read.getByRole('button', { name: 'Copier le code' })).toBeVisible();
    await read.locator('[data-fdcp-code-copy]').click();
    await expect(read.getByRole('status')).toContainText('Ctrl+C');
    const source = await read.locator('[data-fdcp-code-active] code').textContent();
    await expect.poll(() => page.evaluate(() => String(window.getSelection()))).toBe(source);
  });

  for (const target of pages) {
    test(`reflows at 320 CSS pixels: ${target.name} @a11y`, async ({ page }) => {
      await page.setViewportSize({ width: 320, height: 800 });
      await page.goto(target.path);
      await expect(page.locator('[data-fdcp-code-ready]').first()).toBeAttached();
      expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth + 1)).toBe(true);
      const overflowing = await page.locator('[data-fdcp-code], [data-fdcp-code] select, [data-fdcp-code] button')
        .evaluateAll(list => list.filter(el => {
          if (el.getBoundingClientRect().right <= window.innerWidth + 1) return false;
          // Content inside a scrolling data table is exempt from reflow when the table itself fits.
          for (let parent = el.parentElement; parent && parent !== document.body; parent = parent.parentElement) {
            if (getComputedStyle(parent).overflowX !== 'visible' && parent.getBoundingClientRect().right <= window.innerWidth + 1) return false;
          }
          return true;
        }).map(el => el.id || el.className));
      expect(overflowing).toEqual([]);
    });
  }

  test('keeps text readable with WCAG text spacing @a11y', async ({ browser }) => {
    const context = await browser.newContext({ bypassCSP: true });
    try {
      const page = await context.newPage();
      await page.goto(`${test.info().project.use.baseURL}/en/components/code`);
      await page.addStyleTag({
        content: '* { line-height: 1.5 !important; letter-spacing: 0.12em !important; word-spacing: 0.16em !important; } p { margin-bottom: 2em !important; }'
      });
      const clipped = await page.locator('[data-fdcp-code] .fdcp-code__title, [data-fdcp-code] .fdcp-code__language, [data-fdcp-code] label, [data-fdcp-code] button')
        .evaluateAll(list => list.filter(el => el.getClientRects().length > 0 && (el.scrollWidth > el.clientWidth + 1 || el.scrollHeight > el.clientHeight + 1)).map(el => el.textContent));
      expect(clipped).toEqual([]);
      expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth + 1)).toBe(true);
    } finally { await context.close(); }
  });

  test('shows every variant without JavaScript', async ({ browser }) => {
    const context = await browser.newContext({ javaScriptEnabled: false });
    try {
      const page = await context.newPage();
      await page.goto(`${test.info().project.use.baseURL}/en/components/code`);
      const create = page.locator('#code-example-create');
      await expect(create.locator('[data-fdcp-code-controls]')).toBeHidden();
      await expect(create.locator('[data-fdcp-code-panel]:visible')).toHaveCount(3);
      await expect(create.locator('.fdcp-code__language:visible')).toHaveText(['C#', 'JavaScript', 'Python']);
      await expect(create.locator('pre').first()).toHaveAttribute('tabindex', '0');
    } finally { await context.close(); }
  });

  test('keeps borders and focus indicators in forced colours @a11y', async ({ page }) => {
    await page.emulateMedia({ forcedColors: 'active' });
    await page.goto('/en/components/code');
    const create = page.locator('#code-example-create');
    await expect(create).not.toHaveCSS('border-style', 'none');
    const pre = create.locator('[data-fdcp-code-active] pre');
    await pre.focus();
    await expect(pre).not.toHaveCSS('outline-style', 'none');
  });
});
