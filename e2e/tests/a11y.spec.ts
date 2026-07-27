import { test, expect } from '@playwright/test';
import { runAxeAndAttach } from '../utils/a11y';

// Allow axe injection and external resources
test.use({ bypassCSP: true });

const routes: Array<{ path: string; name: string }> = [
  { path: '/home', name: 'Home' },
  { path: '/components', name: 'Components index' },
  { path: '/components/card', name: 'Components Card' },
  { path: '/components/badge', name: 'Components Badge' },
  { path: '/components/filtered-search', name: 'Components Filtered Search' },
  { path: '/components/form', name: 'Components Form' },
  { path: '/components/form-builder', name: 'Components Form Builder' },
  { path: '/components/modal', name: 'Components Modal' },
  { path: '/components/page-heading', name: 'Components Page Heading' },
  { path: '/components/stepper', name: 'Components Stepper' },
  { path: '/components/tabs', name: 'Components Tabs' },
  { path: '/components/user-login', name: 'Components User Login' },
  { path: '/template', name: 'Template' },
  { path: '/installation/global-resources', name: 'Global Resources' }
];

for (const route of routes) {
  test(`a11y: ${route.name} @a11y`, async ({ page }, testInfo) => {
    const res = await page.goto(route.path);
    expect(res?.ok()).toBeTruthy();
    await page.waitForLoadState('load');

    const results = await runAxeAndAttach(page, testInfo);
    const serious = results.violations.filter(v => v.impact === 'serious' || v.impact === 'critical');
    expect.soft(serious, `Serious/critical a11y violations on ${route.name}`).toHaveLength(0);
  });
}


