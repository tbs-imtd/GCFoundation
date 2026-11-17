import type { Page, Route } from '@playwright/test';

export interface AutocompleteRecord {
  value: string;
  text: string;
  description: string;
}

const FIRST_NAMES = [
  'Marie', 'Jean', 'Zoé', 'Liam', 'Noah', 'Chloé', 'Amélie', 'Omar', 'Mei', 'Aiyana',
  'Étienne', 'Sophie', 'Lucas', 'Emma', 'Olivier', 'Léa', 'Priya', 'Daniel', 'Hélène', 'Mathieu'
];
const LAST_NAMES = [
  'Tremblay', 'Roy', 'Côté', 'Smith', 'Nguyen', 'Lévesque', 'Singh', 'Brown', 'Gagné', 'Wilson',
  'Bouchard', 'Martin', 'Pelletier', 'Lavoie', 'Fortin', 'Bélanger', 'Bergeron', 'Leblanc', 'Chen', 'Patel'
];
const TITLES = ['Analyst', 'Advisor', 'Director', 'Developer', 'Officer', 'Manager'];

/** Deterministic PRNG: the records are random-looking but identical on every run. */
export function mulberry32(seed: number): () => number {
  let state = seed >>> 0;
  return () => {
    state = (state + 0x6d2b79f5) >>> 0;
    let t = state;
    t = Math.imul(t ^ (t >>> 15), t | 1);
    t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

/** Generates `count` fictional people. */
export function generateRecords(count: number, seed = 1234): AutocompleteRecord[] {
  const random = mulberry32(seed);
  const pick = (list: string[]) => list[Math.floor(random() * list.length)];

  return Array.from({ length: count }, (_, index) => ({
    value: `M${String(index + 1).padStart(6, '0')}`,
    text: `${pick(FIRST_NAMES)} ${pick(LAST_NAMES)}`,
    description: `${pick(TITLES)} ${Math.floor(random() * 90) + 10}`
  }));
}

const fold = (value: string) => value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase()
  .replace(/[œæøßł]/g, letter => ({ 'œ': 'oe', 'æ': 'ae', 'ø': 'o', 'ß': 'ss', 'ł': 'l' } as Record<string, string>)[letter]).trim();

export interface MockOptions {
  /** Return every record whatever the term, to get totals in the thousands. */
  matchAll?: boolean;
  /** Milliseconds to wait before answering, per search term. */
  delays?: Record<string, number>;
  /** HTTP status to answer with instead of data. */
  failWith?: number;
}

export interface MockDataSource {
  /** One entry per request the component made. */
  requests: Array<{ term: string; skip: number; take: number; lang: string }>;
  options: MockOptions;
}

/**
 * Replaces the data source of one autocomplete (found by the id of its root element) with an in-memory mock.
 */
export async function mockDataSource(
  page: Page,
  rootId: string,
  records: AutocompleteRecord[],
  options: MockOptions = {}
): Promise<MockDataSource> {
  const src = await page.locator(`#${rootId}`).getAttribute('data-src');
  if (!src) throw new Error(`#${rootId} has no data-src`);

  const mock: MockDataSource = { requests: [], options };
  const folded = records.map(record => fold(record.text));

  await page.route(url => url.pathname === new URL(src, page.url()).pathname, async (route: Route) => {
    const params = new URL(route.request().url()).searchParams;
    const term = params.get('q') ?? '';
    const skip = Number(params.get('skip') ?? 0);
    const take = Number(params.get('take') ?? 50);

    mock.requests.push({ term, skip, take, lang: params.get('lang') ?? '' });

    const delay = mock.options.delays?.[term] ?? 0;
    if (delay > 0) await new Promise(resolve => setTimeout(resolve, delay));

    try {
      if (mock.options.failWith) {
        await route.fulfill({ status: mock.options.failWith, body: 'error' });
        return;
      }

      const needle = fold(term);
      const matches = mock.options.matchAll ? records : records.filter((_, index) => folded[index].includes(needle));

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ total: matches.length, items: matches.slice(skip, skip + take) })
      });
    } catch {
      // The component aborted this request (a newer term replaced it).
    }
  });

  return mock;
}
