# GCFoundation

[![CI Validation & Security](https://github.com/tbs-imtd/GCFoundation/actions/workflows/ci-validation.yml/badge.svg)](https://github.com/tbs-imtd/GCFoundation/actions/workflows/ci-validation.yml)
[![Publish NuGet Packages](https://github.com/tbs-imtd/GCFoundation/actions/workflows/publish-nuget.yml/badge.svg)](https://github.com/tbs-imtd/GCFoundation/actions/workflows/publish-nuget.yml)
[![Deploy Web App](https://github.com/tbs-imtd/GCFoundation/actions/workflows/deploy-web.yml/badge.svg)](https://github.com/tbs-imtd/GCFoundation/actions/workflows/deploy-web.yml)

**GCFoundation** is a comprehensive .NET 10 ASP.NET MVC foundation designed for building Government of Canada (GC) web applications. It provides a robust starting point with pre-configured components, security policies, session management, and localization features, ensuring compliance with GC standards.

## Features

- **GCFoundation.Common**: Shared utilities and models used across the foundation.
- **GCFoundation.Components**: Pre-built UI components and helpers integrated with the Government of Canada Design System (GCDS).
  - [fdcp-autocomplete](GCFoundation.Components/Documentation/fdcp-autocomplete/README.md): remote-data autocomplete, and when to use it instead of fdcp-searchable-select.
- **GCFoundation.Security**: Security middleware, including Content Security Policy (CSP) and other headers tuned for GC web apps.
- **GCFoundation.Web**: The main web application project demonstrating the implementation of the foundation.

## Quick Start

This guide shows how to create a new .NET 10 ASP.NET MVC app using the latest GCFoundation NuGet packages.

### Prerequisites

- **.NET 10 SDK** installed
- **PowerShell** or a terminal

### 1. Create Solution and MVC Project

```powershell
mkdir MyGCApp && cd MyGCApp
dotnet new sln -n MyGCApp
dotnet new mvc -n MyGCApp.Web -f net10.0
dotnet sln add MyGCApp.Web
```

### 2. Add GCFoundation Packages

```powershell
dotnet add MyGCApp.Web package GCFoundation.Common
dotnet add MyGCApp.Web package GCFoundation.Components
dotnet add MyGCApp.Web package GCFoundation.Security
# (Optional) Additional packages
dotnet add MyGCApp.Web package cloudscribe.Web.Navigation
dotnet add MyGCApp.Web package cloudscribe.Web.Localization
```

### 3. Configuration

#### Update `Program.cs`

Register GCFoundation services and middleware.

```csharp
// Register GCFoundation services
builder.Services.AddGCFoundationComponents(builder.Configuration);
builder.Services.AddGCFoundationContentPolicies(builder.Configuration);
builder.Services.AddGCFoundationSession(builder.Configuration);

// Enable GCFoundation middleware
app.UseMiddleware<GCFoundationComponentsMiddleware>();
app.UseMiddleware<GCFoundationContentPoliciesMiddleware>();
app.UseMiddleware<GCFoundationLanguageMiddleware>();

// Or use convenience extensions
app.UseGCFoundationComponents();
app.UseGCFoundationContentPolicies();
app.UseGCFoundationSession();
```

#### Update `appsettings.json`

Add the `GCFoundation` configuration section.

```json
{
  "GCFoundation": {
    "Components": {
      "UseCdn": true,
      "CdnBaseUrl": "https://cdn.design-system.alpha.canada.ca",
      "InjectGcds": true
    },
    "ContentPolicies": {
      "ReportOnly": false,
      "ReportUri": "/csp-report",
      "AdditionalScriptSrc": [
        "https://www.googletagmanager.com",
        "https://www.google-analytics.com"
      ],
      "AdditionalStyleSrc": [],
      "AdditionalImgSrc": ["data:"],
      "AdditionalConnectSrc": []
    },
    "Localization": {
      "DefaultCulture": "en-CA",
      "SupportedCultures": ["en-CA", "fr-CA"]
    }
  }
}
```

### 4. Build and Run

```powershell
dotnet build
dotnet run --project MyGCApp.Web
```

## Architecture

- **Components**: Injects required GCDS/GCFoundation assets in a predictable way.
- **Security**: Applies Content Security Policy and related headers tuned for GC web apps.
- **Session**: Provides defaults for cookie/session handling.
- **Localization**: Supports request localization and language middleware.

## License

[MIT License](LICENSE)

---

# GCFoundation

**GCFoundation** est une fondation complète .NET 10 ASP.NET MVC conçue pour la création d'applications web du Gouvernement du Canada (GC). Elle fournit un point de départ robuste avec des composants préconfigurés, des politiques de sécurité, une gestion de session et des fonctionnalités de localisation, assurant la conformité aux normes du GC.

## Fonctionnalités

- **GCFoundation.Common** : Utilitaires et modèles partagés utilisés dans toute la fondation.
- **GCFoundation.Components** : Composants d'interface utilisateur préfabriqués et aides intégrés au Système de conception du gouvernement du Canada (SCGC).
  - [fdcp-autocomplete](GCFoundation.Components/Documentation/fdcp-autocomplete/README.md) : saisie semi-automatique sur données distantes, et quand l'utiliser plutôt que fdcp-searchable-select (documentation en anglais).
- **GCFoundation.Security** : Middleware de sécurité, incluant la politique de sécurité de contenu (CSP) et d'autres en-têtes optimisés pour les applications web du GC.
- **GCFoundation.Web** : Le projet d'application web principal démontrant l'implémentation de la fondation.

## Démarrage Rapide

Ce guide montre comment créer une nouvelle application .NET 10 ASP.NET MVC en utilisant les derniers paquets NuGet GCFoundation.

### Prérequis

- **.NET 10 SDK** installé
- **PowerShell** ou un terminal

### 1. Créer la solution et le projet MVC

```powershell
mkdir MyGCApp && cd MyGCApp
dotnet new sln -n MyGCApp
dotnet new mvc -n MyGCApp.Web -f net10.0
dotnet sln add MyGCApp.Web
```

### 2. Ajouter les paquets GCFoundation

```powershell
dotnet add MyGCApp.Web package GCFoundation.Common
dotnet add MyGCApp.Web package GCFoundation.Components
dotnet add MyGCApp.Web package GCFoundation.Security
# (Facultatif) Paquets supplémentaires
dotnet add MyGCApp.Web package cloudscribe.Web.Navigation
dotnet add MyGCApp.Web package cloudscribe.Web.Localization
```

### 3. Configuration

#### Mettre à jour `Program.cs`

Enregistrer les services et le middleware GCFoundation.

```csharp
// Enregistrer les services GCFoundation
builder.Services.AddGCFoundationComponents(builder.Configuration);
builder.Services.AddGCFoundationContentPolicies(builder.Configuration);
builder.Services.AddGCFoundationSession(builder.Configuration);

// Activer le middleware GCFoundation
app.UseMiddleware<GCFoundationComponentsMiddleware>();
app.UseMiddleware<GCFoundationContentPoliciesMiddleware>();
app.UseMiddleware<GCFoundationLanguageMiddleware>();

// Ou utiliser les extensions pratiques
app.UseGCFoundationComponents();
app.UseGCFoundationContentPolicies();
app.UseGCFoundationSession();
```

#### Mettre à jour `appsettings.json`

Ajouter la section de configuration `GCFoundation`.

```json
{
  "GCFoundation": {
    "Components": {
      "UseCdn": true,
      "CdnBaseUrl": "https://cdn.design-system.alpha.canada.ca",
      "InjectGcds": true
    },
    "ContentPolicies": {
      "ReportOnly": false,
      "ReportUri": "/csp-report",
      "AdditionalScriptSrc": [
        "https://www.googletagmanager.com",
        "https://www.google-analytics.com"
      ],
      "AdditionalStyleSrc": [],
      "AdditionalImgSrc": ["data:"],
      "AdditionalConnectSrc": []
    },
    "Localization": {
      "DefaultCulture": "en-CA",
      "SupportedCultures": ["en-CA", "fr-CA"]
    }
  }
}
```

### 4. Construire et Exécuter

```powershell
dotnet build
dotnet run --project MyGCApp.Web
```

## Architecture

- **Composants** : Injecte les actifs requis du SCGC/GCFoundation de manière prévisible.
- **Sécurité** : Applique la politique de sécurité de contenu et les en-têtes associés optimisés pour les applications web du GC.
- **Session** : Fournit des valeurs par défaut pour la gestion des cookies/sessions.
- **Localisation** : Prend en charge la localisation des requêtes et le middleware de langue.

## Licence

[Licence MIT](LICENSE)
