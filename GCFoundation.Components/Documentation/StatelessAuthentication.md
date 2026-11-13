# DEPRECATED: Stateless Authentication for Azure Cloud

This document is deprecated. See the single consolidated guide: `AuthenticationAndSession.md`.

# Stateless Authentication for Azure Cloud

## Overview

This document explains how to implement stateless authentication with the GCFoundation Account Partial, specifically designed for Azure cloud deployments where session storage is not available or desired.

## Why Stateless Authentication?

### Problems with Session-Based Authentication in Cloud

- **No Shared State**: Multiple server instances can't share session data without Redis/SQL Server
- **Scaling Issues**: Sessions tie users to specific server instances
- **Azure Costs**: Session storage services (Redis Cache) add complexity and cost
- **Reliability**: Session storage becomes a single point of failure

### Benefits of Stateless Authentication

- **Cloud Native**: Works perfectly with Azure App Service scaling
- **No Additional Services**: No need for Redis, SQL Server session state, or other storage
- **Cost Effective**: Eliminates need for session storage services
- **High Availability**: No session storage dependencies
- **Horizontal Scaling**: Works seamlessly across multiple server instances

## Supported Authentication Methods

### 1. Cookie-Based Authentication (Recommended)

Best balance of security, simplicity, and cloud compatibility.

#### Setup

```csharp
// Program.cs
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = false; // Fixed expiration for predictability
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
        options.Cookie.SameSite = SameSiteMode.Lax;
    });
```

#### Implementation

```csharp
// Login Action
[HttpPost("login")]
public async Task<IActionResult> Login(LoginModel model)
{
    // Validate credentials (against database, external API, etc.)
    var user = await ValidateUser(model.Username, model.Password);
    
    if (user != null)
    {
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

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return RedirectToAction("Index", "Home");
    }

    return View(model);
}

// Logout Action
[HttpPost("logout")]
public async Task<IActionResult> Logout()
{
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return RedirectToAction("Index", "Home");
}
```

### 2. Azure Active Directory Integration

Perfect for enterprise applications and Office 365 integration.

#### Setup

```csharp
// Install: Microsoft.Identity.Web

// Program.cs
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
```

#### Configuration

```json
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

#### Implementation

```csharp
// Login - redirect to Azure AD
[HttpGet("login")]
public IActionResult Login()
{
    return Challenge(new AuthenticationProperties { RedirectUri = "/" }, "AzureAD");
}

// Logout
[HttpPost("logout")]
public IActionResult Logout()
{
    return SignOut("Cookies", "AzureAD");
}
```

### 3. JWT Token Authentication

Ideal for API-first applications and microservices.

#### Setup

```csharp
// Install: Microsoft.AspNetCore.Authentication.JwtBearer

// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
        };
    });
```

#### Token Creation

```csharp
// Token creation service
public string CreateToken(User user)
{
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Name, user.FullName),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
        new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
        new Claim("role", user.Role),
        new Claim("department", user.Department),
        new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
        new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddHours(8).ToUnixTimeSeconds().ToString())
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _configuration["JWT:Issuer"],
        audience: _configuration["JWT:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(8),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

## Account Partial Integration

The Account Partial automatically works with all stateless authentication methods because it reads from claims instead of sessions.

### Automatic Claim Mapping

The `AccountService` automatically maps standard claims:

```csharp
// In AccountService.CreateViewModelFromContext()
var viewModel = new AccountViewModel
{
    IsAuthenticated = user?.Identity?.IsAuthenticated ?? false,
    UserName = GetClaimValue(user, ClaimTypes.Name),
    UserEmail = GetClaimValue(user, ClaimTypes.Email),
    FirstName = GetClaimValue(user, ClaimTypes.GivenName),
    LastName = GetClaimValue(user, ClaimTypes.Surname),
    UserRole = GetClaimValue(user, ClaimTypes.Role),
    Department = GetClaimValue(user, "department") ?? GetClaimValue(user, "dept")
};

// Handle login time from claims
var loginTimeClaim = GetClaimValue(user, "login_time") ?? GetClaimValue(user, "auth_time");
if (!string.IsNullOrEmpty(loginTimeClaim))
{
    if (long.TryParse(loginTimeClaim, out var unixTime))
    {
        // Handle Unix timestamp (common in JWT tokens)
        viewModel.LoginTime = DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime;
    }
    else if (DateTime.TryParse(loginTimeClaim, out var loginTime))
    {
        // Handle ISO datetime string
        viewModel.LoginTime = loginTime;
    }
}

// Handle expiration from claims
var expClaim = GetClaimValue(user, "exp");
if (!string.IsNullOrEmpty(expClaim) && long.TryParse(expClaim, out var expUnixTime))
{
    viewModel.SessionExpiry = DateTimeOffset.FromUnixTimeSeconds(expUnixTime).UtcDateTime;
}
```

### Usage in Controllers

```csharp
public class HomeController : GCFoundationBaseController
{
    public IActionResult Index()
    {
        // Enable the account partial - it will automatically read from claims
        ViewData["AccountPartialViewName"] = "_AccountPartial";
        return View();
    }
}
```

### Usage in Views

```razor
@inject AccountService AccountService

@{
    // Automatically creates model from current user's claims
    var loginModel = AccountService.CreateViewModelFromContext();
}

<partial name="_AccountPartial" model="loginModel" />
```

## Azure Deployment Configuration

### App Service Settings

```json
{
  "AccountSettings": {
    "ShowUserName": true,
    "ShowUserEmail": true,
    "ShowLoginTime": true,
    "ShowSessionTimeout": true,
    "ShowLogoutButton": true,
    "LogoutUrl": "/account/logout",
    "LoginUrl": "/account/login"
  }
}
```

### Key Vault Integration

For production secrets:

```csharp
// Program.cs
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
        new DefaultAzureCredential());
}
```

### Application Insights

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry();

// Track authentication events
builder.Services.AddSingleton<ITelemetryInitializer, AuthenticationTelemetryInitializer>();
```

## Security Considerations

### Cookie Security

```csharp
.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;           // Prevent XSS
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
    options.Cookie.SameSite = SameSiteMode.Lax;             // CSRF protection
    options.SlidingExpiration = false;        // Predictable expiration
    options.Events.OnRedirectToLogin = context =>
    {
        // Custom login redirect logic
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
})
```

### JWT Security

```csharp
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ClockSkew = TimeSpan.Zero,                // Strict timing
    RequireExpirationTime = true,
    RequireSignedTokens = true
};
```

### Azure AD Security

- Use HTTPS redirect URIs only
- Configure appropriate app permissions
- Enable conditional access policies
- Use managed identity for Key Vault access

## Troubleshooting

### Common Issues

1. **Claims not appearing**: Check claim mapping in authentication configuration
2. **Expiration not working**: Verify `exp` claim is set correctly
3. **User info not displaying**: Ensure claims use standard claim types
4. **Azure AD issues**: Check tenant configuration and app registration

### Debugging

```csharp
// Debug claims in controller
public IActionResult DebugClaims()
{
    return Json(User.Claims.Select(c => new { c.Type, c.Value }));
}
```

### Logging

```csharp
// Program.cs
builder.Logging.AddApplicationInsights();

// In controllers
_logger.LogInformation("User {UserName} logged in at {LoginTime}", 
    User.Identity.Name, DateTime.UtcNow);
```

## Examples

See the `StatelessAuthExampleController` for complete working examples of all authentication methods.

## Migration from Session-Based Auth

1. Replace session storage with claims
2. Update login logic to create claims instead of session variables
3. Modify logout to clear authentication cookies
4. Test account partial continues to work
5. Remove session configuration from Program.cs
6. Update Azure deployment to remove session storage dependencies
