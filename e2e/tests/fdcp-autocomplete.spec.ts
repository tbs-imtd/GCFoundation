import { test, expect, type Locator, type Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { generateRecords, mockDataSource } from '../utils/autocomplete-data';

const EN = '/en/components/autocomplete';
const FR = '/fr/composants/saisie-semi-automatique';

/** Most rows the virtualized list may render: 6 visible + 2 x 5 overscan + the active row. */
const MAX_RENDERED_ROWS = 20;

interface Autocomplete {
  root: Locator;
  input: Locator;
  value: Locator;
  listbox: Locator;
  options: Locator;
  status: Locator;
  message: Locator;
  clear: Locator;
  error: Locator;
}

function autocomplete(page: Page, id: string): Autocomplete {
  const root = page.locator(`#${id}`);
  return {
    root,
    input: root.locator('[data-fdcp-autocomplete-input]'),
    value: root.locator('[data-fdcp-autocomplete-value]'),
    listbox: root.locator('[data-fdcp-autocomplete-listbox]'),
    options: root.locator('[role="option"]'),
    status: root.locator('[data-fdcp-autocomplete-status]'),
    message: root.locator('[data-fdcp-autocomplete-message]'),
    clear: root.locator('[data-fdcp-autocomplete-clear]'),
    error: root.locator('[data-fdcp-autocomplete-error]')
  };
}

async function open(page: Page, path = EN) {
  await page.goto(path);
  await expect(page.locator('#employee')).toHaveAttribute('data-fdcp-autocomplete-initialized', 'true');
}

/** Moves focus out of the component with the keyboard: the clear button is a tab stop when there is text. */
async function tabOut(page: Page, ac: Autocomplete) {
  await ac.input.press('Tab');
  if (await ac.clear.isVisible()) await page.keyboard.press('Tab');
}

interface ActiveOption {
  problem: string;
  posinset: number;
  text: string;
}

/** Reads the option aria-activedescendant points at, in one round trip to the page. */
function readActiveOption(ac: Autocomplete): Promise<ActiveOption> {
  return ac.root.evaluate(root => {
    const input = root.querySelector('[data-fdcp-autocomplete-input]')!;
    const id = input.getAttribute('aria-activedescendant');
    const option = id ? root.querySelector(`[id="${CSS.escape(id)}"]`) : null;
    const problem = !id ? 'no aria-activedescendant'
      : !option ? `#${id} is not in the component`
      : option.getAttribute('role') !== 'option' ? `#${id} is not an option`
      : option.getAttribute('aria-selected') !== 'true' ? `#${id} is not aria-selected`
      : root.querySelectorAll('[role="option"][aria-selected="true"]').length !== 1 ? 'more than one option is aria-selected'
      : '';

    return {
      problem,
      posinset: Number(option?.getAttribute('aria-posinset') ?? 0),
      text: option?.textContent ?? ''
    };
  });
}

/**
 * The element aria-activedescendant points at must exist, be an option, and be the only selected option.
 * Checked in one page evaluation per poll (usually the first), not one round trip per attribute, so long
 * keyboard tests stay fast when a full parallel run loads the machine.
 */
async function expectActiveDescendantResolves(ac: Autocomplete): Promise<ActiveOption> {
  let active: ActiveOption = { problem: 'not read', posinset: 0, text: '' };
  await expect.poll(async () => (active = await readActiveOption(ac)).problem).toBe('');
  return active;
}

test.describe('FDCP autocomplete: demo data source (5,000 seeded records)', () => {
  test('finds accented names, selects with the keyboard and clears @core', async ({ page }) => {
    const cspViolations: string[] = [];
    page.on('console', message => {
      if (/content security policy/i.test(message.text())) cspViolations.push(message.text());
    });

    await open(page);
    const ac = autocomplete(page, 'employee');

    await expect(ac.input).toHaveAttribute('role', 'combobox');
    await expect(ac.input).toHaveAttribute('aria-expanded', 'false');

    await ac.input.fill('cote');
    await expect(ac.options.first()).toBeVisible();
    await expect(ac.input).toHaveAttribute('aria-expanded', 'true');
    await expect(ac.options.first()).toContainText('Côté');
    await expect(ac.status).toHaveText(/^\d+ results available$/);

    // Arrowing only moves the active option; nothing is committed until Enter.
    await ac.input.press('ArrowDown');
    await ac.input.press('ArrowDown');
    await expectActiveDescendantResolves(ac);
    await expect(ac.value).toHaveValue('');
    await expect(ac.input).toBeFocused();

    const activeText = await ac.root.locator('[role="option"][aria-selected="true"] .fdcp-autocomplete__option-text').innerText();
    await ac.input.press('Enter');

    await expect(ac.input).toHaveValue(activeText);
    await expect(ac.value).toHaveValue(/^E\d{6}$/);
    await expect(ac.input).toHaveAttribute('aria-expanded', 'false');
    await expect(ac.input).not.toHaveAttribute('aria-activedescendant', /.+/);
    await expect(ac.clear).toBeVisible();

    await ac.clear.click();
    await expect(ac.input).toHaveValue('');
    await expect(ac.value).toHaveValue('');
    await expect(ac.input).toBeFocused();
    await expect(ac.clear).toBeHidden();

    expect(cspViolations).toEqual([]);
  });

  test('selects with the pointer and keeps focus in the input @core', async ({ page }) => {
    await open(page);
    const ac = autocomplete(page, 'employee');

    await ac.input.fill('tremblay');
    const third = ac.options.nth(2);
    const text = await third.locator('.fdcp-autocomplete__option-text').innerText();
    await third.click();

    await expect(ac.input).toHaveValue(text);
    await expect(ac.value).toHaveValue(/^E\d{6}$/);
    await expect(ac.input).toBeFocused();
  });

  test('Escape closes the list, a second Escape clears the field @core', async ({ page }) => {
    await open(page);
    const ac = autocomplete(page, 'employee');

    await ac.input.fill('roy');
    await expect(ac.options.first()).toBeVisible();

    await ac.input.press('Escape');
    await expect(ac.listbox).toBeHidden();
    await expect(ac.input).toHaveValue('roy');

    await ac.input.press('Escape');
    await expect(ac.input).toHaveValue('');
  });

  test('Tab closes the list without selecting @core', async ({ page }) => {
    await open(page);
    const ac = autocomplete(page, 'employee');

    await ac.input.fill('roy');
    await ac.input.press('ArrowDown');
    await ac.input.press('Tab');

    await expect(ac.listbox).toBeHidden();
    await expect(ac.value).toHaveValue('');
  });

  test('text that was not picked from the list is an error @core', async ({ page }) => {
    await open(page);
    const ac = autocomplete(page, 'employee');

    await ac.input.fill('zzzzzz');
    await expect(ac.message).toHaveText('No results found');

    await tabOut(page, ac);

    await expect(ac.error).toBeVisible();
    await expect(ac.error).toHaveText('Select an option from the list.');
    await expect(ac.input).toHaveAttribute('aria-invalid', 'true');

    // Picking a real option clears the error.
    await ac.input.fill('roy');
    await ac.options.first().click();
    await expect(ac.error).toBeHidden();
    await expect(ac.input).not.toHaveAttribute('aria-invalid', 'true');
  });

  test('explains the minimum length instead of searching @core', async ({ page }) => {
    await open(page);
    const ac = autocomplete(page, 'employee');
    const requests: string[] = [];
    page.on('request', request => {
      if (request.url().includes('/autocomplete/data')) requests.push(request.url());
    });

    await ac.input.fill('a');
    await expect(ac.message).toHaveText('Type 2 or more characters to search');
    await page.waitForTimeout(400);

    expect(requests).toEqual([]);
  });

  test('shows an existing selection without calling the data source @core', async ({ page }) => {
    const requests: string[] = [];
    page.on('request', request => {
      if (request.url().includes('/autocomplete/data')) requests.push(request.url());
    });

    await open(page);
    const ac = autocomplete(page, 'manager');

    await expect(ac.value).toHaveValue('E000042');
    await expect(ac.input).not.toHaveValue('');
    await expect(ac.input).not.toHaveValue('E000042');
    await expect(ac.clear).toBeVisible();
    expect(requests).toEqual([]);

    // Editing the text drops the saved value: the field is strict.
    await ac.input.press('End');
    await ac.input.press('Backspace');
    await expect(ac.value).toHaveValue('');
  });

  test('required field blocks submit, feeds the error summary, then submits and resets @core', async ({ page }) => {
    await open(page);
    const ac = autocomplete(page, 'approver');
    const form = page.locator('form.autocomplete-required-example');

    await expect(ac.input).toHaveAttribute('aria-required', 'true');
    // GCDS buttons only submit once their web component has hydrated.
    await expect(form.locator('gcds-button[type="submit"]')).toHaveClass(/hydrated/);
    await expect(form.locator('gcds-error-summary')).toHaveClass(/hydrated/);
    await form.locator('gcds-button[type="submit"]').click();

    await expect(ac.error).toHaveText('This field is required.');
    await expect(ac.input).toHaveAttribute('aria-invalid', 'true');
    expect(page.url()).not.toContain('approver=');

    const summaryLink = form.locator('gcds-error-summary').getByRole('link', { name: 'Approver: This field is required.' });
    await expect(summaryLink).toBeVisible();
    await summaryLink.click();
    await expect(ac.input).toBeFocused();

    await ac.input.fill('singh');
    await ac.input.press('ArrowDown');
    await ac.input.press('Enter');
    await expect(ac.error).toBeHidden();
    const selected = await ac.value.inputValue();

    await form.locator('gcds-button[type="reset"]').click();
    await expect(ac.input).toHaveValue('');
    await expect(ac.value).toHaveValue('');

    await ac.input.fill('singh');
    await ac.input.press('ArrowDown');
    await ac.input.press('Enter');
    await form.locator('gcds-button[type="submit"]').click();
    await page.waitForURL(url => url.searchParams.get('approver') === selected);
  });

  test('French page uses French text and asks the data source for French @core @i18n', async ({ page }) => {
    await open(page, FR);
    const ac = autocomplete(page, 'employee');

    await expect(ac.root).toHaveAttribute('lang', 'fr');
    await expect(ac.clear).toHaveAttribute('aria-label', 'Effacer la sélection');

    await ac.input.fill('a');
    await expect(ac.message).toHaveText('Saisissez au moins 2 caractères pour lancer la recherche');

    const request = page.waitForRequest(r => r.url().includes('lang=fr') && r.url().includes('q=levesque'));
    await ac.input.fill('levesque');
    await request;

    await expect(ac.options.first()).toContainText('Lévesque');
    await expect(ac.status).toHaveText(/^\d+ résultats disponibles$/);

    await ac.input.fill('zzzzzz');
    await expect(ac.message).toHaveText('Aucun résultat trouvé');
    await tabOut(page, ac);
    await expect(ac.error).toHaveText('Sélectionnez une option dans la liste.');
  });
});

test.describe('FDCP autocomplete: bound to a model with for', () => {
  test('takes name, id, value, label, hint and required from the model @core', async ({ page }) => {
    await open(page);
    const ac = autocomplete(page, 'SupervisorId');

    await expect(ac.value).toHaveAttribute('name', 'SupervisorId');
    await expect(ac.value).toHaveValue('E000042');
    await expect(ac.input).toHaveValue('Samuel Leblanc');
    await expect(page.locator('label[for="SupervisorId_input"]')).toContainText('Supervisor');
    await expect(ac.root.locator('gcds-hint')).toContainText('approves this employee');
    await expect(ac.input).toHaveAttribute('aria-required', 'true');

    // Clearing the bound value then leaving the field shows the required error from [Required].
    await ac.clear.click();
    await expect(ac.value).toHaveValue('');
    await tabOut(page, ac);
    await expect(ac.error).toBeVisible();
  });

  test('French page uses the French [Display] resources', async ({ page }) => {
    await open(page, FR);
    await expect(page.locator('label[for="SupervisorId_input"]')).toContainText('Superviseur');
    await expect(page.locator('#SupervisorId gcds-hint')).toContainText('approuve les congés');
  });
});

test.describe('FDCP autocomplete: all properties sample', () => {
  test('every property reaches the browser and changes behaviour @core', async ({ page }) => {
    const requests: URL[] = [];
    page.on('request', request => {
      if (request.url().includes('/autocomplete/data')) requests.push(new URL(request.url()));
    });

    await open(page);
    const ac = autocomplete(page, 'delegatedAuthority');

    // Configuration passed through as written.
    await expect(ac.root).toHaveAttribute('data-query-param', 'term');
    await expect(ac.root).toHaveAttribute('data-min-length', '3');
    await expect(ac.root).toHaveAttribute('data-debounce', '300');
    await expect(ac.root).toHaveAttribute('data-page-size', '25');
    await expect(ac.root).toHaveAttribute('data-max-results', '500');
    await expect(ac.root).toHaveAttribute('data-cache', 'true');
    await expect(ac.root).toHaveAttribute('data-cache-ttl', '120');
    await expect(ac.root).toHaveAttribute('data-cache-size', '20');
    await expect(ac.value).toHaveAttribute('name', 'delegatedAuthorityId');
    await expect(ac.input).toHaveAttribute('placeholder', 'For example, Tremblay');
    await expect(ac.input).toHaveAttribute('aria-required', 'true');
    await expect(page.locator('label[for="delegatedAuthority_input"]')).toContainText('Delegated authority');
    await expect(ac.root.locator('gcds-hint')).toContainText('Search by first or last name');
    await expect(ac.clear).toHaveAttribute('aria-label', 'Clear the delegated authority');
    await expect(page.locator('#delegatedAuthority_instructions')).toHaveText(/Use the up and down arrow keys to review employees/);

    // min-length and min-length-text: two letters make no request.
    await ac.input.fill('tr');
    await expect(ac.message).toHaveText('Type 3 or more letters of a name');
    expect(requests).toEqual([]);

    // query-param and page-size reach the data source; multiple-results-text is used.
    await ac.input.fill('trem');
    await expect(ac.options.first()).toBeVisible();
    expect(requests).toHaveLength(1);
    expect(requests[0].searchParams.get('term')).toBe('trem');
    expect(requests[0].searchParams.has('q')).toBe(false);
    expect(requests[0].searchParams.get('take')).toBe('25');
    await expect(ac.status).toHaveText(/^\d+ employees found$/);

    // no-results-text.
    await ac.input.fill('zzzqx');
    await expect(ac.message).toHaveText('No employee matches that name');

    // load-error-text.
    await page.route('**/autocomplete/data?*term=fail*', route => route.fulfill({ status: 500, body: '' }));
    await ac.input.fill('failing');
    await expect(ac.message).toHaveText('The directory is not available. Try again in a moment.');

    // cache: going back to a term already searched makes no new request.
    const before = requests.length;
    await ac.input.fill('trem');
    await expect(ac.options.first()).toBeVisible();
    expect(requests.length).toBe(before);

    // required with no selection.
    await ac.input.fill('');
    await tabOut(page, ac);
    await expect(ac.error).toBeVisible();
  });

  test('value, selected-text and disabled show a fixed choice @core', async ({ page }) => {
    await open(page);
    const ac = autocomplete(page, 'approvingManager');

    await expect(ac.value).toHaveValue('E000042');
    await expect(ac.input).not.toHaveValue('');
    await expect(ac.input).toBeDisabled();
    await expect(ac.value).toBeDisabled();
    await expect(ac.clear).toBeDisabled();
  });

  test('French page uses the French overrides', async ({ page }) => {
    await open(page, FR);
    const ac = autocomplete(page, 'delegatedAuthority');

    await expect(page.locator('label[for="delegatedAuthority_input"]')).toContainText('Délégation de pouvoirs');
    await ac.input.fill('tr');
    await expect(ac.message).toHaveText('Saisissez au moins 3 lettres d’un nom');
  });
});

test.describe('FDCP autocomplete: mocked data source with thousands of random records', () => {
  test('renders a bounded number of rows for 5,000 results and pages on scroll @core', async ({ page }) => {
    await open(page);
    const records = generateRecords(10000);
    const mock = await mockDataSource(page, 'directory', records, { matchAll: true });
    const ac = autocomplete(page, 'directory');

    await ac.input.fill('an');
    await expect(ac.options.first()).toBeVisible();

    // The sample caps the list at 5,000 of the 10,000 matches.
    await expect(ac.options.first()).toHaveAttribute('aria-setsize', '5000');
    await expect(ac.options.first()).toHaveAttribute('aria-posinset', '1');
    await expect(ac.options.first()).toContainText(records[0].text);
    expect(await ac.options.count()).toBeLessThanOrEqual(MAX_RENDERED_ROWS);
    expect(mock.requests).toEqual([{ term: 'an', skip: 0, take: 100, lang: 'en' }]);

    // Jump to the end: only the page that holds the visible rows is requested.
    await ac.listbox.evaluate(element => { element.scrollTop = element.scrollHeight; });
    const last = ac.root.locator('[role="option"][aria-posinset="5000"]');
    await expect(last).toContainText(records[4999].text);
    expect(await ac.options.count()).toBeLessThanOrEqual(MAX_RENDERED_ROWS);
    expect(mock.requests.map(r => r.skip)).toEqual([0, 4900]);

    // Middle of the list.
    await ac.listbox.evaluate(element => { element.scrollTop = element.scrollHeight / 2; });
    await expect(ac.root.locator('[role="option"][data-loaded="true"]').first()).toBeVisible();
    expect(await ac.options.count()).toBeLessThanOrEqual(MAX_RENDERED_ROWS);
    expect(mock.requests.length).toBeLessThanOrEqual(4);

    const ids = await ac.root.locator('[id]').evaluateAll(elements => elements.map(element => element.id));
    expect(new Set(ids).size).toBe(ids.length);
  });

  // Each key press is followed by a single in-page check, and the reopen case is its own test, so the number of
  // round trips to the browser stays small: under a full parallel run each one can take a second or more.
  test('keyboard reaches deep rows and aria-activedescendant always resolves @core', async ({ page }) => {
    await open(page);
    const records = generateRecords(5000);
    await mockDataSource(page, 'directory', records, { matchAll: true });
    const ac = autocomplete(page, 'directory');

    await ac.input.fill('an');
    await expect(ac.options.first()).toBeVisible();

    await ac.input.press('ArrowDown');
    expect((await expectActiveDescendantResolves(ac)).posinset).toBe(1);

    let active = { posinset: 1, text: '' };
    for (let step = 0; step < 12; step++) {
      await ac.input.press('PageDown');
      const previous = active.posinset;
      active = await expectActiveDescendantResolves(ac);
      expect(active.posinset).toBeGreaterThan(previous);
    }

    expect(active.posinset).toBeGreaterThan(60);
    // The first page holds 100 rows, so this row is already loaded.
    expect(active.text).toContain(records[active.posinset - 1].text);

    await ac.input.press('Control+End');
    expect((await expectActiveDescendantResolves(ac)).posinset).toBe(5000);
    // The last row's page loads after the jump.
    await expect(ac.root.locator('[role="option"][aria-selected="true"]')).toContainText(records[4999].text);

    await ac.input.press('Enter');
    await expect(ac.value).toHaveValue(records[4999].value);
    await expect(ac.input).toHaveValue(records[4999].text);
  });

  test('Control+Home returns to the first row after reopening the list @core', async ({ page }) => {
    await open(page);
    const records = generateRecords(5000);
    await mockDataSource(page, 'directory', records, { matchAll: true });
    const ac = autocomplete(page, 'directory');

    await ac.input.fill('an');
    await expect(ac.options.first()).toBeVisible();
    await ac.input.press('ArrowDown');
    await ac.input.press('Control+End');
    await expect(ac.root.locator('[role="option"][aria-selected="true"]')).toContainText(records[4999].text);
    await ac.input.press('Enter');
    await expect(ac.value).toHaveValue(records[4999].value);

    // The same term again is served from the cache, starting at the top of the list.
    await ac.input.fill('an');
    await expect(ac.options.first()).toBeVisible();
    await ac.input.press('ArrowDown');
    await ac.input.press('Control+End');
    await ac.input.press('Control+Home');
    expect((await expectActiveDescendantResolves(ac)).posinset).toBe(1);
    expect(await ac.options.count()).toBeLessThanOrEqual(MAX_RENDERED_ROWS);
  });

  test('repeated searches are served from the cache @core', async ({ page }) => {
    await open(page);
    const mock = await mockDataSource(page, 'employee', generateRecords(5000));
    const ac = autocomplete(page, 'employee');

    await ac.input.fill('trem');
    await expect(ac.options.first()).toBeVisible();
    expect(mock.requests.length).toBe(1);

    await ac.input.fill('roy');
    await expect(ac.options.first()).toContainText('Roy');
    expect(mock.requests.length).toBe(2);

    // Same term again, in a different case and with extra spaces: no request.
    await ac.input.fill('  TREM ');
    await expect(ac.options.first()).toContainText('Tremblay');
    await page.waitForTimeout(400);
    expect(mock.requests.length).toBe(2);

    // Accents and case do not make a new search either.
    await ac.input.fill('cote');
    await expect(ac.options.first()).toContainText('Côté');
    await expect.poll(() => mock.requests.length).toBe(3);
    await ac.input.fill('CÔTÉ');
    await expect(ac.options.first()).toContainText('Côté');
    await page.waitForTimeout(400);
    expect(mock.requests.length).toBe(3);

    // An expired entry is fetched again.
    await ac.root.evaluate((element: any) => { element.FDCPAutocompleteInstance.cache.ttlMs = 0; });
    await ac.input.fill('roy');
    await expect(ac.options.first()).toContainText('Roy');
    await expect.poll(() => mock.requests.length).toBe(4);

    // With caching off every search goes to the source.
    await ac.root.evaluate((element: any) => { element.FDCPAutocompleteInstance.cacheEnabled = false; });
    await ac.input.fill('trem');
    await expect(ac.options.first()).toContainText('Tremblay');
    await expect.poll(() => mock.requests.length).toBe(5);
  });

  test('a slow, older response never replaces a newer one @core', async ({ page }) => {
    await open(page);
    const mock = await mockDataSource(page, 'employee', generateRecords(5000), { delays: { tr: 1200 } });
    const ac = autocomplete(page, 'employee');

    await ac.input.fill('tr');
    await expect.poll(() => mock.requests.length).toBe(1);
    await ac.input.fill('roy');

    await expect(ac.options.first()).toContainText('Roy');
    await page.waitForTimeout(1500);

    const texts = await ac.root.locator('.fdcp-autocomplete__option-text').allInnerTexts();
    expect(texts.length).toBeGreaterThan(0);
    expect(texts.every(text => text.includes('Roy'))).toBe(true);
  });

  test('reports a failing data source and recovers @core', async ({ page }) => {
    await open(page);
    const mock = await mockDataSource(page, 'employee', generateRecords(5000), { failWith: 500 });
    const ac = autocomplete(page, 'employee');
    const errorEvent = ac.root.evaluate(element => new Promise<boolean>(resolve => {
      element.addEventListener('fdcp-autocomplete:error', () => resolve(true), { once: true });
    }));

    await ac.input.fill('trem');
    await expect(ac.message).toHaveText('Unable to load results. Please try again.');
    await expect(ac.status).toHaveText('Unable to load results. Please try again.');
    await expect(ac.listbox).toBeHidden();
    expect(await errorEvent).toBe(true);

    mock.options.failWith = undefined;
    await ac.input.fill('tremb');
    await expect(ac.options.first()).toContainText('Tremblay');
    await expect(ac.message).toBeHidden();
  });

  test('renders hostile data as text, never as markup @core', async ({ page }) => {
    await open(page);
    const hostile = '<img src=x onerror="window.__fdcpXss = 1"><script>window.__fdcpXss = 1</script>';
    await mockDataSource(page, 'employee', [
      { value: hostile, text: hostile, description: hostile },
      { value: 'ok', text: 'Tremblay "quoted" & <b>bold</b>', description: '' }
    ], { matchAll: true });
    const ac = autocomplete(page, 'employee');

    await ac.input.fill('trem');
    await expect(ac.options.first()).toBeVisible();

    await expect(ac.root.locator('.fdcp-autocomplete__option-text').first()).toHaveText(hostile);
    expect(await ac.listbox.locator('img, script, b').count()).toBe(0);

    await ac.options.first().click();
    await expect(ac.input).toHaveValue(hostile);
    await expect(ac.value).toHaveValue(hostile);
    expect(await page.evaluate(() => (window as any).__fdcpXss)).toBeUndefined();
  });
});

test.describe('FDCP autocomplete: accessibility (WCAG 2.2 A, AA and AAA rules)', () => {
  // axe has to be injected into the page.
  test.use({ bypassCSP: true });

  const TAGS = ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa', 'wcag2aaa'];

  async function expectNoViolations(page: Page, selector: string, state: string) {
    const results = await new AxeBuilder({ page }).withTags(TAGS).include(selector).analyze();
    const summary = results.violations.map(v => `${v.id}: ${v.nodes.map(n => n.target.join(' ')).join(', ')}`);
    expect(summary, `axe violations (${state})`).toEqual([]);
  }

  for (const path of [EN, FR]) {
    test(`axe finds no violations when closed, open and invalid: ${path} @a11y`, async ({ page }) => {
      test.slow();
      await open(page, path);
      const ac = autocomplete(page, 'employee');

      await expectNoViolations(page, '#employee', 'closed');

      await ac.input.fill('tremblay');
      await expect(ac.options.first()).toBeVisible();
      await ac.input.press('ArrowDown');
      await expectNoViolations(page, '#employee', 'open with an active option');

      await ac.input.fill('zzzzzz');
      await expect(ac.message).toBeVisible();
      await expectNoViolations(page, '#employee', 'no results');

      await tabOut(page, ac);
      await expect(ac.error).toBeVisible();
      await expectNoViolations(page, '#employee', 'invalid');

      await expectNoViolations(page, '#manager', 'with a selection');
    });
  }

  test('targets are at least 44 by 44 CSS pixels @a11y', async ({ page }) => {
    await open(page);
    const ac = autocomplete(page, 'manager');

    const clear = await ac.clear.boundingBox();
    expect(clear!.width).toBeGreaterThanOrEqual(44);
    expect(clear!.height).toBeGreaterThanOrEqual(44);

    const input = await ac.input.boundingBox();
    expect(input!.height).toBeGreaterThanOrEqual(44);

    const employee = autocomplete(page, 'employee');
    await employee.input.fill('roy');
    await expect(employee.options.first()).toBeVisible();
    const option = await employee.options.first().boundingBox();
    expect(option!.height).toBeGreaterThanOrEqual(44);
  });

  test('active option is distinguishable without colour and in forced colours @a11y', async ({ page }) => {
    await open(page);
    const ac = autocomplete(page, 'employee');

    await ac.input.fill('roy');
    await ac.input.press('ArrowDown');
    const activeText = ac.root.locator('[role="option"][aria-selected="true"] .fdcp-autocomplete__option-text');
    const idleText = ac.root.locator('[role="option"][aria-selected="false"] .fdcp-autocomplete__option-text').first();

    await expect(activeText).toHaveCSS('text-decoration-line', 'underline');
    await expect(idleText).toHaveCSS('text-decoration-line', 'none');

    await page.emulateMedia({ forcedColors: 'active' });
    const active = ac.root.locator('[role="option"][aria-selected="true"]');
    const idle = ac.root.locator('[role="option"][aria-selected="false"]').first();
    const background = (locator: Locator) => locator.evaluate(element => getComputedStyle(element).backgroundColor);
    expect(await background(active)).not.toBe(await background(idle));
  });

  test('reflows at 320 CSS pixels and at 200% text size @a11y', async ({ page }) => {
    await page.setViewportSize({ width: 320, height: 640 });
    await open(page);
    const ac = autocomplete(page, 'employee');

    await ac.input.fill('tremblay');
    await expect(ac.options.first()).toBeVisible();

    for (const part of [ac.root, ac.root.locator('[data-fdcp-autocomplete-panel]')]) {
      const box = await part.boundingBox();
      expect(box!.x).toBeGreaterThanOrEqual(0);
      expect(box!.x + box!.width).toBeLessThanOrEqual(320);
    }

    // Row height follows the text size: rows are re-measured, not fixed in pixels.
    const before = (await ac.options.first().boundingBox())!.height;
    await page.evaluate(() => { document.documentElement.style.fontSize = '200%'; });
    await expect.poll(async () => (await ac.options.first().boundingBox())!.height).toBeGreaterThan(before * 1.5);

    const first = (await ac.options.nth(0).boundingBox())!;
    const second = (await ac.options.nth(1).boundingBox())!;
    expect(Math.abs(second.y - (first.y + first.height))).toBeLessThan(1.5);
  });

  test('the list never covers the input @a11y', async ({ page }) => {
    await page.setViewportSize({ width: 1024, height: 500 });
    await open(page);
    const ac = autocomplete(page, 'directory');

    await ac.input.scrollIntoViewIfNeeded();
    await ac.input.fill('an');
    await expect(ac.options.first()).toBeVisible();

    const input = (await ac.input.boundingBox())!;
    const panel = (await ac.root.locator('[data-fdcp-autocomplete-panel]').boundingBox())!;
    const overlaps = panel.y < input.y + input.height && panel.y + panel.height > input.y;
    expect(overlaps).toBe(false);
  });
});
