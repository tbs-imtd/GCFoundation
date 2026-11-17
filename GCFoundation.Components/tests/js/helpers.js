// Shared by the fdcp-autocomplete unit and performance tests.
import { fakerEN_CA, fakerFR } from '@faker-js/faker';
import fs from 'node:fs';
import vm from 'node:vm';

/**
 * The component is a classic browser script, so load it the way the bundle does and read its test hook.
 */
export function loadComponent() {
    const previous = globalThis.module;

    globalThis.module = { exports: {} };
    vm.runInThisContext(
        fs.readFileSync(new URL('../../wwwroot/src/js/components/fdcp-autocomplete.js', import.meta.url), 'utf8'),
        { filename: 'fdcp-autocomplete.js' }
    );

    const exports = globalThis.module.exports;

    if (previous === undefined) {
        delete globalThis.module;
    } else {
        globalThis.module = previous;
    }

    return exports;
}

/**
 * Faker-generated fictional people, shaped like data source items.
 * Seeded, so a failing test sees the same records when it is re-run. French and English names are mixed
 * so the data carries accents, apostrophes and hyphens.
 */
export function fakePeople(count, seed = 20251117) {
    fakerFR.seed(seed);
    fakerEN_CA.seed(seed);

    return Array.from({ length: count }, (_, index) => {
        const faker = index % 2 === 0 ? fakerFR : fakerEN_CA;

        return {
            value: `E${String(index + 1).padStart(6, '0')}`,
            text: faker.person.fullName(),
            description: `${faker.person.jobTitle()}, ${faker.commerce.department()}`
        };
    });
}

/** A seeded Faker for one-off values in a test. */
export function seededFaker(seed = 20251117) {
    fakerFR.seed(seed);
    return fakerFR;
}

/** Strings that would be dangerous if a data source response were ever inserted as HTML. */
export const PAYLOADS = [
    '<script>alert(1)</script>',
    '"><img src=x onerror=alert(1)>',
    "' onfocus='alert(1)",
    '</div><svg onload=alert(1)>',
    'javascript:alert(1)',
    '{{constructor.constructor("alert(1)")()}}'
];
