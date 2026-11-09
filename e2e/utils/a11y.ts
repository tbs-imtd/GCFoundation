import type { Page, TestInfo } from '@playwright/test';
import type axe from 'axe-core';
import AxeBuilder from '@axe-core/playwright';
import { promises as fs } from 'fs';

type AxeResults = axe.AxeResults;

/** axe may return a shadow-DOM selector array. Playwright and querySelector need a CSS string. */
function cssSelector(target: axe.CrossTreeSelector | undefined): string | undefined {
  return typeof target === 'string' && target.length > 0 ? target : undefined;
}

export async function runAxeAndAttach(
  page: Page,
  testInfo: TestInfo,
  tags: string[] = ['wcag2a', 'wcag2aa']
): Promise<AxeResults> {
  const results = await new AxeBuilder({ page }).withTags(tags).analyze();

  const jsonPath = testInfo.outputPath('axe-results.json');
  const mdPath = testInfo.outputPath('axe-summary.md');
  const htmlPath = testInfo.outputPath('axe-report.html');

  await fs.writeFile(jsonPath, JSON.stringify(results, null, 2), 'utf-8');
  await fs.writeFile(mdPath, generateMarkdown(results), 'utf-8');
  const html = await tryCreateHtmlReport(results);
  if (html) await fs.writeFile(htmlPath, html, 'utf-8');

  await testInfo.attach('axe-results.json', { path: jsonPath, contentType: 'application/json' });
  await testInfo.attach('axe-summary.md', { path: mdPath, contentType: 'text/markdown' });
  if (html) {
    await testInfo.attach('axe-report.html', { path: htmlPath, contentType: 'text/html' });
  }

  // Highlight serious/critical nodes on the page and capture screenshots to help locate issues
  const highlightSelectors: string[] = [];
  const seriousViolations = results.violations.filter((v: axe.Result) => v.impact === 'serious' || v.impact === 'critical');
  let perNodeCount = 0;
  for (const v of seriousViolations) {
    for (const [idx, n] of v.nodes.entries()) {
      const sel = cssSelector(n.target?.[0]);
      if (!sel) continue;
      highlightSelectors.push(sel);
      try {
        const loc = page.locator(sel).first();
        await loc.scrollIntoViewIfNeeded();
        const file = testInfo.outputPath(safeName(`axe-node-${v.id}-${idx}.png`));
        await loc.screenshot({ path: file });
        await testInfo.attach(`axe-node-${v.id}-${idx}.png`, { path: file, contentType: 'image/png' });
        perNodeCount++;
        if (perNodeCount >= 10) break; // avoid too many attachments
      } catch {
        // ignore screenshot failures for tricky selectors
      }
    }
    if (perNodeCount >= 10) break;
  }

  if (highlightSelectors.length > 0) {
    try {
      await page.evaluate((sels: string[]) => {
        sels.forEach(sel => {
          document.querySelectorAll<HTMLElement>(sel).forEach(el => {
            el.style.outline = '3px solid #e11';
            el.style.outlineOffset = '-2px';
            el.setAttribute('data-axe-highlight', '');
          });
        });
      }, highlightSelectors);

      const overview = testInfo.outputPath('axe-overview.png');
      await page.screenshot({ path: overview, fullPage: true });
      await testInfo.attach('axe-overview.png', { path: overview, contentType: 'image/png' });

      await page.evaluate(() => {
        document.querySelectorAll<HTMLElement>('[data-axe-highlight]').forEach(el => {
          el.style.outline = '';
          el.style.outlineOffset = '';
          el.removeAttribute('data-axe-highlight');
        });
      });
    } catch {
      // best-effort highlighting
    }
  }

  return results;
}

function generateMarkdown(results: AxeResults): string {
  const lines: string[] = [];
  lines.push(`# Axe Accessibility Summary`);
  lines.push(`Violations: ${results.violations.length}`);
  lines.push('');
  for (const v of results.violations) {
    lines.push(`## ${v.id} (${v.impact ?? 'unknown'})`);
    lines.push(v.description || '');
    if (v.helpUrl) lines.push(`Help: ${v.helpUrl}`);
    lines.push(`Affected nodes: ${v.nodes.length}`);
    const sampleNodes = v.nodes.slice(0, 5);
    for (const n of sampleNodes) {
      lines.push('- Selector: ' + (cssSelector(n.target?.[0]) ?? '(unknown)'));
      if (n.failureSummary) lines.push('  ' + n.failureSummary.replace(/\n/g, ' '));
    }
    lines.push('');
  }
  return lines.join('\n');
}

async function tryCreateHtmlReport(results: AxeResults): Promise<string | undefined> {
  try {
    // Avoid static analysis by using eval('require')
    // eslint-disable-next-line no-eval
    const req: NodeRequire | undefined = (eval('require') as NodeRequire | undefined);
    if (!req) return undefined;
    const mod = req('axe-html-reporter');
    if (mod && typeof mod.createHtmlReport === 'function') {
      return mod.createHtmlReport({ results });
    }
  } catch {
    // Module not installed; skip HTML report gracefully
  }
  return undefined;
}

function safeName(input: string): string {
  return input.replace(/[^a-z0-9._-]+/gi, '_');
}


