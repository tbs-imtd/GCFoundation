import type { Result } from 'axe-core';
import { test, expect } from '@playwright/test';
import { runAxeAndAttach } from '../utils/a11y';

// Bypass CSP in tests to prevent CSP from blocking external resources and axe injection
test.use({ bypassCSP: true });

test.describe('Smoke', () => {
  test('home page loads, no console errors, basic a11y passes @smoke', async ({ page, browserName }) => {
    // Capture console errors during navigation and load
    const consoleErrors: string[] = [];
    page.on('console', msg => { if (msg.type() === 'error') consoleErrors.push(msg.text()); });

    const response = await page.goto('/');
    expect(response?.ok()).toBeTruthy();

    // Wait for page to be fully loaded instead of checking body visibility
    await page.waitForLoadState('load');

    // Axe a11y (WCAG A/AA) with detailed attachments
    const results = await runAxeAndAttach(page, test.info());
    const serious = results.violations.filter((v: Result) => v.impact === 'serious' || v.impact === 'critical');
    expect.soft(serious, `A11y serious violations on ${browserName}`).toHaveLength(0);

    // Allow page events to flush
    await page.waitForTimeout(50);

    // Filter out known benign CDN/CORS warnings for the GC Design System CDN in CI
    const filtered = consoleErrors.filter(msg => {
      const lower = msg.toLowerCase();
      const isGcdsCdn = lower.includes('cdn.design-system.alpha.canada.ca');
      if (isGcdsCdn && lower.includes('content security policy')) return false;
      if (isGcdsCdn && lower.includes('refused to connect')) return false;
      if (isGcdsCdn && (lower.includes('cors policy') || lower.includes("access-control-allow-origin"))) return false;
      // Some browsers emit a generic net::ERR_FAILED without the URL; ignore if a related CDN error exists
      if (lower.includes('net::err_failed') && consoleErrors.some(m => m.toLowerCase().includes('cdn.design-system.alpha.canada.ca'))) return false;
      return true;
    });
    expect(filtered, 'No unexpected console errors on initial load').toHaveLength(0);
  });
});


