# FDCP Component Reference

## Find current reference implementations

Do not treat one component as permanently canonical. Before implementing:

1. Inspect comparable Tag Helpers under `GCFoundation.Components/TagHelpers/FDCP`.
2. Trace their supporting enums, JS, SCSS, tests, samples, resources, and controller documentation.
3. Prefer recently maintained patterns that appear across multiple components.
4. Confirm GCDS APIs and tokens currently installed by the project.
5. Resolve inconsistencies in favor of accessibility, existing public contracts, and current tests.

## Decision guide

### Thin wrapper

Use when a GCDS component already supplies the complete interaction:

```csharp
[HtmlTargetElement("fdcp-example", Attributes = "for")]
public class FDCPExampleTagHelper : FDCPBaseFormComponentTagHelper
{
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var field = ResolveFormField();
        output.TagName = "gcds-example";
        output.TagMode = TagMode.StartTagAndEndTag;
        // Map resolved field metadata to GCDS attributes.
    }
}
```

Follow `FDCPInputTagHelper` and `FDCPSelectTagHelper`.

### Custom interactive form control

Use only when GCDS cannot provide the required interaction. The component must own:

- Successful HTML form controls
- Client behavior
- Required and server validation
- GCDS error-summary events
- Accessible roles, states, keyboard behavior, and announcements
- GCDS-token-based styling

Follow the closest current custom interactive form control.

## File registration

- Tag Helpers are discovered by `@addTagHelper *, GCFoundation.Components`.
- SCSS partials require `@forward 'fdcp-{name}';` in `wwwroot/src/scss/components/_index.scss`.
- JS files under `wwwroot/src/js/components` are included by the existing Gulp build; verify the current Gulp configuration before assuming this for a new file type.
- Do not edit generated files under `wwwroot/css` or `wwwroot/js`.

## Patterns worth reusing from current components

### Tag Helper

- Two target declarations support `for, items` and `items, name`.
- `ResolveFormField` unifies bound and standalone metadata.
- IDs are generated from the field ID and sanitized.
- `HtmlEncoder.Default` protects all generated values.
- Localized strings are sent to JavaScript through root `data-*` attributes.
- Child slot content is parsed before rendering.
- The root carries a component discovery data attribute.

### JavaScript

- Cache DOM references once in the constructor.
- Bind events in one method.
- Initialize on `DOMContentLoaded` from the root data attribute.
- Guard against duplicate initialization.
- Keep visual state synchronized with ARIA state.
- Separate selection, filtering, focus, placement, summary, and validation methods.
- Emit a component change event when the public selection changes.
- Expose a global constructor only when manual initialization is part of the public contract.

### SCSS

- Keep selectors under the component BEM root.
- Style semantic attributes such as `[aria-invalid="true"]` when they are the source of truth.
- Use GCDS component tokens first and global tokens as fallbacks.
- Ensure focus outlines are not clipped by overflow containers.
- Apply spacing to scroll containers rather than only the first DOM option when filtering changes the first visible option.

## Validation protocol for custom controls

Inline error:

```html
<gcds-error-message
  id="{field-error-id}"
  message-id="{field-error-id}"
  data-fdcp-{component}-error>
  Localized error
</gcds-error-message>
```

Error-summary event:

```javascript
control.dispatchEvent(new CustomEvent('gcdsError', {
    bubbles: true,
    composed: true,
    detail: { message: localizedFieldSpecificSummary }
}));
```

Clear event:

```javascript
control.dispatchEvent(new CustomEvent('gcdsValid', {
    bubbles: true,
    composed: true
}));
```

The event target must be focusable. Verify both client-generated and server-rendered summary links.

## Localization layers

Use:

- `GCFoundation.Components/Resources/Localization*.resx` for reusable component UI strings.
- `GCFoundation.Components/Resources/Validation*.resx` for component validation strings.
- `GCFoundation.Web/Resources/Components*.resx` for documentation and sample text.

Update default/English and French resources together. Keep generated designer accessors synchronized.

## Test matrix

At minimum cover:

- Default rendering
- Every mode
- Bound and standalone names/values
- Initial selections
- Disabled items/component
- Grouped and ungrouped items
- Empty results
- Optional content
- Required state
- Server ModelState error
- Localized JS data
- Correct submitted controls
- ARIA names, descriptions, roles, state, and unique IDs

For interactive controls, manually or automatically test:

- Pointer selection
- Arrow keys, Enter, Escape, Space, and Tab as applicable
- Switching between pointer and keyboard
- Filtering and clearing filters
- Focus restoration and panel placement
- Required submit blocking
- Error summary population and link focus
- English and French cultures

## Common mistakes

- Reimplementing a GCDS component unnecessarily
- Styling with hard-coded GCDS look-alike values
- Adding `required` to every checkbox in a group
- Marking an internal search box invalid instead of the form control
- Rendering errors below the control instead of in GCDS order
- Using the same summary key/message for multiple custom controls when the summary deduplicates it
- Forgetting live-region localization
- Letting hidden options remain keyboard reachable
- Using radio semantics when the requested behavior is select-like
- Mixing single combobox/listbox semantics with multiple checkbox semantics
- Forgetting SCSS forwarding, documentation resources, or matching sample code
- Treating generated bundles as source files
