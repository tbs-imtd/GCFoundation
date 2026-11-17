import { test, expect, type Locator, type Page, type Request } from '@playwright/test';
import * as fs from 'fs';
import * as path from 'path';

/**
 * Evidence that every data source sample on the Autocomplete page works in a real browser, against the
 * running GCFoundation.Web app (no mocks): Web API, database, GraphQL, an existing API with a different
 * shape, and data already on the page.
 *
 * Set FDCP_EVIDENCE_DIR to also save a screenshot of each sample in use.
 */

const EN = '/en/components/autocomplete';
const FR = '/fr/composants/saisie-semi-automatique';
const EVIDENCE_DIR = process.env.FDCP_EVIDENCE_DIR;

interface Source {
  /** id of the component's root element */
  id: string;
  name: string;
  /** substring of the request URL the sample must call */
  endpoint: string;
  method: 'GET' | 'POST';
  /** reads the search term back out of a request, proving the request carried it */
  termOf: (request: Request) => string | null | undefined;
  /** reads the paging offset back out of a request */
  skipOf: (request: Request) => number;
  pageSize: number;
}

const query = (request: Request) => new URL(request.url()).searchParams;

const SERVER_SOURCES: Source[] = [
  {
    id: 'employee',
    name: 'Web API',
    endpoint: '/autocomplete/data',
    method: 'GET',
    termOf: request => query(request).get('q'),
    skipOf: request => Number(query(request).get('skip')),
    pageSize: 50
  },
  {
    id: 'databaseEmployee',
    name: 'Database',
    endpoint: '/autocomplete/sources/database',
    method: 'GET',
    termOf: request => query(request).get('q'),
    skipOf: request => Number(query(request).get('skip')),
    pageSize: 50
  },
  {
    id: 'graphqlEmployee',
    name: 'GraphQL',
    endpoint: '/autocomplete/sources/graphql',
    method: 'POST',
    termOf: request => request.postDataJSON()?.variables?.term,
    skipOf: request => Number(request.postDataJSON()?.variables?.skip),
    pageSize: 50
  },
  {
    id: 'legacyEmployee',
    name: 'Existing API',
    endpoint: '/autocomplete/sources/legacy',
    method: 'GET',
    termOf: request => query(request).get('search'),
    skipOf: request => (Number(query(request).get('page')) - 1) * Number(query(request).get('pageSize')),
    pageSize: 25
  }
];

function parts(page: Page, id: string) {
  const root = page.locator(`#${id}`);
  return {
    root,
    input: root.locator('[data-fdcp-autocomplete-input]'),
    value: root.locator('[data-fdcp-autocomplete-value]'),
    listbox: root.locator('[data-fdcp-autocomplete-listbox]'),
    options: root.locator('[role="option"]'),
    texts: root.locator('.fdcp-autocomplete__option-text'),
    descriptions: root.locator('.fdcp-autocomplete__option-description'),
    status: root.locator('[data-fdcp-autocomplete-status]'),
    message: root.locator('[data-fdcp-autocomplete-message]')
  };
}

async function open(page: Page, url = EN) {
  await page.goto(url);
  await expect(page.locator('#localProvince')).toHaveAttribute('data-fdcp-autocomplete-initialized', 'true');
}

/** Scrolls a sample to the top of the window, so its list has room to open below it. */
async function toTop(root: Locator) {
  await root.evaluate(element => element.scrollIntoView({ block: 'start' }));
}

/** Records the requests a sample makes to its endpoint. */
function watch(page: Page, source: Source): Request[] {
  const requests: Request[] = [];
  page.on('request', request => {
    if (request.url().includes(source.endpoint)) requests.push(request);
  });
  return requests;
}

/** Screenshots a component together with its open list, which is positioned outside the component's own box. */
async function evidence(page: Page, id: string, name: string, testInfo: { attach: Function }) {
  const root = page.locator(`#${id}`);
  const boxes = [await root.boundingBox(), await root.locator('[data-fdcp-autocomplete-panel]').boundingBox()].filter(Boolean) as Array<{ x: number; y: number; width: number; height: number }>;
  const left = Math.max(0, Math.min(...boxes.map(b => b.x)) - 16);
  const top = Math.max(0, Math.min(...boxes.map(b => b.y)) - 16);
  const right = Math.max(...boxes.map(b => b.x + b.width)) + 16;
  const bottom = Math.max(...boxes.map(b => b.y + b.height)) + 16;
  const image = await page.screenshot({ clip: { x: left, y: top, width: right - left, height: bottom - top } });
  await testInfo.attach(`${name}.png`, { body: image, contentType: 'image/png' });

  if (EVIDENCE_DIR) {
    fs.mkdirSync(EVIDENCE_DIR, { recursive: true });
    fs.writeFileSync(path.join(EVIDENCE_DIR, `${name}.png`), image);
  }
}

test.describe('FDCP autocomplete data sources: each sample works end to end', () => {
  for (const source of SERVER_SOURCES) {
    test(`${source.name}: search, page, select and submit a value @core`, async ({ page }, testInfo) => {
      const cspViolations: string[] = [];
      page.on('console', message => {
        if (/content security policy/i.test(message.text())) cspViolations.push(message.text());
      });

      await open(page);
      const requests = watch(page, source);
      const ac = parts(page, source.id);

      // 1. Typing calls this sample's endpoint, with the right verb, carrying the term.
      await toTop(ac.root);
      await ac.input.fill('tremblay');
      await expect(ac.options.first()).toBeVisible();

      expect(requests.length).toBe(1);
      expect(requests[0].method()).toBe(source.method);
      expect(source.termOf(requests[0])).toBe('tremblay');
      expect(source.skipOf(requests[0])).toBe(0);
      expect((await requests[0].response())!.status()).toBe(200);

      // 2. Results are rendered from the response.
      await expect(ac.texts.first()).toContainText('Tremblay');
      await expect(ac.descriptions.first()).not.toBeEmpty();
      await expect(ac.status).toHaveText(/^\d+ results available$/);
      const total = Number(await ac.options.first().getAttribute('aria-setsize'));
      expect(total).toBeGreaterThan(source.pageSize);

      await ac.input.press('ArrowDown');
      await evidence(page, source.id, `source-${source.id}`, testInfo);

      // 3. Scrolling to the end asks the source for the last page only.
      await ac.listbox.evaluate(element => { element.scrollTop = element.scrollHeight; });
      const last = ac.root.locator(`[role="option"][aria-posinset="${total}"][data-loaded="true"]`);
      await expect(last).toBeVisible();
      await expect(last).toContainText('Tremblay');

      const lastPageSkip = Math.floor((total - 1) / source.pageSize) * source.pageSize;
      expect(requests.map(source.skipOf)).toContain(lastPageSkip);
      expect(requests.every(request => request.method() === source.method)).toBe(true);
      expect(requests.every(request => source.termOf(request) === 'tremblay')).toBe(true);

      // 4. Selecting puts the record's value in the submitted field.
      const text = await last.locator('.fdcp-autocomplete__option-text').innerText();
      await last.click();
      await expect(ac.input).toHaveValue(text);
      await expect(ac.value).toHaveValue(/^E\d{6}$/);
      await expect(ac.value).toHaveAttribute('name', source.id);

      // 5. The same search again is answered from the cache.
      const before = requests.length;
      await ac.input.fill('tremblay');
      await expect(ac.options.first()).toBeVisible();
      await page.waitForTimeout(400);
      expect(requests.length).toBe(before);

      expect(cspViolations).toEqual([]);
    });
  }

  test('all four server sources show the same results for the same search @core', async ({ page }) => {
    await open(page);
    const seen: Record<string, { total: string | null; texts: string[]; descriptions: string[] }> = {};

    for (const source of SERVER_SOURCES) {
      const ac = parts(page, source.id);
      await ac.input.fill('côté');
      await expect(ac.options.first()).toBeVisible();

      seen[source.name] = {
        total: await ac.options.first().getAttribute('aria-setsize'),
        texts: (await ac.texts.allInnerTexts()).slice(0, 6),
        descriptions: (await ac.descriptions.allInnerTexts()).slice(0, 6)
      };
      await ac.input.press('Escape');
    }

    const reference = seen['Web API'];
    expect(Number(reference.total)).toBeGreaterThan(0);
    expect(reference.texts.length).toBe(6);

    for (const source of SERVER_SOURCES) {
      expect(seen[source.name], `${source.name} differs from the Web API`).toEqual(reference);
    }
  });

  test('all four server sources answer in French on the French page @core @i18n', async ({ page }) => {
    await open(page);
    const english: Record<string, string> = {};

    for (const source of SERVER_SOURCES) {
      const ac = parts(page, source.id);
      await ac.input.fill('lavoie');
      await expect(ac.options.first()).toBeVisible();
      english[source.id] = await ac.descriptions.first().innerText();
    }

    await open(page, FR);

    for (const source of SERVER_SOURCES) {
      const ac = parts(page, source.id);
      await ac.input.fill('lavoie');
      await expect(ac.options.first()).toBeVisible();

      // Same record, description in the other language.
      await expect(ac.texts.first()).toContainText('Lavoie');
      expect(await ac.descriptions.first().innerText(), source.name).not.toBe(english[source.id]);
      await expect(ac.status).toHaveText(/^\d+ résultats disponibles$/);
    }
  });

  test('GraphQL: posts a query document with variables and reads the data envelope @core', async ({ page }) => {
    await open(page);
    const source = SERVER_SOURCES[2];
    const requests = watch(page, source);
    const ac = parts(page, source.id);

    await ac.input.fill('singh');
    await expect(ac.options.first()).toBeVisible();

    const request = requests[0];
    const body = request.postDataJSON();

    expect(request.headers()['content-type']).toContain('application/json');
    expect(body.query).toContain('query Employees(');
    expect(body.query).toContain('employees(term: $term, skip: $skip, take: $take, lang: $lang)');
    expect(body.variables).toEqual({ term: 'singh', skip: 0, take: 50, lang: 'en' });

    const response = await (await request.response())!.json();
    expect(Object.keys(response)).toEqual(['data']);
    expect(response.data.employees.items.length).toBeGreaterThan(0);
    expect(response.data.employees.items[0].text).toContain('Singh');
    await expect(ac.texts.first()).toHaveText(response.data.employees.items[0].text);
  });

  test('GraphQL: a GraphQL error is shown as a load error, then recovers @core', async ({ page }) => {
    await open(page);
    const source = SERVER_SOURCES[2];
    const ac = parts(page, source.id);
    let fail = true;

    // GraphQL reports failures with HTTP 200 and an "errors" array.
    await page.route(url => url.pathname.endsWith(source.endpoint), async route => {
      if (fail) {
        await route.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify({ errors: [{ message: 'Field "employees" failed' }] }) });
      } else {
        await route.continue();
      }
    });

    await ac.input.fill('singh');
    await expect(ac.message).toHaveText('Unable to load results. Please try again.');
    await expect(ac.listbox).toBeHidden();

    fail = false;
    await ac.input.fill('singh ');
    await ac.input.fill('sing');
    await expect(ac.texts.first()).toContainText('Singh');
  });

  test('Existing API: translates skip to a page number and maps the response fields @core', async ({ page }) => {
    await open(page);
    const source = SERVER_SOURCES[3];
    const requests = watch(page, source);
    const ac = parts(page, source.id);

    await ac.input.fill('roy');
    await expect(ac.options.first()).toBeVisible();

    const first = query(requests[0]);
    expect(first.get('search')).toBe('roy');
    expect(first.get('page')).toBe('1');
    expect(first.get('pageSize')).toBe('25');
    expect(first.has('skip')).toBe(false);
    expect(first.has('q')).toBe(false);

    const body = await (await requests[0].response())!.json();
    expect(Object.keys(body).sort()).toEqual(['count', 'page', 'pageSize', 'results']);
    expect(Object.keys(body.results[0]).sort()).toEqual(['displayName', 'id', 'titleEn', 'titleFr']);
    await expect(ac.texts.first()).toHaveText(body.results[0].displayName);
    await expect(ac.descriptions.first()).toHaveText(body.results[0].titleEn);
    await expect(ac.options.first()).toHaveAttribute('aria-setsize', String(body.count));

    // Ten screens down is row 61, on the API's page 3.
    await ac.input.press('ArrowDown');
    for (let press = 0; press < 10; press++) await ac.input.press('PageDown');
    await expect(ac.root.locator('[role="option"][aria-selected="true"]')).toHaveAttribute('aria-posinset', '61');
    await expect(ac.root.locator('[role="option"][aria-selected="true"][data-loaded="true"]')).toBeVisible();
    expect(requests.map(request => query(request).get('page'))).toEqual(['1', '2', '3']);

    await ac.input.press('Enter');
    await expect(ac.value).toHaveValue(/^E\d{6}$/);
  });

  test('Data on the page: filters locally with no request, in both languages @core @i18n', async ({ page }, testInfo) => {
    const requests: string[] = [];
    await open(page);
    page.on('request', request => requests.push(request.url()));
    const ac = parts(page, 'localProvince');

    await ac.input.fill('n');
    await expect(ac.options.first()).toBeVisible();
    await expect(ac.texts).toHaveText([
      'Manitoba', 'New Brunswick', 'Newfoundland and Labrador', 'Nova Scotia', 'Northwest Territories',
      'Nunavut', 'Ontario', 'Prince Edward Island', 'Saskatchewan', 'Yukon'
    ]);
    await expect(ac.status).toHaveText('10 results available');

    await toTop(ac.root);
    await ac.input.fill('que');
    await expect(ac.texts).toHaveText(['Quebec']);
    await expect(ac.status).toHaveText('1 result available');
    await ac.input.press('ArrowDown');
    await evidence(page, 'localProvince', 'source-localProvince', testInfo);
    await ac.input.press('Enter');

    await expect(ac.input).toHaveValue('Quebec');
    await expect(ac.value).toHaveValue('QC');

    await ac.input.fill('zz');
    await expect(ac.message).toHaveText('No results found');
    expect(requests).toEqual([]);

    // French page: French names, and accents are ignored when matching.
    await open(page, FR);
    const fr = parts(page, 'localProvince');
    await fr.input.fill('ile');
    await expect(fr.texts).toHaveText(['Île-du-Prince-Édouard']);
    await fr.options.first().click();
    await expect(fr.input).toHaveValue('Île-du-Prince-Édouard');
    await expect(fr.value).toHaveValue('PE');
  });

  test('the code shown for each sample is the code the page runs @core', async ({ page }) => {
    await open(page);

    for (const id of ['graphqlEmployee', 'legacyEmployee', 'localProvince']) {
      const running = await page.evaluate(marker =>
        Array.from(document.scripts).map(script => script.textContent ?? '').find(text => text.includes(`'${marker}'`)) ?? '', id);
      const shown = await page.evaluate(marker =>
        Array.from(document.querySelectorAll('fdcp-code, [data-fdcp-code-ready]'))
          .map(element => element.textContent ?? '').find(text => text.includes(`'${marker}'`)) ?? '', id);
      const squash = (text: string) => text.replace(/\s+/g, ' ').trim();

      expect(squash(running).length, `${id} script`).toBeGreaterThan(200);
      expect(squash(shown), `${id} sample`).toContain(squash(running));
    }
  });
});
