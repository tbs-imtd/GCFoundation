# fdcp-autocomplete

A text input that looks up suggestions from a data source (a web API by default) as the user types, and submits the **value** of the suggestion the user picks.

It is built for lists that are too large to send to the browser: results are requested a page at a time, only the visible rows are rendered, and repeated searches are answered from an in-memory cache.

- Tag helper: `GCFoundation.Components/TagHelpers/FDCP/FDCPAutocompleteTagHelper.cs`
- Script: `GCFoundation.Components/wwwroot/src/js/components/fdcp-autocomplete.js`
- Styles: `GCFoundation.Components/wwwroot/src/scss/components/_fdcp-autocomplete.scss`
- Live samples: `/en/components/autocomplete` and `/fr/composants/saisie-semi-automatique` in `GCFoundation.Web`
- Data source samples (Web API, database, GraphQL, existing API, local data) with evidence: [DATA-SOURCES.md](DATA-SOURCES.md)

## Which component should I use?

| | `fdcp-autocomplete` | `fdcp-searchable-select` |
|---|---|---|
| Where the options live | Remote data source, queried as the user types | Passed in as `items` and rendered into the page |
| List size it suits | Hundreds to many thousands | Up to a few hundred |
| How the user starts | Types straight into a text box | Opens a panel with a button, then searches inside it |
| Selection | Single | Single or multiple |
| Option groups | No | Yes (`SelectListItem.Group`) |
| Second line per option | Yes (`description`) | No |
| Paging, virtualization, caching | Yes | No (every option is in the DOM) |
| Works without an API | No | Yes |
| Works offline or when the API is down | No. Shows an error and recovers on the next keystroke | Yes |
| Matching | Decided by your API. The samples ignore case and accents and fold `œ æ ø ß ł` | Simple "contains" on the option text, in the browser |
| ARIA pattern | Combobox with list autocomplete | Button-opened combobox (single) or checkbox group (multiple) |

**Use `fdcp-searchable-select` when** the list is known when the page renders and is small enough to send in full: countries, provinces, classifications, a department's branches. Also use it when you need groups or multiple selections.

**Use `fdcp-autocomplete` when** the list is large, changes often, or must not be sent to the browser in full: employees, organizations, addresses, codes. The user must end up with exactly one item from the source.

If both would work, prefer `fdcp-searchable-select`. It has no network dependency and nothing to secure.

## Quick start

```cshtml
<fdcp-autocomplete for="@Model.EmployeeId"
                   src="@Url.Action("Search", "Employees")"
                   selected-text="@Model.EmployeeName"></fdcp-autocomplete>
```

Without a model:

```cshtml
<fdcp-autocomplete name="employee"
                   src="/api/employees"
                   label="Employee"
                   hint="Type at least 2 characters of a first or last name."
                   required="true"></fdcp-autocomplete>
```

The posted form field is `EmployeeId` (or `employee`) and holds the selected item's `value`. The visible text is not submitted.

The field is **strict**: text that was typed but not picked from the list is a validation error ("Select an option from the list."), and editing the text of an existing selection clears the value.

## Properties

| Attribute | Type | Default | Description |
|---|---|---|---|
| `for` | `ModelExpression` | | Model property to bind. Supplies name, value, label, hint and required state. |
| `name` | `string` | | Field name when not using `for`. One of `for` or `name` is required. |
| `src` | `string` | required | Data source URL. Must be relative, `http` or `https`; anything else throws at render time. |
| `label` | `string` | display name | Label text. |
| `hint` | `string` | display description | Hint text. |
| `required` | `bool` | from `[Required]` | Whether a selection is required. |
| `disabled` | `bool` | `false` | Disables the control; nothing is submitted. |
| `value` | `string` | model value | Current value. |
| `selected-text` | `string` | the value | Text shown for the current value. Lets an edit form show the saved selection without a lookup. |
| `placeholder` | `string` | | Placeholder text. |
| `query-param` | `string` | `q` | Query-string name for the search term. |
| `min-length` | `int` | `2` | Characters required before a lookup. |
| `debounce` | `int` | `250` | Milliseconds to wait after the last keystroke. |
| `page-size` | `int` | `50` | Records requested per call. |
| `max-results` | `int` | `1000` | Most records the list will load for one term. |
| `cache` | `bool` | `true` | Cache pages of results in memory. |
| `cache-ttl` | `int` | `300` | Seconds a cached page stays valid. |
| `cache-size` | `int` | `50` | Most pages kept (least recently used is dropped). |
| `loading-text`, `no-results-text`, `one-result-text`, `multiple-results-text`, `min-length-text`, `load-error-text`, `clear-text`, `instructions-text` | `string` | localized | Overrides for built-in English and French text. `multiple-results-text` and `min-length-text` take `{0}`. |

## Data source contract

Request:

```
GET {src}?q={term}&skip={offset}&take={page-size}&lang={en|fr}
Accept: application/json
X-Requested-With: XMLHttpRequest
```

Response:

```json
{
  "total": 1342,
  "items": [
    { "value": "E000123", "text": "Marie Tremblay", "description": "Senior Analyst, Digital Services" }
  ]
}
```

- `total` is the number of matches across all pages. `items` is the requested page.
- `description` is optional. Use it to tell apart items with the same text.
- A bare array `[{...}]` is also accepted and treated as the only page.
- Items missing `value` or `text` are dropped. Text longer than 500 characters is cut.

`GCFoundation.Components.Models` has matching records, so an endpoint can be this short:

```csharp
[Authorize]
[HttpGet("employees/search")]
public IActionResult Search(string? q, int skip = 0, int take = 50, string? lang = null)
{
    // Enforce these on the server. The client's settings are only a courtesy.
    if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2) return Json(new FDCPAutocompleteResult(0, []));
    take = Math.Clamp(take, 1, 100);
    skip = Math.Clamp(skip, 0, 1000);

    var (total, people) = _directory.Search(q.Trim(), skip, take);

    return Json(new FDCPAutocompleteResult(
        total,
        [.. people.Select(p => new FDCPAutocompleteItem(p.Id, p.DisplayName, p.Title(lang)))]));
}
```

`GCFoundation.Web/Services/AutocompleteSampleDirectory.cs` is a complete working example over 5,000 generated records, including accent-insensitive matching and both "First Last" and "Last, First" order.

### A data source that is not a simple GET

Replace the lookup function. It receives the term, paging, language and an `AbortSignal`, and returns the same shape as above. Attach it from the `fdcp-autocomplete:ready` event, which fires when a component has finished setting up, so the code works wherever the script sits on the page:

```js
document.addEventListener('fdcp-autocomplete:ready', event => {
    if (event.target.id !== 'employee') return;

    event.detail.instance.dataSource = async ({ term, skip, take, lang, signal }) => {
        const response = await fetch('/graphql', { method: 'POST', signal, /* ... */ });
        const data = await response.json();
        return { total: data.total, items: data.people.map(p => ({ value: p.id, text: p.name })) };
    };
});
```

Paging, caching, virtualization and stale-response handling still apply.

[DATA-SOURCES.md](DATA-SOURCES.md) has complete, tested samples for a database, GraphQL, an existing API with a different shape, and data already on the page.

## Events

All bubble from the component's root element.

| Event | `detail` | When |
|---|---|---|
| `fdcp-autocomplete:ready` | `{ instance }` | The component finished setting up. The place to attach a custom `dataSource`. |
| `fdcp-autocomplete:change` | `{ value, text, item }` | The submitted value changed (selected, cleared, or dropped by editing). |
| `fdcp-autocomplete:loaded` | `{ term, skip, total, count }` | A page of results arrived. |
| `fdcp-autocomplete:error` | `{ term, skip, error }` | The data source failed. |

Validation also raises the GCDS `gcdsError` and `gcdsValid` events from the text input, so `gcds-error-summary` lists and links to the field.

## Keyboard

| Key | Action |
|---|---|
| Type | Search after the debounce delay |
| `↓` | Open the list, or move to the next option |
| `↑` | Move to the previous option |
| `Alt` + `↓` | Open the list without moving |
| `Page Down` / `Page Up` | Move by one screen of options |
| `Ctrl` + `Home` / `End` | First / last option (plain `Home` and `End` move the text caret) |
| `Enter` | Select the active option. With no active option, the form submits as usual |
| `Esc` | Close the list. Press again to clear the field |
| `Tab` | Close the list without selecting and move on |

Focus stays in the text input the whole time; the active option is exposed with `aria-activedescendant`. Moving through the list never selects anything.

## Performance

**Paging.** The first page is requested when the user stops typing. Further pages are requested only when rows from them scroll into view, so jumping to the end of 5,000 results costs one more request, not fifty.

**Virtualization.** Only the visible rows plus five above and below are in the DOM, about 16 elements whatever the total. Every option has the same height, measured from a rendered row and re-measured when text size, zoom or width changes. Each rendered option carries `aria-setsize` and `aria-posinset`, and the active option is always kept rendered, so `aria-activedescendant` always points at a real element.

**Caching.** Pages are kept in an in-memory LRU cache keyed by language, normalized term and offset. The term is normalized by trimming, collapsing spaces, ignoring case and accents, and spelling out `œ`, `æ`, `ø`, `ß` and `ł` (as `oe`, `ae`, `o`, `ss`, `l`), so "Côté", "cote" and "COTE" share one entry. This assumes the data source treats those as the same search; if yours is case- or accent-sensitive, set `cache="false"`. Identical requests in flight are shared. Failures are not cached. The cache belongs to one component instance and is cleared when the page is hidden.

**Stale responses.** Each new term aborts the previous request, and a response for an older term is ignored even if it arrives late. Closing the list (Tab, Esc, focus leaving) cancels lookups in flight.

Measured on a developer laptop (2026-11-17, Chrome, mocked source holding 10,000 records):

| Measure | Result | Budget in the test |
|---|---|---|
| First results after the last keystroke | 268 ms, of which 250 ms is the debounce | 1,000 ms |
| Same search again (cached) | 253 ms, no request | 400 ms |
| Frame time while scrolling 5,000 rows | 16.7 ms median and 95th percentile, 0 long tasks | 34 ms median |
| Rows in the DOM while scrolling 5,000 | 16 at most | 20 |
| Down-arrow key press | 0.16 ms | 8 ms |
| Requests for 8 fast keystrokes | 1 | 1 |
| Component set-up | 0.01 ms per instance | 2 ms |
| Server: search 5,000 records | 0.06 ms | 2 ms |
| Server: search 50,000 records | 0.66 ms (12.7x the time for 10x the data) | 15 ms |
| Server: render the tag helper | 0.007 ms | 0.5 ms |

Budgets are deliberately several times the measured cost: they are there to catch a change in how the work scales, not to benchmark the machine.

Tuning:

- Raise `min-length` or `debounce` to cut load on a slow source.
- Raise `page-size` if rows are small and the source is fast; lower it if each record is expensive.
- `max-results` bounds both memory and how much of the source one term can pull.
- Set `cache="false"` for data that changes minute to minute, or that must not stay in memory.

## Security

What the component does:

- Every value the tag helper writes is HTML-encoded, and element IDs are sanitized.
- Suggestions are inserted with `textContent` only. Markup returned by a data source is displayed as text and never parsed.
- `src` is checked on the server (relative, `http` or `https` only) and again in the browser before each request.
- Requests are `GET` with `credentials: 'same-origin'`. A cross-origin source only works if it is listed in `ContentPolicySettings:ConnectSrc`; the component adds no CSP exceptions and uses no inline script or `style` attributes.
- Responses are validated and coerced to strings; malformed items are dropped.
- Cached results live in memory only. Nothing is written to `localStorage`, `sessionStorage` or IndexedDB.

What **your endpoint** must do. The component cannot do these for you:

1. **Require authorization.** A directory search must not be anonymous.
2. **Enforce limits on the server**: minimum term length, maximum `take`, maximum `skip`. Paging makes it easier to walk an entire directory, so cap how deep a caller can go, and rate-limit.
3. **Return only what the list shows.** No phone numbers, PRI or other identifiers.
4. **Re-validate the posted value.** The hidden field is user-controlled; confirm the ID exists and that the caller may use it.
5. **Send `Cache-Control: no-store`.** `GCFoundationContentPoliciesMiddleware` already does when it is in the pipeline.

Search terms travel in the query string, so they can appear in server and proxy logs. If that is not acceptable for your data, supply a custom `dataSource` that uses `POST`.

## Accessibility (WCAG 2.2, including AAA)

WCAG conformance is claimed for a whole page, not a component. This component is built so that it does not stop a page from reaching AAA. The criteria that apply to it:

| Criterion | Level | How it is met | How it is checked |
|---|---|---|---|
| 1.3.1, 4.1.2 Name, role, value | A | Native `<input>` with `role="combobox"`, `aria-expanded`, `aria-controls`, `aria-activedescendant`; `listbox` / `option`; label, hint, error and instructions linked by ID | Tag helper tests, e2e, axe |
| 1.4.6 Contrast (Enhanced) | AAA | Text and its background are 7:1 or better in every state | axe enhanced-contrast rule, open and closed |
| 1.4.1 Use of colour | A | Active option is underlined as well as inverted; errors have text | e2e |
| 1.4.4, 1.4.10, 1.4.12 Resize, reflow, text spacing | AA | Sizes in `rem`; rows re-measured on resize; no horizontal overflow at 320 px | e2e at 320 px and 200% text |
| 2.1.3 Keyboard (No Exception) | AAA | Every action, including paging, clearing and retrying, works from the keyboard | e2e |
| 2.2.3 No Timing, 2.2.4 Interruptions | AAA | No time limits; nothing closes by itself; announcements are polite | Design |
| 2.3.3 Animation from Interactions | AAA | No animation or transitions | Stylesheet |
| 2.4.7, 2.4.12, 2.4.13 Focus visible, not obscured, appearance | AA / AAA | GCDS focus ring; the list opens above when there is no room below, so it never covers the input | e2e |
| 2.5.5 Target Size (Enhanced) | AAA | Options, input and clear button are at least 44 by 44 CSS px | e2e |
| 3.2.2, 3.2.5 Change on Request | AAA | Arrowing never selects; nothing submits or navigates by itself | e2e |
| 3.3.1, 3.3.3 Error identification and suggestion | A / AA | Inline message naming the fix, plus an error-summary entry naming the field | e2e |
| 3.3.5 Help | AAA | Hint text and keyboard instructions via `aria-describedby` | Tag helper tests |
| 3.3.6 Error Prevention (All) | AAA | A selection can always be reviewed, changed or cleared before submit | e2e |
| 4.1.3 Status Messages | AA | Loading, result counts, no results and errors go to a polite live region | e2e |

Checked automatically: axe with the `wcag2a`, `wcag2aa`, `wcag21a`, `wcag21aa`, `wcag22aa` and `wcag2aaa` rule sets, in English and French, with the list closed, open with an active option, showing no results, invalid, and with a selection. Zero violations.

**Not yet done:** a manual pass with NVDA, JAWS and VoiceOver. Automated rules cannot establish AAA, and combobox announcements differ between screen readers. Do this before relying on the component in production.

## Localization

English and French text is built in (`Resources/Localization*.resx`, `Resources/Validation*.resx`) and follows the request culture. The tag helper passes the strings to the script as `data-*` attributes; the text attributes in the property table override them per instance.

The root element gets `lang="en"` or `lang="fr"`, and the same value is sent to the data source as `lang`, so the API can return text in the right language. The cache keeps languages separate.

## Tests

| What | Where | Run |
|---|---|---|
| Tag helper: markup, binding, ARIA, localization, encoding, URL checks | `Tests/TagHelpers/FDCP/FDCPAutocompleteTagHelperTests.cs` | `dotnet test --filter "FullyQualifiedName~Autocomplete"` |
| Tag helper against hundreds of Faker-generated inputs (encoding round-trip, injected markup, IDs, settings, URLs) | `Tests/TagHelpers/FDCP/FDCPAutocompleteTagHelperFakerTests.cs` | same |
| Sample directory: 5,000 seeded records, matching, paging limits; Faker-generated terms and garbage | `Tests/Services/AutocompleteSampleDirectory*Tests.cs` | same |
| Demo endpoints with a mocked directory (Moq) and Faker data; all data sources agree | `Tests/Controllers/Autocomplete*EndpointTests.cs` | same |
| Server performance budgets | `Tests/Performance/AutocompletePerformanceTests.cs` | `dotnet test --filter "Category=Performance"` |
| Script logic: cache, virtual window, response validation, request building (Faker data, mocked `fetch` and clock) | `GCFoundation.Components/tests/js/fdcp-autocomplete.test.js` | `npm test` in `GCFoundation.Components` |
| Script performance budgets | `GCFoundation.Components/tests/js/perf/` | `npm run test:perf` in `GCFoundation.Components` |
| Browser behaviour, real endpoint and mocked source with 5,000 and 10,000 random records, accessibility | `e2e/tests/fdcp-autocomplete.spec.ts` | `npx playwright test fdcp-autocomplete.spec` in `e2e`, with `GCFoundation.Web` running on port 5215 |
| Every data source sample, end to end | `e2e/tests/fdcp-autocomplete.sources.spec.ts` | `npx playwright test fdcp-autocomplete.sources` |
| Browser performance budgets | `e2e/tests/fdcp-autocomplete.perf.spec.ts` | `npm run test:perf` in `e2e` |

C# test paths are under `GCFoundation.Tests.Components/`.

Test data comes from Faker: [Bogus](https://github.com/bchavez/Bogus) in C# and [@faker-js/faker](https://fakerjs.dev) in JavaScript. Every generator is seeded, so a failing test sees the same data when it is re-run. The browser tests use a small seeded generator of their own (`e2e/utils/autocomplete-data.ts`).

## Known limits

- Single selection only. For multiple selections use `fdcp-searchable-select`.
- Every option has the same height. Text that does not fit on one line is cut with an ellipsis; the full text is still in the accessible name.
- Assistive technology sees only the rendered rows. Position in the full list comes from `aria-setsize` and `aria-posinset`.
- The list holds at most `max-results` rows per term; users narrow further by typing.
- Browser tests run in Chrome and Edge. Firefox and Safari have not been tested.
