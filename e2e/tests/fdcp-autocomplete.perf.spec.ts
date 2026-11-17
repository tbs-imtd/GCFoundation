import { test, expect, type Page } from '@playwright/test';
import { generateRecords, mockDataSource } from '../utils/autocomplete-data';

/**
 * Browser performance budgets for fdcp-autocomplete, against a mocked data source with thousands of records.
 * Run with: npm run test:perf
 *
 * Budgets are generous multiples of what a developer machine needs. They catch regressions in how the work
 * scales (rendering every row, a request per keystroke), not small timing differences.
 */

const EN = '/en/components/autocomplete';
const DEBOUNCE = 250;

// Timings are only meaningful without other tests competing for the CPU.
test.describe.configure({ mode: 'serial' });

async function open(page: Page) {
  await page.goto(EN);
  await expect(page.locator('#directory')).toHaveAttribute('data-fdcp-autocomplete-initialized', 'true');
}

/** Types a term in the page and resolves with the milliseconds until results are on screen. */
function timeToResults(page: Page, id: string, term: string) {
  return page.evaluate(({ id, term }) => new Promise<number>(resolve => {
    const root = document.getElementById(id)!;
    const input = root.querySelector<HTMLInputElement>('[data-fdcp-autocomplete-input]')!;
    const start = performance.now();

    root.addEventListener('fdcp-autocomplete:loaded', () => {
      // One frame later the rows have been painted.
      requestAnimationFrame(() => resolve(performance.now() - start));
    }, { once: true });

    input.focus();
    input.value = term;
    input.dispatchEvent(new Event('input', { bubbles: true }));
  }), { id, term });
}

test.describe('FDCP autocomplete: performance budgets', () => {
  test('fast typing sends one request, not one per keystroke @perf', async ({ page }) => {
    await open(page);
    const mock = await mockDataSource(page, 'directory', generateRecords(10000), { matchAll: true });
    const input = page.locator('#directory [data-fdcp-autocomplete-input]');

    await input.pressSequentially('tremblay', { delay: 40 });
    await expect(page.locator('#directory [role="option"]').first()).toBeVisible();
    await page.waitForTimeout(DEBOUNCE + 150);

    expect(mock.requests.map(r => r.term)).toEqual(['tremblay']);
  });

  test('first results for a 10,000-record match appear quickly @perf', async ({ page }, testInfo) => {
    await open(page);
    await mockDataSource(page, 'directory', generateRecords(10000), { matchAll: true });

    const elapsed = await timeToResults(page, 'directory', 'an');
    testInfo.annotations.push({ type: 'timing', description: `first results: ${elapsed.toFixed(0)} ms (includes ${DEBOUNCE} ms debounce)` });

    expect(elapsed).toBeGreaterThanOrEqual(DEBOUNCE - 5);
    expect(elapsed).toBeLessThan(DEBOUNCE + 750);
    expect(await page.locator('#directory [role="option"]').count()).toBeLessThanOrEqual(20);
  });

  test('a cached search renders without the network @perf', async ({ page }, testInfo) => {
    await open(page);
    const mock = await mockDataSource(page, 'directory', generateRecords(10000), { matchAll: true, delays: { an: 400 } });

    const cold = await timeToResults(page, 'directory', 'an');
    await timeToResults(page, 'directory', 'roy');
    const warm = await timeToResults(page, 'directory', 'an');
    testInfo.annotations.push({ type: 'timing', description: `cold ${cold.toFixed(0)} ms, cached ${warm.toFixed(0)} ms` });

    expect(mock.requests.filter(r => r.term === 'an').length).toBe(1);
    expect(cold).toBeGreaterThan(DEBOUNCE + 350);
    expect(warm).toBeLessThan(DEBOUNCE + 150);
  });

  test('scrolling through 5,000 rows keeps frames short and the DOM small @perf', async ({ page }, testInfo) => {
    await open(page);
    await mockDataSource(page, 'directory', generateRecords(10000), { matchAll: true });
    await timeToResults(page, 'directory', 'an');

    const result = await page.evaluate(async () => {
      const root = document.getElementById('directory')!;
      const listbox = root.querySelector<HTMLElement>('[data-fdcp-autocomplete-listbox]')!;
      const frame = () => new Promise<number>(resolve => requestAnimationFrame(resolve));
      const longTasks: number[] = [];
      const observer = new PerformanceObserver(list => list.getEntries().forEach(entry => longTasks.push(entry.duration)));
      observer.observe({ entryTypes: ['longtask'] });

      const steps = 150;
      const distance = listbox.scrollHeight - listbox.clientHeight;
      const gaps: number[] = [];
      let maxRows = 0;
      let previous = await frame();

      for (let step = 1; step <= steps; step++) {
        listbox.scrollTop = (distance * step) / steps;
        const now = await frame();
        gaps.push(now - previous);
        previous = now;
        maxRows = Math.max(maxRows, root.querySelectorAll('[role="option"]').length);
      }

      observer.disconnect();
      gaps.sort((a, b) => a - b);

      return {
        median: gaps[Math.floor(gaps.length / 2)],
        p95: gaps[Math.floor(gaps.length * 0.95)],
        longTasks: longTasks.length,
        maxRows,
        lastPosition: root.querySelector('[role="option"]:last-child')?.getAttribute('aria-posinset')
      };
    });

    testInfo.annotations.push({ type: 'timing', description: `frame median ${result.median.toFixed(1)} ms, p95 ${result.p95.toFixed(1)} ms, long tasks ${result.longTasks}, max rows ${result.maxRows}` });

    expect(result.maxRows).toBeLessThanOrEqual(20);
    expect(result.median).toBeLessThan(34);
    expect(result.p95).toBeLessThan(100);
    expect(result.longTasks).toBeLessThanOrEqual(2);
  });

  test('holding the down arrow through 300 rows stays responsive @perf', async ({ page }, testInfo) => {
    await open(page);
    await mockDataSource(page, 'directory', generateRecords(10000), { matchAll: true });
    await timeToResults(page, 'directory', 'an');

    const result = await page.evaluate(() => {
      const root = document.getElementById('directory')!;
      const input = root.querySelector<HTMLInputElement>('[data-fdcp-autocomplete-input]')!;
      const start = performance.now();
      let dangling = 0;

      for (let press = 0; press < 300; press++) {
        input.dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowDown', bubbles: true, cancelable: true }));
        if (!document.getElementById(input.getAttribute('aria-activedescendant') ?? '')) dangling++;
      }

      return {
        perKey: (performance.now() - start) / 300,
        dangling,
        position: root.querySelector('[role="option"][aria-selected="true"]')?.getAttribute('aria-posinset'),
        rows: root.querySelectorAll('[role="option"]').length
      };
    });

    testInfo.annotations.push({ type: 'timing', description: `${result.perKey.toFixed(3)} ms per key press` });

    expect(result.position).toBe('300');
    expect(result.dangling).toBe(0);
    expect(result.rows).toBeLessThanOrEqual(20);
    expect(result.perKey).toBeLessThan(8);
  });

  test('100 searches leave the DOM and cache the same size @perf', async ({ page }) => {
    await open(page);
    await mockDataSource(page, 'directory', generateRecords(10000), { matchAll: true });
    const size = () => page.evaluate(() => {
      const root = document.getElementById('directory') as any;
      return { nodes: root.querySelectorAll('*').length, cache: root.FDCPAutocompleteInstance.cache.size };
    });

    await timeToResults(page, 'directory', 'term 0');
    const first = await size();

    await page.evaluate(() => { (document.getElementById('directory') as any).FDCPAutocompleteInstance.debounce = 0; });
    for (let index = 1; index <= 100; index++) {
      await timeToResults(page, 'directory', `term ${index}`);
    }
    const last = await size();

    expect(last.nodes).toBe(first.nodes);
    expect(last.cache).toBeLessThanOrEqual(50);
  });

  test('setting up the component is cheap @perf', async ({ page }, testInfo) => {
    await open(page);

    const perInstance = await page.evaluate(() => {
      const Component = (window as any).FDCPAutocomplete;
      const element = document.getElementById('directory')!;
      const start = performance.now();
      // Constructing binds listeners again; these extra instances are discarded with the page.
      for (let index = 0; index < 200; index++) new Component(element);
      return (performance.now() - start) / 200;
    });

    testInfo.annotations.push({ type: 'timing', description: `${perInstance.toFixed(3)} ms per instance` });
    expect(perInstance).toBeLessThan(2);
  });
});
