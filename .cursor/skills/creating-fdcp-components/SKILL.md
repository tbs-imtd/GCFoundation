---
name: creating-fdcp-components
description: Creates and updates FDCP components in GCFoundation using the repository's Tag Helper, GCDS, accessibility, localization, validation, asset, documentation, and test conventions. Use whenever adding a new fdcp-* component, extending an FDCP form control, or creating component samples and documentation.
---

# Creating FDCP Components

Use the current codebase as the source of truth. Before editing, inspect the closest existing components across Tag Helpers, JS, SCSS, tests, samples, resources, and documentation. Prefer patterns that are current and repeated in the repository over instructions in this skill when they differ.

Read [reference.md](reference.md) when the component needs custom markup, JavaScript, validation, slots, localization, or documentation.

## 1. Classify the component

Choose the smallest implementation that meets the requirement:

1. **Thin GCDS form wrapper**: extend `FDCPBaseFormComponentTagHelper`, override `Process`, and render the appropriate `gcds-*` component.
2. **Custom form control**: extend `FDCPBaseFormComponentTagHelper`, override `ProcessAsync`, and own all markup, binding, accessibility, validation, JS, and SCSS. Follow the closest current custom form control.
3. **Non-form component**: extend `TagHelper` or the closest project base class.

Do not recreate behavior already provided by a suitable GCDS component.

## 2. Follow repository naming

- Tag: `fdcp-{kebab-name}`
- Class: `FDCP{Name}TagHelper`
- C# file: `GCFoundation.Components/TagHelpers/FDCP/FDCP{Name}TagHelper.cs`
- Enum: `GCFoundation.Components/Enums/FDCP{Name}{Purpose}.cs`
- JS: `GCFoundation.Components/wwwroot/src/js/components/fdcp-{kebab-name}.js`
- SCSS: `GCFoundation.Components/wwwroot/src/scss/components/_fdcp-{kebab-name}.scss`
- Test: `GCFoundation.Tests.Components/Tests/TagHelpers/FDCP/FDCP{Name}TagHelperTests.cs`
- Samples: `GCFoundation.Web/Views/Components/{Name}/_*.cshtml`

Use a `.fdcp-{kebab-name}` BEM root and `data-fdcp-{kebab-name}[-part]` JS hooks.

## 3. Implement form binding correctly

- Support model binding with `for`; support `name` when standalone usage is required.
- Resolve metadata through `ResolveFormField`.
- Preserve field name, value, label, hint, required, disabled, ID, and ModelState errors.
- Encode caller-controlled text and attributes.
- Generate stable, unique IDs and connect labels, hints, errors, controls, panels, and live regions.
- For multiple values, use successful form controls sharing the field name; do not mark every checkbox `required`.

## 4. Match GCDS

- Prefer GCDS components for primitives such as hints, error messages, icons, buttons, and error summaries.
- GCDS wrapper Tag Helpers (`TagHelpers/GCDS`) must expose every GCDS attribute under its exact GCDS name (e.g. `NoticeRole` → `notice-role`), so GCDS documentation works unchanged. Optional attributes must only render when set. Keep renamed legacy names as `[Obsolete]` aliases; `GcdsAttributeParityTests` enforces this and must be refreshed on GCDS upgrades.
- Declare attributes as auto-properties. When GCDS documents a default, initialize the property with it (`public HeadingRole HeadingRole { get; set; } = HeadingRole.primary;`) rather than a private nullable backing field with a fallback getter. Keep enum attributes non-nullable: Razor only accepts bare enum names such as `heading-role="light"` for non-nullable enum properties, and treats them as C# expressions on `Enum?` properties. Only when GCDS has no default for an enum attribute, keep a private nullable backing field behind a non-nullable property and render the field, so the attribute is omitted until set (e.g. `ContainerTagHelper.Layout`, `GridTagHelper.AlignItems`, `FieldsetTagHelper.LegendSize`).
- Use GCDS design tokens; avoid hard-coded colours, focus styles, fonts, spacing, and control dimensions when a token exists.
- Match GCDS label, required marker, hint, error placement, border, focus, disabled, and form spacing behavior.
- Keep component-specific styles under the component root.
- Add the SCSS partial to `wwwroot/src/scss/components/_index.scss`.

## 5. Build accessibility into the contract

- Use native semantics first.
- Choose the correct ARIA pattern for the interaction; do not mix patterns.
- Ensure complete keyboard operation, visible focus, logical focus return, and pointer/keyboard parity.
- Keep hidden options out of navigation.
- Announce dynamic result changes with a polite live region.
- Set `aria-invalid` only on the control representing the invalid form value.
- Link hint and error IDs with `aria-describedby`.
- Test labels, roles, states, expanded controls, selected states, disabled states, and unique IDs.

Choose the accessibility pattern that matches the actual interaction. Use labeled native controls whenever they can provide the required semantics.

## 6. Integrate validation

For custom form controls:

- Resolve server errors with `ResolveModelStateError`.
- Render `gcds-error-message` before the control, after the hint.
- Localize client validation text through component resources and pass it to JS through `data-*`.
- Validate on the same interactions as comparable GCDS controls and on form submission.
- On invalid state, show the inline error, set `aria-invalid`, and emit bubbling/composed `gcdsError` with a field-specific summary message.
- On valid state, clear the inline state and emit bubbling/composed `gcdsValid`.
- Dispatch summary events from the focusable control so summary links focus the correct element.
- Keep inline messages concise; make summary messages unique enough that multiple identical required errors do not collapse.

## 7. Localize all user-facing text

- Add English/default and French resource values.
- Keep strongly typed resource accessors synchronized with `.resx` files.
- Do not hard-code live-region, validation, count, placeholder, or sample text in one language.
- Samples must render the active culture only.
- Escaped sample code must display the resolved localized values.

## 8. Add documentation and samples

- Add the component route/action and component view model in `ComponentsController`.
- Add the component to the documentation index.
- Add localized name, overview, property descriptions, sample headings, and sample strings.
- Provide a live sample and the same source in `<fdcp-code>`, with tags encoded in the child content.
- Include representative modes and optional content.
- Add a required form example when the component participates in forms.

## 9. Test behavior, not implementation accidents

Add focused Tag Helper tests for:

- Model-bound and standalone usage
- Each supported mode and state
- Selected values and posted names
- Groups and optional slots
- Required and server-error markup
- Localization data passed to JS
- Roles, labels, ARIA relationships, tabindex policy, and unique IDs

Add JS or browser coverage when behavior cannot be proven by rendered-markup tests, especially keyboard navigation, filtering, focus, validation events, and error-summary integration.

## 10. Verify

Run the smallest relevant checks, then broaden:

```powershell
cd GCFoundation.Components
npm run build
dotnet build GCFoundation.Components.csproj --no-restore

cd ../GCFoundation.Tests.Components
dotnet test GCFoundation.Tests.Components.csproj --no-restore /p:BuildProjectReferences=false --filter "FullyQualifiedName~FDCP{Name}TagHelperTests"
```

Also build `GCFoundation.Web` when Razor samples, resources, models, routes, or documentation change. Manually verify the component documentation page and form submission behavior.

Do not hand-edit generated `foundation.min.css`, `foundation.min.js`, or source maps. Do not commit unrelated dependency or analyzer cleanup.

## Completion checklist

- [ ] Correct GCDS-wrapper/custom-component classification
- [ ] Naming and file placement follow repository conventions
- [ ] Form binding and ModelState behavior work
- [ ] English and French text are complete
- [ ] Accessibility and keyboard behavior are verified
- [ ] GCDS styling, validation, and error summary are integrated
- [ ] SCSS is forwarded and assets build
- [ ] Documentation, samples, and displayed code agree
- [ ] Focused tests and relevant project builds pass
