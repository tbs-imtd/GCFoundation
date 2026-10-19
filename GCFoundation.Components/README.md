# GCFoundation.Components

[![NuGet](https://img.shields.io/nuget/v/GCFoundation.Components.svg)](https://www.nuget.org/packages/GCFoundation.Components/)

**GCFoundation.Components** provides pre-built Razor components, tag helpers, and view components based on the **GC Design System (GCDS)** for Government of Canada web applications.

## Features

- **GCDS Tag Helpers** – Easily render GCDS-compliant HTML elements (buttons, alerts, cards, navigation, etc.).
- **View Components** – Reusable UI building blocks for breadcrumbs, language toggles, headers, footers, and more.
- **Localization Support** – Built-in resource files for English and French.
- **Static Web Assets** – Bundled CSS/JS automatically served via `_content/GCFoundation.Components`.

## Installation

```shell
dotnet add package GCFoundation.Components
```

## Quick Start

1. **Register services** in `Program.cs`:

```csharp
builder.Services.AddGCFoundationComponents(builder.Configuration);
```

2. **Enable middleware**:

```csharp
app.UseGCFoundationComponents();
```

3. **Import tag helpers** in `_ViewImports.cshtml`:

```razor
@addTagHelper *, GCFoundation.Components
```

4. **Use components** in your views:

```razor
<gcds-button type="submit">Submit</gcds-button>
```

## Dependencies

- `GCFoundation.Common`
- `cloudscribe.Web.Navigation`
- `HtmlAgilityPack`

## License

[MIT](../LICENSE)

