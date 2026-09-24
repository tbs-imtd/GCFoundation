import type { Page, Locator } from '@playwright/test';
import { expect } from '@playwright/test';

/**
 * Utility functions for testing FDCP Grid Table accessibility
 */

export interface GridTableAccessibilityResult {
  hasCaption: boolean;
  captionText: string;
  hasScreenReaderOnlyCaption: boolean;
  captionInsideGrid: boolean;
  columnHeaderCount: number;
  allColumnsHaveScope: boolean;
  firstCellsHaveRowScope: boolean;
  hasProperAriaSort: boolean;
  hasFocusIndicators: boolean;
  hasAriaLabels: boolean;
}

/**
 * Comprehensive accessibility check for a Grid Table
 */
export async function checkGridTableAccessibility(
  page: Page,
  tableSelector: string = 'table.gridjs-table'
): Promise<GridTableAccessibilityResult> {
  const table = page.locator(tableSelector);
  await expect(table).toBeVisible();

  // Caption is outside role="grid" table and linked via aria-labelledby
  const captionId = await table.getAttribute('aria-labelledby');
  const caption = captionId
    ? page.locator(`#${captionId}`)
    : table.locator('caption');
  const hasCaption = (await caption.count()) > 0;
  const captionText = hasCaption ? (await caption.textContent()) || '' : '';
  const captionClass = hasCaption ? await caption.getAttribute('class') : '';
  const hasScreenReaderOnlyCaption = captionClass?.includes('visibility-sr-only') || false;
  const captionInsideGrid = (await table.locator('caption').count()) > 0;

  // Check column headers
  const headers = page.locator('thead th.gridjs-th');
  const columnHeaderCount = await headers.count();
  
  let allColumnsHaveScope = true;
  for (let i = 0; i < columnHeaderCount; i++) {
    const scope = await headers.nth(i).getAttribute('scope');
    if (scope !== 'col') {
      allColumnsHaveScope = false;
      break;
    }
  }

  // Check row headers (first cell of each row)
  const rows = page.locator('tbody tr.gridjs-tr');
  const rowCount = await rows.count();
  let firstCellsHaveRowScope = true;
  
  for (let i = 0; i < Math.min(rowCount, 5); i++) {
    const firstCell = rows.nth(i).locator('td').first();
    const scope = await firstCell.getAttribute('scope');
    if (scope !== 'row') {
      firstCellsHaveRowScope = false;
      break;
    }
  }

  // Check aria-sort
  let hasProperAriaSort = true;
  for (let i = 0; i < columnHeaderCount; i++) {
    const ariaSort = await headers.nth(i).getAttribute('aria-sort');
    // Should be 'none', 'ascending', 'descending', or null (acceptable for unsortable columns)
    if (ariaSort && !['none', 'ascending', 'descending'].includes(ariaSort)) {
      hasProperAriaSort = false;
      break;
    }
  }

  // Check focus indicators
  const firstHeader = headers.first();
  await firstHeader.focus();
  const hasFocusIndicators = await firstHeader.evaluate(el => {
    const styles = window.getComputedStyle(el);
    return styles.outlineWidth !== '0px' || styles.boxShadow !== 'none';
  });

  // Check ARIA labels on the grid table (not the generic wrapper div)
  const ariaLabel = await table.getAttribute('aria-label');
  const ariaLabelledBy = await table.getAttribute('aria-labelledby');
  const hasAriaLabels = ariaLabel !== null || ariaLabelledBy !== null;

  return {
    hasCaption,
    captionText: captionText.trim(),
    hasScreenReaderOnlyCaption,
    captionInsideGrid,
    columnHeaderCount,
    allColumnsHaveScope,
    firstCellsHaveRowScope,
    hasProperAriaSort,
    hasFocusIndicators,
    hasAriaLabels
  };
}

/**
 * Test sorting and verify aria-sort updates correctly
 */
export async function testSortAccessibility(
  page: Page,
  headerIndex: number = 0
): Promise<boolean> {
  const header = page.locator('thead th.gridjs-th').nth(headerIndex);
  await expect(header).toBeVisible();

  // Click to sort ascending
  await header.click();
  await page.waitForTimeout(500);
  
  let ariaSort = await header.getAttribute('aria-sort');
  if (ariaSort !== 'ascending') return false;

  // Click to sort descending
  await header.click();
  await page.waitForTimeout(500);
  
  ariaSort = await header.getAttribute('aria-sort');
  if (ariaSort !== 'descending') return false;

  // Check other headers have 'none'
  const headers = page.locator('thead th.gridjs-th');
  const count = await headers.count();
  
  for (let i = 0; i < count; i++) {
    if (i === headerIndex) continue;
    const otherAriaSort = await headers.nth(i).getAttribute('aria-sort');
    if (otherAriaSort !== 'none' && otherAriaSort !== null) return false;
  }

  return true;
}

/**
 * Test keyboard navigation
 */
export async function testKeyboardNavigation(
  page: Page,
  element: Locator
): Promise<boolean> {
  await element.focus();
  
  // Verify focus
  const isFocused = await element.evaluate(el => el === document.activeElement);
  if (!isFocused) return false;

  // Test Enter key
  await page.keyboard.press('Enter');
  await page.waitForTimeout(300);

  // Test Space key
  await page.keyboard.press('Space');
  await page.waitForTimeout(300);

  return true;
}

/**
 * Check if element has proper focus indicator
 */
export async function hasFocusIndicator(locator: Locator): Promise<boolean> {
  await locator.focus();
  
  return await locator.evaluate(el => {
    const styles = window.getComputedStyle(el);
    return styles.outlineWidth !== '0px' || styles.boxShadow !== 'none';
  });
}

/**
 * Get color contrast ratio (simplified check)
 * For production, use axe-core's contrast check which is more accurate
 */
export async function getColorInfo(locator: Locator): Promise<{
  color: string;
  backgroundColor: string;
}> {
  return await locator.evaluate(el => {
    const styles = window.getComputedStyle(el);
    return {
      color: styles.color,
      backgroundColor: styles.backgroundColor
    };
  });
}

/**
 * Check if pagination has proper screen reader announcements
 */
export async function checkPaginationAccessibility(
  page: Page
): Promise<{
  hasSummary: boolean;
  summaryHasRole: boolean;
  summaryRole: string | null;
  hasNavigation: boolean;
}> {
  const summary = page.locator('.gridjs-summary');
  const hasSummary = await summary.count() > 0;
  
  const summaryRole = hasSummary ? await summary.getAttribute('role') : null;
  const summaryHasRole = summaryRole === 'status';

  const pagination = page.locator('.gridjs-pagination');
  const hasNavigation = await pagination.count() > 0;

  return {
    hasSummary,
    summaryHasRole,
    summaryRole,
    hasNavigation
  };
}

/**
 * Verify all interactive elements are keyboard accessible
 */
export async function verifyKeyboardAccessibility(
  page: Page,
  selectors: string[]
): Promise<{ element: string; accessible: boolean }[]> {
  const results: { element: string; accessible: boolean }[] = [];

  for (const selector of selectors) {
    const elements = page.locator(selector);
    const count = await elements.count();
    
    if (count === 0) {
      results.push({ element: selector, accessible: false });
      continue;
    }

    const firstElement = elements.first();
    
    try {
      await firstElement.focus();
      const isFocused = await firstElement.evaluate(el => el === document.activeElement);
      results.push({ element: selector, accessible: isFocused });
    } catch {
      results.push({ element: selector, accessible: false });
    }
  }

  return results;
}

/**
 * Test screen reader text (aria-label, aria-labelledby, etc.)
 */
export async function getScreenReaderText(locator: Locator): Promise<{
  ariaLabel: string | null;
  ariaLabelledBy: string | null;
  ariaDescribedBy: string | null;
  title: string | null;
  alt: string | null;
}> {
  return await locator.evaluate(el => ({
    ariaLabel: el.getAttribute('aria-label'),
    ariaLabelledBy: el.getAttribute('aria-labelledby'),
    ariaDescribedBy: el.getAttribute('aria-describedby'),
    title: el.getAttribute('title'),
    alt: el.getAttribute('alt')
  }));
}

