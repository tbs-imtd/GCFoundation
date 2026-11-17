// Performance budgets for the DOM-free parts of fdcp-autocomplete. Run with: npm run test:perf
//
// Budgets are several times what a developer laptop needs. They are there to catch a change in
// algorithmic cost (a linear scan creeping into a hot path), not to benchmark the machine.
import assert from 'node:assert/strict';
import { performance } from 'node:perf_hooks';
import test from 'node:test';
import { fakePeople, loadComponent, seededFaker } from '../helpers.js';

const {
    FDCPAutocompleteCache,
    fdcpAutocompleteWindow,
    fdcpAutocompleteScrollTopForIndex,
    fdcpAutocompleteNormalizeResult
} = loadComponent();

/** Warms up once, then returns the fastest of several runs in milliseconds. */
function measure(action, runs = 5) {
    action();
    let best = Infinity;

    for (let run = 0; run < runs; run += 1) {
        const start = performance.now();
        action();
        best = Math.min(best, performance.now() - start);
    }

    return best;
}

function report(t, name, actualMs, budgetMs) {
    t.diagnostic(`${name}: ${actualMs.toFixed(4)} ms (budget ${budgetMs} ms)`);
    assert.ok(actualMs <= budgetMs, `${name} took ${actualMs.toFixed(4)} ms, budget is ${budgetMs} ms`);
}

test('perf: window calculation is constant time, whatever the list size', t => {
    const cost = total => measure(() => {
        for (let step = 0; step < 100000; step += 1) {
            fdcpAutocompleteWindow({ total, rowHeight: 60, scrollTop: (step * 977) % (total * 60), viewportHeight: 360, overscan: 5 });
        }
    });
    const small = cost(1000);
    const huge = cost(10000000);

    report(t, '100,000 window calculations over 1,000 rows', small, 100);
    report(t, '100,000 window calculations over 10,000,000 rows', huge, 100);
    assert.ok(huge < small * 5 + 5, `10,000,000 rows cost ${(huge / small).toFixed(1)}x the time of 1,000 rows`);
});

test('perf: scroll-to-index calculation', t => {
    const elapsed = measure(() => {
        for (let index = 0; index < 100000; index += 1) {
            fdcpAutocompleteScrollTopForIndex({ index, rowHeight: 60, scrollTop: (index * 31) % 6000000, viewportHeight: 360 });
        }
    });

    report(t, '100,000 scroll-to-index calculations', elapsed, 50);
});

test('perf: validating a 100-record page of Faker data', t => {
    const page = { total: 5000, items: fakePeople(100) };
    const elapsed = measure(() => fdcpAutocompleteNormalizeResult(page, 5000), 20);

    report(t, 'normalize one 100-record page', elapsed, 2);
});

test('perf: validating 10,000 Faker records scales linearly', t => {
    const records = fakePeople(10000);
    const small = measure(() => fdcpAutocompleteNormalizeResult(records.slice(0, 1000), 10000), 10);
    const large = measure(() => fdcpAutocompleteNormalizeResult(records, 10000), 10);

    report(t, 'normalize 10,000 records', large, 50);
    t.diagnostic(`1,000 records: ${small.toFixed(4)} ms, 10,000 records: ${large.toFixed(4)} ms`);
    assert.ok(large < small * 40 + 2, `10x the records cost ${(large / small).toFixed(1)}x the time`);
});

test('perf: cache reads and writes stay cheap when full', t => {
    const faker = seededFaker();
    const cache = new FDCPAutocompleteCache({ maxEntries: 50 });
    const keys = Array.from({ length: 5000 }, (_, index) => FDCPAutocompleteCache.key('en', `${faker.person.lastName()} ${index}`, 0));
    const page = { total: 100, items: fakePeople(100) };

    const writes = measure(() => {
        for (const key of keys) cache.set(key, page);
    });
    const reads = measure(() => {
        for (let round = 0; round < 100; round += 1) {
            for (const key of keys.slice(-50)) cache.get(key);
        }
    });

    report(t, '5,000 cache writes with eviction', writes, 50);
    report(t, '5,000 cache hits', reads, 50);
    assert.equal(cache.size, 50);
});

test('perf: cache cost grows far slower than its capacity', t => {
    const page = { total: 1, items: [] };
    const cost = maxEntries => {
        const cache = new FDCPAutocompleteCache({ maxEntries });
        const keys = Array.from({ length: maxEntries }, (_, index) => `en|term ${index}|0`);
        for (const key of keys) cache.set(key, page);

        return measure(() => {
            for (let step = 0; step < 20000; step += 1) {
                cache.get(keys[step % keys.length]);
                cache.set(`en|new ${step}|0`, page);
            }
        });
    };
    const small = cost(50);
    const large = cost(5000);

    t.diagnostic(`20,000 get+set pairs: capacity 50 = ${small.toFixed(3)} ms, capacity 5,000 = ${large.toFixed(3)} ms`);
    // Not perfectly flat: a large Map carries more deleted slots to step over and more memory to touch.
    // The guard is against a linear scan, which would cost about 100x here.
    assert.ok(large < small * 30 + 10, `a 100x larger cache cost ${(large / small).toFixed(1)}x per operation`);
});

test('perf: building cache keys from Faker terms', t => {
    const faker = seededFaker();
    const terms = Array.from({ length: 10000 }, () => `  ${faker.person.fullName()}   ${faker.person.lastName()} `);
    const elapsed = measure(() => {
        for (const term of terms) FDCPAutocompleteCache.key('fr', term, 50);
    });

    report(t, '10,000 cache keys', elapsed, 50);
});

test('perf: 1,000 concurrent identical lookups cost one data source call', async t => {
    const cache = new FDCPAutocompleteCache();
    let calls = 0;
    const loader = () => {
        calls += 1;
        return new Promise(resolve => setTimeout(() => resolve({ total: 100, items: fakePeople(100) }), 5));
    };

    const start = performance.now();
    const results = await Promise.all(Array.from({ length: 1000 }, () => cache.fetch('en|tremblay|0', loader)));
    const elapsed = performance.now() - start;

    assert.equal(calls, 1);
    assert.ok(results.every(result => result === results[0]));
    report(t, '1,000 de-duplicated lookups', elapsed, 250);
});
