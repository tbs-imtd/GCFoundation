// Unit tests for the DOM-free parts of fdcp-autocomplete. Run with: npm test
import assert from 'node:assert/strict';
import test from 'node:test';
import { PAYLOADS, fakePeople, loadComponent, seededFaker } from './helpers.js';

const {
    FDCPAutocomplete,
    FDCPAutocompleteCache,
    fdcpAutocompleteWindow,
    fdcpAutocompleteScrollTopForIndex,
    fdcpAutocompleteNormalizeResult
} = loadComponent();

/* ---------- Cache ---------- */

test('cache: miss then hit', () => {
    const cache = new FDCPAutocompleteCache();

    assert.equal(cache.get('k'), undefined);
    cache.set('k', { total: 1 });
    assert.deepEqual(cache.get('k'), { total: 1 });
});

test('cache: key normalizes the term and separates language and page', () => {
    assert.equal(FDCPAutocompleteCache.key('en', '  Marie   TREMBLAY ', 0), 'en|marie tremblay|0');
    assert.notEqual(FDCPAutocompleteCache.key('en', 'roy', 0), FDCPAutocompleteCache.key('fr', 'roy', 0));
    assert.notEqual(FDCPAutocompleteCache.key('en', 'roy', 0), FDCPAutocompleteCache.key('en', 'roy', 50));
});

test('cache: key ignores accents and spells out special letters', () => {
    const key = term => FDCPAutocompleteCache.key('fr', term, 0);

    assert.equal(key('Côté'), 'fr|cote|0');
    assert.equal(key('CÔTÉ'), key('cote'));
    assert.equal(key('  Lévesque   Noël '), 'fr|levesque noel|0');
    assert.equal(key('Cœur'), key('coeur'));
    assert.equal(key('ŒUVRE'), key('oeuvre'));
    assert.equal(key('Lætitia'), key('laetitia'));
    assert.equal(key('Søren Ødegård'), key('soren odegard'));
    assert.equal(key('Straße'), key('STRASSE'));
    assert.equal(key('\u1E9E'), key('ss'));
    assert.equal(key('Łukasz Wałęsa'), key('lukasz walesa'));
    assert.equal(key('Çelik Ñúñez'), key('celik nunez'));

    // Folding must not merge different searches.
    assert.notEqual(key('cote'), key('cotes'));
    assert.notEqual(key('roy'), key('ray'));
});

test('cache: a search with accents is served from the entry without them', async () => {
    const cache = new FDCPAutocompleteCache();
    let calls = 0;
    const loader = () => { calls += 1; return Promise.resolve({ total: 1, items: [] }); };

    await cache.fetch(FDCPAutocompleteCache.key('fr', 'cote', 0), loader);
    await cache.fetch(FDCPAutocompleteCache.key('fr', 'Côté', 0), loader);
    await cache.fetch(FDCPAutocompleteCache.key('fr', 'CÔTÉ', 0), loader);

    assert.equal(calls, 1);
});

test('cache: evicts the least recently used entry', () => {
    const cache = new FDCPAutocompleteCache({ maxEntries: 2 });

    cache.set('a', 1);
    cache.set('b', 2);
    cache.get('a');
    cache.set('c', 3);

    assert.equal(cache.size, 2);
    assert.equal(cache.get('b'), undefined);
    assert.equal(cache.get('a'), 1);
    assert.equal(cache.get('c'), 3);
});

test('cache: entries expire after the TTL (mocked clock)', () => {
    let clock = 1000;
    const cache = new FDCPAutocompleteCache({ ttlMs: 500, now: () => clock });

    cache.set('k', 'v');
    clock += 499;
    assert.equal(cache.get('k'), 'v');
    clock += 1;
    assert.equal(cache.get('k'), undefined);
    assert.equal(cache.size, 0);
});

test('cache: fetch calls the loader once and serves repeats from memory', async () => {
    const cache = new FDCPAutocompleteCache();
    let calls = 0;
    const loader = () => { calls += 1; return Promise.resolve({ total: 3 }); };

    assert.deepEqual(await cache.fetch('k', loader), { total: 3 });
    assert.deepEqual(await cache.fetch('k', loader), { total: 3 });
    assert.equal(calls, 1);
});

test('cache: identical in-flight requests share one loader call', async () => {
    const cache = new FDCPAutocompleteCache();
    let calls = 0;
    let release;
    const loader = () => { calls += 1; return new Promise(resolve => { release = resolve; }); };

    const first = cache.fetch('k', loader);
    const second = cache.fetch('k', loader);
    await Promise.resolve();
    release('done');

    assert.deepEqual(await Promise.all([first, second]), ['done', 'done']);
    assert.equal(calls, 1);
});

test('cache: failures are not cached', async () => {
    const cache = new FDCPAutocompleteCache();
    let calls = 0;
    const loader = () => { calls += 1; return calls === 1 ? Promise.reject(new Error('boom')) : Promise.resolve('ok'); };

    await assert.rejects(cache.fetch('k', loader), /boom/);
    assert.equal(await cache.fetch('k', loader), 'ok');
    assert.equal(calls, 2);
});

test('cache: forgetPending lets an aborted request be retried', async () => {
    const cache = new FDCPAutocompleteCache();
    let calls = 0;
    const never = () => { calls += 1; return new Promise(() => { }); };

    cache.fetch('k', never);
    cache.forgetPending();
    const retry = cache.fetch('k', () => { calls += 1; return Promise.resolve('fresh'); });

    assert.equal(await retry, 'fresh');
    assert.equal(calls, 2);
});

test('cache: clear empties everything', () => {
    const cache = new FDCPAutocompleteCache();

    cache.set('a', 1);
    cache.clear();
    assert.equal(cache.size, 0);
});

/* ---------- Virtual window ---------- */

test('window: start of a 10,000-row list renders only viewport plus overscan', () => {
    const view = fdcpAutocompleteWindow({ total: 10000, rowHeight: 60, scrollTop: 0, viewportHeight: 360, overscan: 5 });

    assert.deepEqual(view, { start: 0, end: 11, offsetTop: 0, totalHeight: 600000 });
});

test('window: middle of the list', () => {
    const view = fdcpAutocompleteWindow({ total: 10000, rowHeight: 60, scrollTop: 60 * 5000, viewportHeight: 360, overscan: 5 });

    assert.equal(view.start, 4995);
    assert.equal(view.end, 5011);
    assert.equal(view.offsetTop, 4995 * 60);
});

test('window: end of the list never runs past the total', () => {
    const view = fdcpAutocompleteWindow({ total: 10000, rowHeight: 60, scrollTop: 60 * 10000, viewportHeight: 360, overscan: 5 });

    assert.equal(view.end, 10000);
    assert.equal(view.start, 9994);
});

test('window: rendered row count is bounded whatever the total', () => {
    for (const total of [1, 50, 1000, 10000, 1000000]) {
        for (const scrollTop of [0, 1234, total * 30, total * 60]) {
            const view = fdcpAutocompleteWindow({ total, rowHeight: 60, scrollTop, viewportHeight: 360, overscan: 5 });

            assert.ok(view.end - view.start <= 16, `total=${total} scrollTop=${scrollTop}`);
            assert.ok(view.start >= 0 && view.end <= total);
        }
    }
});

test('window: empty list and unmeasured rows produce an empty window', () => {
    const empty = { start: 0, end: 0, offsetTop: 0, totalHeight: 0 };

    assert.deepEqual(fdcpAutocompleteWindow({ total: 0, rowHeight: 60, scrollTop: 0, viewportHeight: 360 }), empty);
    assert.deepEqual(fdcpAutocompleteWindow({ total: 10, rowHeight: 0, scrollTop: 0, viewportHeight: 360 }), empty);
});

test('scrollTopForIndex: moves the least distance needed', () => {
    const base = { rowHeight: 60, viewportHeight: 360 };

    assert.equal(fdcpAutocompleteScrollTopForIndex({ ...base, index: 2, scrollTop: 0 }), 0);
    assert.equal(fdcpAutocompleteScrollTopForIndex({ ...base, index: 6, scrollTop: 0 }), 60);
    assert.equal(fdcpAutocompleteScrollTopForIndex({ ...base, index: 1, scrollTop: 300 }), 60);
    assert.equal(fdcpAutocompleteScrollTopForIndex({ ...base, index: 9999, scrollTop: 0 }), 9999 * 60 + 60 - 360);
});

/* ---------- Response normalization ---------- */

test('normalize: accepts the { total, items } shape', () => {
    const result = fdcpAutocompleteNormalizeResult({ total: 1342, items: [{ value: 1, text: 'Marie Tremblay', description: 'CIOB' }] });

    assert.deepEqual(result, { total: 1000, items: [{ value: '1', text: 'Marie Tremblay', description: 'CIOB' }] });
});

test('normalize: accepts a bare array as a single page', () => {
    const result = fdcpAutocompleteNormalizeResult([{ value: 'a', text: 'A' }]);

    assert.deepEqual(result, { total: 1, items: [{ value: 'a', text: 'A', description: '' }] });
});

test('normalize: rejects anything that is not a result', () => {
    for (const raw of [null, undefined, 'text', 42, {}, { items: 'nope' }]) {
        assert.throws(() => fdcpAutocompleteNormalizeResult(raw), TypeError);
    }
});

test('normalize: drops malformed items and coerces types', () => {
    const result = fdcpAutocompleteNormalizeResult({
        total: 'not a number',
        items: [
            null,
            'string',
            { value: 'x' },
            { text: 'no value' },
            { value: '', text: 'empty value' },
            { value: 'ok', text: '   ' },
            { value: { nested: true }, text: 'object value' },
            { value: 7, text: 8, description: { html: '<b>' } }
        ]
    });

    assert.deepEqual(result.items, [{ value: '7', text: '8', description: '' }]);
    assert.equal(result.total, 8);
});

test('normalize: hostile strings stay plain strings', () => {
    const hostile = '<img src=x onerror=alert(1)>';
    const result = fdcpAutocompleteNormalizeResult([{ value: hostile, text: hostile, description: hostile }]);

    assert.deepEqual(result.items[0], { value: hostile, text: hostile, description: hostile });
});

test('normalize: caps 10,000 Faker records and over-long text', () => {
    const records = fakePeople(10000);
    records[0].text = 'x'.repeat(5000);

    const result = fdcpAutocompleteNormalizeResult({ total: 10000, items: records }, 1000);

    assert.equal(result.items.length, 1000);
    assert.equal(result.total, 1000);
    assert.equal(result.items[0].text.length, 500);
    assert.equal(result.items[999].value, 'E001000');
});

test('normalize: 5,000 Faker records pass through unchanged', () => {
    const records = fakePeople(5000);
    const result = fdcpAutocompleteNormalizeResult({ total: records.length, items: records }, 5000);

    assert.equal(result.total, 5000);
    assert.deepEqual(result.items, records);
    assert.ok(result.items.some(item => /[^\x00-\x7f]/.test(item.text)), 'data should include accented names');
    assert.equal(new Set(result.items.map(item => item.value)).size, 5000);
});

test('normalize: Faker records mixed with garbage keep only the valid ones, in order', () => {
    const faker = seededFaker();
    const good = fakePeople(500);
    const garbage = () => faker.helpers.arrayElement([
        null, undefined, 42, faker.lorem.word(), [], {},
        { value: faker.string.uuid() },
        { text: faker.person.fullName() },
        { value: '', text: faker.person.fullName() },
        { value: faker.string.uuid(), text: '   ' },
        { value: { id: 1 }, text: faker.person.fullName() },
        { value: faker.string.uuid(), text: [faker.person.fullName()] }
    ]);
    const mixed = [];

    for (const record of good) {
        if (faker.datatype.boolean()) mixed.push(garbage());
        mixed.push(record);
    }

    const result = fdcpAutocompleteNormalizeResult({ total: mixed.length, items: mixed }, 5000);

    assert.deepEqual(result.items, good);
});

test('normalize: payloads inside Faker text are kept as inert strings', () => {
    const faker = seededFaker();
    const records = fakePeople(200).map(record => ({
        value: record.value + faker.helpers.arrayElement(PAYLOADS),
        text: record.text + ' ' + faker.helpers.arrayElement(PAYLOADS),
        description: faker.helpers.arrayElement(PAYLOADS)
    }));

    const result = fdcpAutocompleteNormalizeResult(records);

    assert.deepEqual(result.items, records);
    assert.ok(result.items.every(item => typeof item.value === 'string' && typeof item.text === 'string' && typeof item.description === 'string'));
});

test('normalize: numbers from a data source become strings', () => {
    const faker = seededFaker();
    const records = Array.from({ length: 100 }, () => ({ value: faker.number.int({ min: 1, max: 999999 }), text: faker.number.int() }));

    const result = fdcpAutocompleteNormalizeResult(records);

    assert.deepEqual(result.items, records.map(record => ({ value: String(record.value), text: String(record.text), description: '' })));
});

test('cache: Faker terms differing only in case and spacing share one key', () => {
    const faker = seededFaker();

    for (let sample = 0; sample < 300; sample += 1) {
        const term = faker.person.lastName();

        assert.equal(FDCPAutocompleteCache.key('fr', '  ' + term.toUpperCase() + '  ', 0), FDCPAutocompleteCache.key('fr', term, 0));
        assert.equal(FDCPAutocompleteCache.key('fr', term + '   ' + term, 50), FDCPAutocompleteCache.key('fr', term + ' ' + term, 50));
    }
});

test('cache: holds the most recent pages of 2,000 Faker searches and nothing more', async () => {
    const faker = seededFaker();
    const cache = new FDCPAutocompleteCache({ maxEntries: 50 });
    const pages = fakePeople(2000);
    const keys = [];

    for (let index = 0; index < 2000; index += 1) {
        const key = FDCPAutocompleteCache.key('en', faker.person.lastName() + ' ' + index, 0);
        keys.push(key);
        await cache.fetch(key, () => Promise.resolve({ total: 1, items: [pages[index]] }));
        assert.ok(cache.size <= 50);
    }

    assert.equal(cache.size, 50);
    keys.slice(-50).forEach((key, offset) => assert.deepEqual(cache.get(key).items[0], pages[1950 + offset]));
    assert.equal(cache.get(keys[0]), undefined);
});

test('window: random scroll positions and list sizes always give a valid, bounded window', () => {
    const faker = seededFaker();

    for (let sample = 0; sample < 2000; sample += 1) {
        const total = faker.number.int({ min: 0, max: 100000 });
        const rowHeight = faker.number.int({ min: 20, max: 200 });
        const viewportHeight = faker.number.int({ min: rowHeight, max: 1200 });
        const scrollTop = faker.number.int({ min: 0, max: Math.max(0, total * rowHeight) });
        const overscan = faker.number.int({ min: 0, max: 10 });
        const view = fdcpAutocompleteWindow({ total, rowHeight, scrollTop, viewportHeight, overscan });
        const visible = Math.ceil(viewportHeight / rowHeight);

        assert.ok(view.start >= 0 && view.start <= view.end && view.end <= total);
        assert.ok(view.end - view.start <= visible + 2 * overscan + 1);
        assert.equal(view.totalHeight, total * rowHeight);
        assert.equal(view.offsetTop, view.start * rowHeight);

        if (total > 0) {
            // The row at the top of the viewport is always inside the window.
            const first = Math.min(total - 1, Math.floor(scrollTop / rowHeight));
            assert.ok(first >= view.start && first < view.end);

            // Scrolling to a random row brings it fully into view.
            const index = faker.number.int({ min: 0, max: total - 1 });
            const top = fdcpAutocompleteScrollTopForIndex({ index, rowHeight, scrollTop, viewportHeight });
            assert.ok(index * rowHeight >= top && (index + 1) * rowHeight <= top + viewportHeight);
        }
    }
});

/* ---------- Default data source (fetch mocked) ---------- */

function withMockedFetch(response, run) {
    const calls = [];
    const original = { fetch: globalThis.fetch, document: globalThis.document };

    globalThis.document = { baseURI: 'https://example.test/en/components/autocomplete' };
    globalThis.fetch = (url, options) => {
        calls.push({ url, options });
        return Promise.resolve(response);
    };

    return Promise.resolve(run(calls)).finally(() => {
        globalThis.fetch = original.fetch;
        globalThis.document = original.document;
    });
}

const source = (src, queryParam = 'q') => ({ src, queryParam });
const request = { term: 'Côté & Roy', skip: 50, take: 25, lang: 'fr', signal: undefined };

test('data source: builds an encoded same-origin GET request', () =>
    withMockedFetch({ ok: true, json: () => Promise.resolve({ total: 0, items: [] }) }, async calls => {
        const result = await FDCPAutocomplete.prototype.fetchFromSource.call(source('/api/employees?dept=1', 'term'), request);
        const url = new URL(calls[0].url);

        assert.deepEqual(result, { total: 0, items: [] });
        assert.equal(url.origin, 'https://example.test');
        assert.equal(url.pathname, '/api/employees');
        assert.equal(url.searchParams.get('dept'), '1');
        assert.equal(url.searchParams.get('term'), 'Côté & Roy');
        assert.equal(url.searchParams.get('skip'), '50');
        assert.equal(url.searchParams.get('take'), '25');
        assert.equal(url.searchParams.get('lang'), 'fr');
        assert.equal(calls[0].options.method, 'GET');
        assert.equal(calls[0].options.credentials, 'same-origin');
        assert.equal(calls[0].options.headers.Accept, 'application/json');
    }));

test('data source: Faker search terms survive URL encoding exactly', () =>
    withMockedFetch({ ok: true, json: () => Promise.resolve([]) }, async calls => {
        const faker = seededFaker();
        const terms = Array.from({ length: 200 }, () => faker.helpers.arrayElement([
            faker.person.fullName(),
            faker.person.lastName() + ', ' + faker.person.firstName(),
            faker.person.lastName() + ' & ' + faker.person.lastName(),
            faker.lorem.word() + '?' + faker.lorem.word() + '=' + faker.lorem.word() + '#' + faker.lorem.word(),
            faker.helpers.arrayElement(PAYLOADS),
            faker.internet.url()
        ]));

        for (const term of terms) {
            await FDCPAutocomplete.prototype.fetchFromSource.call(source('/api/employees'), { ...request, term });
        }

        calls.forEach((call, index) => {
            const url = new URL(call.url);

            assert.equal(url.origin, 'https://example.test');
            assert.equal(url.pathname, '/api/employees');
            assert.equal(url.hash, '');
            assert.deepEqual([...url.searchParams.keys()].sort(), ['lang', 'q', 'skip', 'take']);
            assert.equal(url.searchParams.get('q'), terms[index]);
        });
    }));

test('data source: refuses script and data URLs without calling fetch', () =>
    withMockedFetch({ ok: true, json: () => Promise.resolve([]) }, async calls => {
        for (const src of ['javascript:alert(1)', 'data:application/json,[]', 'file:///etc/passwd']) {
            await assert.rejects(FDCPAutocomplete.prototype.fetchFromSource.call(source(src), request), /Unsupported/);
        }

        assert.equal(calls.length, 0);
    }));

test('data source: a failed response rejects', () =>
    withMockedFetch({ ok: false, status: 500 }, async () => {
        await assert.rejects(FDCPAutocomplete.prototype.fetchFromSource.call(source('/api'), request), /500/);
    }));
