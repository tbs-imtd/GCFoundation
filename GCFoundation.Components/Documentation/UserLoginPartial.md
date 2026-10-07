# User Login Partial Documentation

Note: For a complete end-to-end view of authentication methods and session guidance, see `AuthenticationAndSession.md` (single consolidated guide).

## Overview

The User Login Partial is a configurable, localized component for displaying user authentication information in GCFoundation applications. It supports both English and French languages and can be customized to show different user information based on your application's requirements.

## Features

- **Fully Bilingual**: Automatic English/French localization support
- **Highly Configurable**: Control what user information is displayed
- **Multiple Display Modes**: Compact header-inline and full expanded view
- **Session Management**: Display session timeout warnings and countdowns
- **User Avatar Support**: Show user initials or custom avatars
- **GCDS Integration**: Uses Government of Canada Design System components
- **Responsive Design**: Works on mobile and desktop devices
- **Accessibility**: Proper ARIA labels and semantic markup
- **Easy Integration**: Works with existing authentication systems

## Installation and Configuration

### 1. Update appsettings.json

Add the user login settings to your `appsettings.json` file:

```json
{
  "UserLoginSettings": {
    "ShowUserName": true,
    "ShowUserEmail": false,
    "ShowLoginTime": false,
    "ShowSessionTimeout": true,
    "ShowLogoutButton": true,
    "ShowProfileLink": false,
    "ProfileUrl": "/profile",
    "LogoutUrl": "/authentication/logout",
    "LoginUrl": "/authentication/login",
    "ContainerCssClasses": "mb-200",
    "ShowUserAvatar": false,
    "CustomGreetingKey": null,
    "Position": "header",
    "DisplayMode": "Dropdown",
    "MenuItems": [
      {
        "TextEn": "My profile",
        "TextFr": "Mon profil",
        "Url": "/profile",
        "Icon": "profile"
      }
    ]
  }
}
```

### 2. Register Services

The services are automatically registered when you call `AddGCFoundationComponents()` in your `Program.cs`:

```csharp
builder.Services.AddGCFoundationComponents(builder.Configuration);
```

This registers:
- `GCFoundationUserLoginSettings` configuration
- `UserLoginService` for creating view models

### 3. Enable in Layout

The foundation layout (`_FoundationLayout.cshtml`) already supports login partials. Simply set the partial name in your controller:

```csharp
public IActionResult Index()
{
    ViewData["LoginPartialViewName"] = "_UserLoginPartial";
    return View();
}
```

## Configuration Options

### GCFoundationUserLoginSettings Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ShowUserName` | bool | true | Show the user's full name |
| `ShowUserEmail` | bool | false | Show the user's email address |
| `ShowLoginTime` | bool | false | Show when the user logged in |
| `ShowSessionTimeout` | bool | true | Show session timeout information |
| `ShowLogoutButton` | bool | true | Show the logout button |
| `ShowProfileLink` | bool | false | Show a link to user profile |
| `ProfileUrl` | string | null | URL for the profile page |
| `LogoutUrl` | string | "/authentication/logout" | URL for logout action |
| `LoginUrl` | string | "/authentication/login" | URL for login action |
| `ContainerCssClasses` | string | "mb-200" | CSS classes for the container |

| `ShowUserAvatar` | bool | false | Show user avatar/initials |
| `CustomGreetingKey` | string | null | Custom localization key for greeting |
| `Position` | string | "header" | Position in the layout |
| `DisplayMode` | string | "Inline" | Header rendering style: `Inline` or `Dropdown` |
| `MenuButtonLabel` | string | null | Optional text in the dropdown trigger; defaults to generated initials |
| `MenuItems` | array | [] | Additional application-provided account menu items; enabled profile and sign-out actions are appended automatically |

## Usage Examples

### Basic Usage in Controller

```csharp
[Route("dashboard")]
public class DashboardController : GCFoundationBaseController
{
    public IActionResult Index()
    {
        // Enable the user login partial in the layout
        ViewData["LoginPartialViewName"] = "_UserLoginPartial";
        return View();
    }
}
```

### Custom User Information

```csharp
public class ProfileController : GCFoundationBaseController
{
    private readonly UserLoginService _userLoginService;
    
    public ProfileController(UserLoginService userLoginService)
    {
        _userLoginService = userLoginService;
    }
    
    public IActionResult Index()
    {
        // Create custom user model
        var userModel = _userLoginService.CreateViewModel(
            userName: "John Doe",
            userEmail: "john.doe@canada.ca",
            firstName: "John",
            lastName: "Doe",
            userRole: "Senior Developer",
            department: "Digital Services",
            loginTime: DateTime.UtcNow.AddMinutes(-30)
        );
        
        return View(userModel);
    }
}
```

### Using in Views

```razor
@using GCFoundation.Components.Models
@using GCFoundation.Components.Services
@inject UserLoginService UserLoginService

@{
    // Get user information from current context
    var loginModel = UserLoginService.CreateViewModelFromContext();
}

<partial name="_UserLoginPartial" model="loginModel" />
```

### Header-inline variant (default)

When `Position` is set to `"header"` (default), the component renders a compact, right-aligned inline variant directly beneath the site header (menu + breadcrumbs):

- Text: "Signed in as: NAME" / "Connecté en tant que : NOM"
- Button: "Sign out" / "Se déconnecter"

No additional markup is required beyond rendering the partial as shown above. The inline layout uses the CSS class `.user-login-inline`.

## Localization

### Resource Keys

The following localization keys are available in English and French:

| Key | English | French |
|-----|---------|--------|
| `WelcomeMessage` | "Welcome, {0}!" | "Bienvenue, {0}!" |
| `LoggedInAs` | "Logged in as" | "Connecté en tant que" |
| `LoginTime` | "Logged in at" | "Connecté à" |
| `SessionExpiry` | "Session expires at" | "Session expire à" |
| `SessionTimeRemaining` | "Session expires in {0} minute(s)" | "Session expire dans {0} minute(s)" |
| `SessionExpiringSoon` | "Your session expires soon" | "Votre session expire bientôt" |
| `LoginButton` | "Sign In" | "Se connecter" |
| `LogoutButton` | "Sign Out" | "Se déconnecter" |
| `ProfileButton` | "Profile" | "Profil" |

### Custom Greeting Messages

You can create custom greeting messages by setting the `CustomGreetingKey` property:

```json
{
  "UserLoginSettings": {
    "CustomGreetingKey": "CustomWelcome"
  }
}
```

Then add the key to your resource files:

**UserLogin.en.resx**:
```xml
<data name="CustomWelcome" xml:space="preserve">
  <value>Hello, {0}! Welcome back.</value>
</data>
```

**UserLogin.fr.resx**:
```xml
<data name="CustomWelcome" xml:space="preserve">
  <value>Bonjour, {0}! Bon retour.</value>
</data>
```

## Integration with Authentication

### Claims-Based Authentication

The `UserLoginService` automatically extracts user information from claims:

```csharp
// The service looks for these standard claims:
// - ClaimTypes.Name (user name)
// - ClaimTypes.Email (email address)
// - ClaimTypes.GivenName (first name)
// - ClaimTypes.Surname (last name)
// - ClaimTypes.Role (user role)
// - "department" or "dept" (department/organization)
```

### Session-Based Login Time

The service looks for session data to determine login time:

```csharp
// In your authentication controller
public IActionResult Login()
{
    // Set login time in session
    HttpContext.Session.SetString("UserSessionStarted", DateTime.UtcNow.ToString("o"));
    
    // Your authentication logic here
    
    return RedirectToAction("Index", "Home");
}
```

### Custom User Information

You can override the automatic context extraction by creating custom view models:

```csharp
public class CustomAuthController : Controller
{
    private readonly UserLoginService _userLoginService;
    
    public IActionResult GetUserInfo()
    {
        // Get user info from your custom source (database, external API, etc.)
        var userInfo = GetUserFromDatabase(User.Identity.Name);
        
        var userModel = _userLoginService.CreateViewModel(
            userName: userInfo.FullName,
            userEmail: userInfo.Email,
            firstName: userInfo.FirstName,
            lastName: userInfo.LastName,
            userRole: userInfo.JobTitle,
            department: userInfo.Department,
            loginTime: userInfo.LastLoginTime
        );
        
        return Json(userModel);
    }
}
```

## Styling and Customization

### CSS Classes

The component uses GCDS and Foundation CSS classes compiled via the Components SCSS bundle. You can customize the appearance by:

1. **Container Classes**: Set `ContainerCssClasses` in configuration
2. **Custom CSS**: Override styles in your application's CSS
3. **GCDS Themes**: Use different GCDS component themes

#### SCSS sources

The styles live in the Components project and are compiled to `wwwroot/css/foundation.min.css`:

- `wwwroot/src/scss/components/_fdcp-user-login.scss` (main component styles, includes `.user-login-inline`)
- `wwwroot/src/scss/components/_fdcp-demo-panel.scss` (example-only panel styles used in docs/examples)

Build pipeline compiles these with the Components `gulpfile.js`. No inline CSS is used so CSP nonces are not required for styles.

If you must add inline styles or scripts, use the nonce helper:

```razor
@using GCFoundation.Components.Helpers
@{
    var nonce = CspNonceHelper.AddNonceToDirective(Context, DirectiveType.Style);
}
<style nonce="@nonce">/* your minimal inline styles */</style>
```

### Example Custom Styling

```css
.user-login-partial .user-greeting {
    font-size: 1.25rem;
    color: var(--gcds-color-primary);
}

.user-login-partial .session-info {
    background: var(--gcds-color-background-warning);
    border-left: 4px solid var(--gcds-color-warning);
}

@media (max-width: 768px) {
    .user-login-partial .user-actions {
        flex-direction: column;
        gap: 0.25rem;
    }
}
```

## Accessibility

The component includes several accessibility features:

- **ARIA Labels**: Proper labeling for screen readers
- **Semantic HTML**: Uses appropriate HTML elements
- **Keyboard Navigation**: Full keyboard accessibility
- **Color Contrast**: Meets WCAG guidelines through GCDS components
- **Screen Reader Support**: Descriptive text for all interactive elements

## Troubleshooting

### Common Issues

1. **Partial not displaying**: Ensure `LoginPartialViewName` is set in ViewData
2. **Localization not working**: Verify resource files are included in build
3. **Session timeout not showing**: Check session configuration and login time in session
4. **Custom settings not applied**: Verify appsettings.json configuration section name

### Debug Information

You can inspect the user login model in your views:

```razor
@{
    var debugModel = UserLoginService.CreateViewModelFromContext();
}

<pre>
IsAuthenticated: @debugModel.IsAuthenticated
DisplayName: @debugModel.DisplayName
Email: @debugModel.UserEmail
Login Time: @debugModel.FormattedLoginTime
Session Expiry: @debugModel.FormattedSessionExpiry
Minutes Until Expiry: @debugModel.MinutesUntilExpiry
</pre>
```

## Examples

The Web project includes a complete example implementation:

- **Controller**: `UserLoginExampleController.cs`
- **Views**: `Views/UserLoginExample/`
- **Routes**: `/examples/user-login`

Visit the examples to see different configurations and use cases in action.

