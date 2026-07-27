import { test, expect } from '@playwright/test';

test.describe('FDCP tabs', () => {
  test('supports manual keyboard activation and scoped panels @core', async ({ page }) => {
    await page.goto('/components/tabs');

    const component = page.locator('#profile-tabs');
    const tabs = component.getByRole('tab');
    const overviewTab = tabs.nth(0);
    const detailsTab = tabs.nth(1);
    const historyTab = tabs.nth(2);
    const detailsPanel = component.getByRole('tabpanel', { name: 'Details' });

    await expect(component).toHaveAttribute('data-fdcp-tabs-initialized', 'true');
    await expect(overviewTab).toHaveAttribute('aria-selected', 'true');
    await expect(detailsTab).toHaveAttribute('aria-selected', 'false');
    await expect(detailsPanel).toBeHidden();

    await overviewTab.focus();
    await overviewTab.press('ArrowRight');

    await expect(detailsTab).toBeFocused();
    await expect(detailsTab).toHaveAttribute('aria-selected', 'false');
    await expect(detailsPanel).toBeHidden();

    await detailsTab.press('Enter');

    await expect(detailsTab).toHaveAttribute('aria-selected', 'true');
    await expect(detailsTab).toBeFocused();
    await expect(detailsPanel).toBeVisible();

    await detailsTab.press('ArrowRight');

    await expect(historyTab).toBeFocused();
    await expect(historyTab).toHaveAttribute('aria-selected', 'false');
  });

  test('lazy-loads a panel once and keeps generated IDs unique @core', async ({ page }) => {
    await page.goto('/components/tabs');

    const component = page.locator('#profile-tabs');
    const serverTab = component.getByRole('tab', { name: 'Server data' });
    const serverPanel = component.getByRole('tabpanel', { name: 'Server data' });

    const idsAreUnique = await component.locator('[id]').evaluateAll((elements) => {
      const ids = elements.map((element) => element.id);
      return new Set(ids).size === ids.length;
    });
    expect(idsAreUnique).toBe(true);

    await serverTab.click();

    await expect(serverTab).toBeFocused();
    await expect(serverPanel).toContainText(
      'This content was loaded from the server when the tab was selected.'
    );
    await expect(serverPanel).toHaveAttribute('data-loaded', 'true');
    await expect(serverPanel).not.toHaveAttribute('aria-busy', 'true');
  });
});
