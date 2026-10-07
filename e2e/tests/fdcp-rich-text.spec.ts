import { test, expect } from '@playwright/test';

test.describe('FDCP rich text', () => {
  test('Tab and Shift+Tab move focus out of the editor @core', async ({ page }) => {
    await page.goto('/template/stepper/demo?step=2');

    const container = page.locator('[data-fdcp-rich-text="true"][data-for="Bio"]');
    await expect(container).toHaveAttribute('data-quill-initialized', 'true');

    const editor = container.locator('.ql-editor');
    await editor.click();
    await page.keyboard.type('test');
    await expect(editor).toBeFocused();

    await page.keyboard.press('Tab');
    await expect(editor).not.toBeFocused();
    await expect(editor).not.toContainText('\t');

    await editor.click();
    await page.keyboard.press('Shift+Tab');
    await expect(editor).not.toBeFocused();
  });
});
