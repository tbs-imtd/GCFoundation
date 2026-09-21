# API documentation

Start with the **API documentation page template** for a complete developer documentation page. It uses the standard Foundation layout, a page heading and the `fdcp-api-docs` page-body helper to compose contents navigation, overview, downloads, authored guidance and an OpenAPI reference.

The implementation has three responsibilities:

| Layer | Responsibility |
| --- | --- |
| Page template and `fdcp-api-docs` | GC shell integration, page composition, contents navigation, section ordering, overview and downloads. |
| `fdcp-api-reference` and its shared renderer | Authentication schemes, operations, parameters, request bodies, responses, examples and schemas. |
| OpenAPI reader | Interpret JSON and resolve supported references. |

The page-body helper and standalone reference component share the same reference renderer. Pass OpenAPI JSON as a string and add child sections for service-specific guidance.

```cshtml
@addTagHelper *, GCFoundation.Components

@{
    Layout = "_FoundationLayout";
}
<gcds-heading tag="h1">@Localizer["Requests API"]</gcds-heading>

<fdcp-api-docs document="@Model.OpenApiJson" id="requests-api"
               download-url="@Url.Content("~/api-spec/requests.json")">
    <fdcp-api-docs-section id="certificates" title="@Localizer["Client certificates"]">
        <partial name="_CertificateInstructions" />
    </fdcp-api-docs-section>
    <fdcp-api-docs-section id="limits" title="@Localizer["Rate limits"]">
        <p>@Localizer["Wait for the Retry-After interval when the service returns HTTP 429."]</p>
    </fdcp-api-docs-section>
    <fdcp-api-docs-section id="support" title="@Localizer["Support"]" after-endpoints="true">
        <partial name="_ApiSupport" />
    </fdcp-api-docs-section>
</fdcp-api-docs>
```

Read the specification in the application's controller/service and populate the view model:

```csharp
var json = await File.ReadAllTextAsync(
    Path.Combine(environment.WebRootPath, "api-spec", "requests.json"));
return View(new ApiPageViewModel { OpenApiJson = json });
```

The helper does not read files, fetch remote documents or execute API calls. Use the application's existing loading, authorization and caching policies. The download URL is optional and independent of the JSON. Use `Url.Content` to respect the application's path base and configure the application to serve the download.

## Standalone reference component

Use the smaller component when an existing page already owns its heading, navigation and guidance:

```cshtml
<fdcp-api-reference document="@Model.OpenApiJson" id="requests-reference">
</fdcp-api-reference>
```

`document` is required. `id` is optional; set a unique value for stable links. This component renders a single-column reference with no page navigation, overview, downloads or authored section slots. Both entry points require the standard Foundation CSS bundle. Generated headings start at level 2, so supply a level-1 page heading.

## Flexible sections

Sections contain normal Razor markup, partials, localized text and other tag helpers. They appear before generated authentication/endpoints, in declaration order. `after-endpoints="true"` places a section after the complete generated reference, including schemas. Both positions are included in contents navigation. Omit `document` for a guide containing only authored sections. Put all authored content inside `fdcp-api-docs-section` tags.

| Attribute | Purpose |
| --- | --- |
| `document` | OpenAPI 3.x JSON string; optional for authored-only guides. |
| `id` | Unique component prefix, generated if omitted. Set explicitly for stable deep links. ASCII letters, digits, hyphens and underscores only. |
| `title` | Overrides `info.title` in the contents navigation. |
| `download-url` | Optional HTTP(S) or application-relative specification link. |
| Child `title` | Required section heading and navigation label. |
| Child `id` | Stable suffix; the anchor is `{parent-id}-section-{child-id}`. |
| Child `after-endpoints` | Defaults to `false`; set to `true` for content after the reference. |

Use a distinct parent ID per instance. Generated child anchors are prefixed and deduplicated. Child punctuation is normalized to hyphens. Explicit child IDs preserve deep links when titles are translated.

## OpenAPI presentation

The renderer includes metadata, servers/variables, HTTP operations, deprecation notices, path and operation parameters, request bodies, every response code and media type, explicit examples, encoding, response headers, authentication schemes and effective security requirements. Operation parameters override path parameters with the same name and location. Operation security overrides the document default, including anonymous access.

Schemas have property tables and complete JSON definitions preserving arrays, nested objects, enums, required fields, formats, constraints and compositions. Reusable schema references link to their definitions without recursive expansion. Explicit examples are shown separately; no example payloads are invented from schemas.

Parameter inheritance and Reference Object handling follow the [OpenAPI specification](https://spec.openapis.org/oas/v3.2.0.html). Reference annotations are applied for 3.1 and later, while 3.0 retains its original behaviour. For 3.2 and later, the reference also renders `query` and `additionalOperations` methods. This is a presentation reader, not a complete OpenAPI validator.

## Assets, localization and accessibility

The existing `foundation.min.css` and `foundation.min.js` bundles include this component. The standard GCFoundation layout loads them; no additional CDN dependencies or inline scripts are needed.

Styling uses the layout's GCDS colour, typography, spacing, border and focus tokens, plus its CSS shortcuts for the download panel and visually hidden table captions. Custom layouts must also load the GCDS styles and CSS shortcuts. The current navigation entry uses a side marker and bold text; focused links use the paired GCDS focus foreground/background colours, including when the current entry is hovered.

Labels follow `CurrentUICulture` with English defaults and French translations. Supply translated document text and authored content as needed. OpenAPI descriptions are encoded plain text with line breaks preserved; Markdown is not rendered. Authored sections retain their Razor HTML. Avoid `Html.Raw` for untrusted content.

All documentation is visible without JavaScript. Contents links, semantic headings, labelled scrollable tables, focusable code blocks and visible focus outlines support keyboard access. JavaScript updates `aria-current` and initializes documentation loaded by FDCP tabs. Add `tabindex="0"` to authored code blocks for keyboard scrolling without JavaScript. Mobile styling stacks navigation above content; print styling hides navigation and wraps code.

## Supported input and limitations

Accepts JSON-encoded OpenAPI **3.x** versions in `3.minor.patch` form, including `3.2.0` and `3.2.1`. Convert YAML or Swagger 2 first. Bundle external Reference Objects (parameters, request bodies, responses, examples, security schemes and path items) into local JSON pointers. Missing, external or cyclic Reference Objects throw actionable exceptions. Schema references remain in the displayed definition; external or non-component schema references are shown as source references rather than fetched or expanded. Recursive schemas are safe to display.

Patch versions do not change rendering behaviour. Future 3.x minor versions are accepted using the known fields; this does not guarantee full support for new features. Newer features such as streaming item schemas, hierarchical tags and serialized examples do not yet have dedicated presentation. Use authored sections and the specification download for content that the renderer does not display.

Malformed JSON, malformed version strings and versions outside major version 3 throw during server rendering. Validate documents when loading/deploying if the application needs custom error handling. Callbacks, webhooks and links have no dedicated generated UI yet; use authored sections for guidance and offer a specification download for the complete contract.

The **Page Templates** catalogue links to the template guide at `/en/template/api-docs`, implementation examples at `/en/template/api-docs/code`, and the full demo at `/en/template/api-docs/demo`. French routes use `/fr/modele/documentation-api` with the same `/code` and `/demo` suffixes.

The **Components** catalogue contains the standalone renderer at `/en/components/api-reference` (French: `/fr/composants/reference-api`). The former `/en/components/api-docs` and `/en/components/api-docs/demo` routes, and their French equivalents, redirect to the corresponding template pages.
