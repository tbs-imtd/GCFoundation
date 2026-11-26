# GCFoundation

[![CI Validation & Security](https://github.com/tbs-imtd/GCFoundation/actions/workflows/ci-validation.yml/badge.svg)](https://github.com/tbs-imtd/GCFoundation/actions/workflows/ci-validation.yml)
[![Publish NuGet Packages](https://github.com/tbs-imtd/GCFoundation/actions/workflows/publish-nuget.yml/badge.svg)](https://github.com/tbs-imtd/GCFoundation/actions/workflows/publish-nuget.yml)
[![Deploy Web App](https://github.com/tbs-imtd/GCFoundation/actions/workflows/deploy-web.yml/badge.svg)](https://github.com/tbs-imtd/GCFoundation/actions/workflows/deploy-web.yml)

([Français](#gcfoundation-1))

**GCFoundation** is a set of NuGet packages for building Government of Canada web applications with ASP.NET Core MVC on .NET 10. It provides the Canada.ca layout (header, footer, menu, breadcrumbs and language toggle), tag helpers for the [GC Design System (GCDS)](https://design-system.canada.ca/) and additional FDCP components, a strict Content Security Policy, session management, and English and French localization.

## Packages

| Package | Purpose |
| --- | --- |
| `GCFoundation.Components` | Shared layout, GCDS and FDCP tag helpers, error pages, navigation, session and localization services |
| `GCFoundation.Security` | Content Security Policy and related security headers |
| `GCFoundation.Common` | Settings, models and utilities shared by the other packages (installed automatically) |

This repository also contains `GCFoundation.Web`, the documentation and demo site.

## Quick start

These steps add GCFoundation to an ASP.NET Core MVC project. The documentation site's Installation page has the same steps with more detail, plus optional features such as navigation rules, hardened cookies, extra content sources and translated URLs.

### Prerequisites

- The [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2026 (18.0 or later), Visual Studio Code with the C# Dev Kit, or another editor that supports .NET 10
- An MVC project that targets `net10.0`, for example one created with `dotnet new mvc -n MyApp`

The examples use `MyApp` as the project name and root namespace.

### 1. Install the packages

```powershell
dotnet add package GCFoundation.Components
dotnet add package GCFoundation.Security
dotnet add package cloudscribe.Web.Localization
```

### 2. Configure `Program.cs`

```csharp
using cloudscribe.Web.Localization;
using GCFoundation.Common.Utilities;
using GCFoundation.Components.Middleware;
using GCFoundation.Components.Services;
using GCFoundation.Components.Services.Interfaces;
using GCFoundation.Security.Middlewares;
using Microsoft.AspNetCore.Localization;
using MyApp.Resources;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();
builder.Services.AddLocalization();

// Menu and breadcrumbs: read navigation.xml and translate its labels
builder.Services.AddCloudscribeNavigation(builder.Configuration.GetSection("NavigationOptions"));
builder.Services.AddSingleton<ITopNavigationLocalizationService, TopNavigationLocalizationService<Navigation>>();
builder.Services.AddSingleton<IBreadcrumbsLocalizationService, BreadcrumbsLocalizationService<Navigation>>();

// GCFoundation: layout and components, content security policy, session
builder.Services.AddGCFoundationComponents(builder.Configuration);
builder.Services.AddGCFoundationContentPolicies(builder.Configuration);
builder.Services.AddGCFoundationSession(builder.Configuration);

// English and French, selected from the first segment of the URL
var supportedCultures = LanguageUtility.GetSupportedCulture();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-CA");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0, new FirstUrlSegmentRequestCultureProvider(supportedCultures.ToList()));
});

var app = builder.Build();

app.UseMiddleware<GCFoundationLanguageMiddleware>();
app.UseGCFoundationComponents();
app.UseGCFoundationContentPolicies();
app.UseGCFoundationSession();

// Bilingual error pages provided by GCFoundation
if (!app.Environment.IsDevelopment())
{
    app.UseStatusCodePagesWithReExecute("/error/not-found");
    app.UseExceptionHandler("/error/global");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRequestLocalization();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{culture=en}/{controller=Home}/{action=Index}/{id?}");

app.Run();
```

### 3. Add your settings to `appsettings.json`

```json
"FoundationComponentsSettings": {
  "ApplicationNameEn": "My application",
  "ApplicationNameFr": "Mon application"
}
```

Every other setting has a working default.

### 4. Describe your navigation

Create `navigation.xml` at the root of the project. The `text` attributes are resource keys:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<NavNode key="Home" controller="Home" action="Index"
         preservedRouteParameters="culture" text="Nav_Home">
  <Children>
    <NavNode key="Privacy" controller="Home" action="Privacy"
             preservedRouteParameters="culture" text="Nav_Privacy" />
  </Children>
</NavNode>
```

Then add the labels in a `Resources` folder: an empty `Navigation` class, plus `Navigation.resx` (English) and `Navigation.fr.resx` (French) containing `Nav_Home` and `Nav_Privacy`.

```csharp
namespace MyApp.Resources;

public class Navigation
{
}
```

If Visual Studio generated a `Navigation.Designer.cs` file for the English file, that class already does the job: don't create `Navigation.cs`.

### 5. Use the layout and tag helpers

`Views/_ViewImports.cshtml`:

```cshtml
@using MyApp
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, GCFoundation.Components
```

`Views/_ViewStart.cshtml`:

```cshtml
@{
    Layout = "_FoundationLayout";
}
```

Your views now contain only the page content, for example `<gcds-heading tag="h1">Welcome</gcds-heading>`.

### 6. Run

```powershell
dotnet run
```

Open `/en` or `/fr`. You should see the Canada.ca header with your application name, the menu from `navigation.xml`, and a working language toggle.

## License

[MIT License](LICENSE)

---

# GCFoundation

([English](#gcfoundation))

**GCFoundation** est un ensemble de paquets NuGet pour créer des applications web du gouvernement du Canada avec ASP.NET Core MVC sur .NET 10. Il fournit la mise en page de Canada.ca (en-tête, pied de page, menu, fil d'Ariane et bouton de langue), des assistants de balise pour le [Système de design GC (SDGC)](https://design-system.canada.ca/fr/) et des composants FDCP supplémentaires, une politique de sécurité du contenu stricte, la gestion de session ainsi que la localisation en anglais et en français.

## Paquets

| Paquet | Rôle |
| --- | --- |
| `GCFoundation.Components` | Mise en page partagée, assistants de balise GCDS et FDCP, pages d'erreur, services de navigation, de session et de localisation |
| `GCFoundation.Security` | Politique de sécurité du contenu (CSP) et en-têtes de sécurité connexes |
| `GCFoundation.Common` | Paramètres, modèles et utilitaires communs aux autres paquets (installé automatiquement) |

Ce dépôt contient aussi `GCFoundation.Web`, le site de documentation et de démonstration.

## Démarrage rapide

Ces étapes ajoutent GCFoundation à un projet ASP.NET Core MVC. La page Installation du site de documentation présente les mêmes étapes plus en détail, ainsi que des fonctionnalités facultatives comme les règles de navigation, le renforcement des témoins, les sources de contenu supplémentaires et la traduction des URL.

### Prérequis

- Le [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2026 (18.0 ou ultérieure), Visual Studio Code avec le C# Dev Kit, ou un autre éditeur compatible avec .NET 10
- Un projet MVC qui cible `net10.0`, par exemple créé avec `dotnet new mvc -n MyApp`

Les exemples utilisent `MyApp` comme nom de projet et espace de noms racine.

### 1. Installer les paquets

```powershell
dotnet add package GCFoundation.Components
dotnet add package GCFoundation.Security
dotnet add package cloudscribe.Web.Localization
```

### 2. Configurer `Program.cs`

```csharp
using cloudscribe.Web.Localization;
using GCFoundation.Common.Utilities;
using GCFoundation.Components.Middleware;
using GCFoundation.Components.Services;
using GCFoundation.Components.Services.Interfaces;
using GCFoundation.Security.Middlewares;
using Microsoft.AspNetCore.Localization;
using MyApp.Resources;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();
builder.Services.AddLocalization();

// Menu et fil d'Ariane : lire navigation.xml et traduire ses libellés
builder.Services.AddCloudscribeNavigation(builder.Configuration.GetSection("NavigationOptions"));
builder.Services.AddSingleton<ITopNavigationLocalizationService, TopNavigationLocalizationService<Navigation>>();
builder.Services.AddSingleton<IBreadcrumbsLocalizationService, BreadcrumbsLocalizationService<Navigation>>();

// GCFoundation : mise en page et composants, politique de sécurité du contenu, session
builder.Services.AddGCFoundationComponents(builder.Configuration);
builder.Services.AddGCFoundationContentPolicies(builder.Configuration);
builder.Services.AddGCFoundationSession(builder.Configuration);

// Anglais et français, selon le premier segment de l'URL
var supportedCultures = LanguageUtility.GetSupportedCulture();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-CA");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0, new FirstUrlSegmentRequestCultureProvider(supportedCultures.ToList()));
});

var app = builder.Build();

app.UseMiddleware<GCFoundationLanguageMiddleware>();
app.UseGCFoundationComponents();
app.UseGCFoundationContentPolicies();
app.UseGCFoundationSession();

// Pages d'erreur bilingues fournies par GCFoundation
if (!app.Environment.IsDevelopment())
{
    app.UseStatusCodePagesWithReExecute("/error/not-found");
    app.UseExceptionHandler("/error/global");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRequestLocalization();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{culture=en}/{controller=Home}/{action=Index}/{id?}");

app.Run();
```

### 3. Ajouter vos paramètres dans `appsettings.json`

```json
"FoundationComponentsSettings": {
  "ApplicationNameEn": "My application",
  "ApplicationNameFr": "Mon application"
}
```

Tous les autres paramètres ont une valeur par défaut fonctionnelle.

### 4. Décrire votre navigation

Créez `navigation.xml` à la racine du projet. Les attributs `text` sont des clés de ressources :

```xml
<?xml version="1.0" encoding="utf-8" ?>
<NavNode key="Home" controller="Home" action="Index"
         preservedRouteParameters="culture" text="Nav_Home">
  <Children>
    <NavNode key="Privacy" controller="Home" action="Privacy"
             preservedRouteParameters="culture" text="Nav_Privacy" />
  </Children>
</NavNode>
```

Ajoutez ensuite les libellés dans un dossier `Resources` : une classe `Navigation` vide, ainsi que `Navigation.resx` (anglais) et `Navigation.fr.resx` (français) contenant `Nav_Home` et `Nav_Privacy`.

```csharp
namespace MyApp.Resources;

public class Navigation
{
}
```

Si Visual Studio a généré un fichier `Navigation.Designer.cs` pour le fichier anglais, cette classe fait déjà le travail : ne créez pas `Navigation.cs`.

### 5. Utiliser la mise en page et les assistants de balise

`Views/_ViewImports.cshtml` :

```cshtml
@using MyApp
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, GCFoundation.Components
```

`Views/_ViewStart.cshtml` :

```cshtml
@{
    Layout = "_FoundationLayout";
}
```

Vos vues ne contiennent plus que le contenu de la page, par exemple `<gcds-heading tag="h1">Bienvenue</gcds-heading>`.

### 6. Exécuter

```powershell
dotnet run
```

Ouvrez `/en` ou `/fr`. Vous devriez voir l'en-tête de Canada.ca avec le nom de votre application, le menu tiré de `navigation.xml` et un bouton de langue fonctionnel.

## Licence

[Licence MIT](LICENSE)
