# GCFoundation Authentication and Session (Single Guide)

## Overview

This guide consolidates all authentication and session guidance for GCFoundation into a single page. It covers multiple authentication schemes (Cookies, Azure AD, JWT, external OAuth) and the session experience (timeout reminder modal and keep-alive), and shows how the `Account` partial integrates with all of them.

## Quick Start

1. Add services
```csharp
// Program.cs
builder.Services.AddGCFoundationComponents(builder.Configuration);
```

2. Configure the account partial
```json
// appsettings.json
{
  "AccountSettings": {
    "ShowUserName": true,
    "ShowUserEmail": false,
    "ShowLoginTime": false,
    "ShowSessionTimeout": true,
    "ShowLogoutButton": true,
    "LoginUrl": "/authentication/login",
    "LogoutUrl": "/authentication/logout",
    "Position": "header"
  }
}
```

3. Render the partial
```csharp
public IActionResult Index()
{
    ViewData["AccountPartialViewName"] = "_AccountPartial";
    return View();
}
```

4. Choose an authentication method (see below). The partial reads `User.Claims`, so it works across schemes.

## Supported Authentication Schemes

### Cookie Authentication (recommended for MVC)

```csharp
// Program.cs
builder.Services
    .AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/authentication/login";
        options.LogoutPath = "/authentication/logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = false; // predictable expiry; use true for sliding
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// Example login (create claims, sign in)
var claims = new List<Claim>
{
    new(ClaimTypes.Name, user.FullName),
    new(ClaimTypes.Email, user.Email),
    new(ClaimTypes.GivenName, user.FirstName),
    new(ClaimTypes.Surname, user.LastName),
    new(ClaimTypes.Role, user.Role),
    new("department", user.Department),
    new("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
    new("exp", DateTimeOffset.UtcNow.AddHours(8).ToUnixTimeSeconds().ToString())
};
var identity = new ClaimsIdentity(claims, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
await HttpContext.SignInAsync(
    Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
    new ClaimsPrincipal(identity),
    new AuthenticationProperties { IsPersistent = false, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });
```

### Azure Active Directory (Microsoft Entra ID)

```csharp
// NuGet: Microsoft.Identity.Web
// Program.cs
builder.Services
    .AddAuthentication(Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

// appsettings.json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "yourdomain.onmicrosoft.com",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "CallbackPath": "/signin-oidc"
  }
}
```

### JWT Bearer (APIs and SPAs)

```csharp
// NuGet: Microsoft.AspNetCore.Authentication.JwtBearer
// Program.cs
builder.Services
    .AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
        };
    });
```

### External OAuth Providers

```csharp
builder.Services
    .AddAuthentication()
    .AddGoogle(o => { o.ClientId = configuration["Google:ClientId"]; o.ClientSecret = configuration["Google:ClientSecret"]; })
    .AddGitHub(o => { o.ClientId = configuration["GitHub:ClientId"]; o.ClientSecret = configuration["GitHub:ClientSecret"]; });
```

## Account Partial Integration

The `AccountService` builds the model from the current `HttpContext.User` claims, so it works across all schemes without session state.

Claim mapping used by the partial:

- Name: `ClaimTypes.Name`
- Email: `ClaimTypes.Email`
- FirstName: `ClaimTypes.GivenName`
- LastName: `ClaimTypes.Surname`
- Role: `ClaimTypes.Role`
- Department: `"department"` or `"dept"`
- LoginTime: `"auth_time"` or `"login_time"` or parsed from session fallback
- Expiration: `"exp"` (Unix seconds)

Usage in Razor:

```razor
@inject GCFoundation.Components.Services.AccountService AccountService
@{
    var loginModel = AccountService.CreateViewModelFromContext();
}
<partial name="_AccountPartial" model="loginModel" />
```

## Session Experience (Optional)

If your app uses server-side session, GCFoundation can show a session reminder modal and keep the session alive.

### Configure session settings
```json
// appsettings.json
{
  "GCFoundationSessionSetting": {
    "UseSession": true,
    "UseReminder": true,
    "SessionTimeout": 20,
    "ReminderTime": 5,
    "UseSlidingExpiration": true,
    "RefreshURL": "https://localhost:5001/authentication/refresh",
    "LogoutURL": "https://localhost:5001/authentication/logout"
  }
}
```

Ensure your layout renders the reminder modal when enabled and includes the compiled script. The Foundation layout in Components already handles this when `UseSession` and `UseReminder` are true and will render `Views/Shared/Modals/_ExtendSessionModal.cshtml` and load `wwwroot/src/js/SessionManagement.js` via the bundle.

### Refresh endpoint example
```csharp
[Route("authentication")]
public class AuthenticationController : GCFoundation.Components.Controllers.GCFoundationBaseController
{
    [HttpPost("refresh")]
    [ValidateAntiForgeryToken]
    public IActionResult RefreshSession()
    {
        HttpContext.Session.SetString("KeepAlive", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
        return Ok();
    }
}
```

## Security Recommendations

- Enforce HTTPS; set cookie `SecurePolicy = Always`.
- Set appropriate `SameSite` value based on your flows.
- For JWT, set `ClockSkew = TimeSpan.Zero` and require signed, expiring tokens.
- For Azure AD, restrict redirect URIs to HTTPS and enable MFA/conditional access.

## Troubleshooting

- Claims missing: verify your authentication handler populates standard claims.
- Expiry not shown: ensure an `exp` claim (Unix seconds) is present.
- Partial not rendering: set `ViewData["AccountPartialViewName"]` to `"_AccountPartial"` in your controller.

## Examples in This Repository

- Interactive examples: `GCFoundation.Web/Views/AccountExample/` and controller `AccountExampleController`
- Stateless auth examples: `GCFoundation.Web/Views/StatelessAuthExample/` and controller `StatelessAuthExampleController`

## Migration Notes (Session → Stateless)

1. Stop storing identity in session; issue claims at sign-in.
2. Ensure `auth_time` and `exp` are added to claims.
3. Switch to cookie/JWT/Azure AD authentication as appropriate.
4. Verify the `Account` partial displays the same info from claims.



