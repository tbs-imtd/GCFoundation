// Unit tests for fdcp-lazy-loader and the built bundles. Run with: npm test
import assert from 'node:assert/strict';
import { createHash } from 'node:crypto';
import fs from 'node:fs';
import test from 'node:test';
import vm from 'node:vm';

const loaderSource = fs.readFileSync(new URL('../../wwwroot/src/js/components/fdcp-lazy-loader.js', import.meta.url), 'utf8');
const builtUrl = file => new URL(`../../wwwroot/js/${file}`, import.meta.url);

/** Just enough of a document for the loader: one script element, a selector match, and a head. */
function fakePage({ hasAutocomplete, bundleSrc = 'https://example.gc.ca/app/_content/GCFoundation.Components/js/foundation.min.js?v=abc', readyState = 'complete' }) {
    const appended = [];
    const listeners = {};
    const document = {
        readyState,
        baseURI: 'https://example.gc.ca/app/page',
        currentScript: { src: bundleSrc },
        head: { appendChild: element => appended.push(element) },
        querySelector: selector => (selector === '[data-fdcp-autocomplete]' && hasAutocomplete ? {} : null),
        addEventListener: (type, handler) => { listeners[type] = handler; },
        createElement: () => {
            const handlers = {};
            return {
                attributes: {},
                setAttribute(name, value) { this.attributes[name] = value; },
                addEventListener: (type, handler) => { handlers[type] = handler; },
                remove() { this.removed = true; },
                fire: type => handlers[type](),
            };
        }
    };
    const window = {};
    const consoleErrors = [];

    vm.runInNewContext(loaderSource, { document, window, URL, Promise, Error, Object, console: { error: e => consoleErrors.push(e) } });
    return { window, appended, listeners, consoleErrors };
}

test('lazy loader: loads the autocomplete script when the page has one', () => {
    const { appended } = fakePage({ hasAutocomplete: true });

    assert.equal(appended.length, 1);
    assert.equal(appended[0].attributes['data-fdcp-lazy'], 'autocomplete');
    assert.equal(appended[0].async, false);
});

test('lazy loader: loads nothing when the page has no autocomplete', () => {
    const { appended, window } = fakePage({ hasAutocomplete: false });

    assert.equal(appended.length, 0);
    assert.equal(typeof window.FDCPLazy.load, 'function');
});

test('lazy loader: resolves next to foundation.min.js, keeping the virtual directory and dropping its query', () => {
    const { appended } = fakePage({ hasAutocomplete: true });
    const url = new URL(appended[0].src);

    assert.equal(url.origin + url.pathname, 'https://example.gc.ca/app/_content/GCFoundation.Components/js/fdcp-autocomplete.min.js');
    // The source file still has the placeholder, so no version is added.
    assert.equal(url.searchParams.get('v'), null);
});

test('lazy loader: waits for DOMContentLoaded while the page is loading', () => {
    const { appended, listeners } = fakePage({ hasAutocomplete: true, readyState: 'loading' });

    assert.equal(appended.length, 0);
    listeners.DOMContentLoaded();
    assert.equal(appended.length, 1);
});

test('lazy loader: load() adds the script once and shares the promise', async () => {
    const { appended, window } = fakePage({ hasAutocomplete: false });
    const first = window.FDCPLazy.load('autocomplete');
    const second = window.FDCPLazy.load('autocomplete');

    assert.equal(first, second);
    assert.equal(appended.length, 1);
    appended[0].fire('load');
    await first;
});

test('lazy loader: a failed load can be retried', async () => {
    const { appended, window } = fakePage({ hasAutocomplete: false });
    const first = window.FDCPLazy.load('autocomplete');

    appended[0].fire('error');
    await assert.rejects(first, /fdcp-autocomplete\.min\.js/);
    assert.equal(appended[0].removed, true);

    window.FDCPLazy.load('autocomplete');
    assert.equal(appended.length, 2);
});

test('lazy loader: rejects an unknown component', async () => {
    const { window } = fakePage({ hasAutocomplete: false });

    await assert.rejects(window.FDCPLazy.load('nope'), /Unknown lazy component/);
});

test('build: foundation.min.js does not contain the autocomplete and stamps its version', () => {
    const bundle = fs.readFileSync(builtUrl('foundation.min.js'), 'utf8');
    const lazy = fs.readFileSync(builtUrl('fdcp-autocomplete.min.js'));
    const version = createHash('sha256').update(lazy).digest('hex').slice(0, 16);

    assert.ok(!bundle.includes('FDCPAutocompleteCache'), 'autocomplete code is still in foundation.min.js');
    assert.ok(!bundle.includes('__FDCP_LAZY_VERSION_'), 'a version placeholder was not replaced');
    assert.ok(bundle.includes(version), 'foundation.min.js is out of date with fdcp-autocomplete.min.js; run npm run build');
    assert.ok(lazy.toString().includes('FDCPAutocompleteCache'));
});
