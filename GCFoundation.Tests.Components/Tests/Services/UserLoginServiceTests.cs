using GCFoundation.Components.Services;
using GCFoundation.Components.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Claims;

namespace GCFoundation.Tests.Components.Tests.Services;

public class UserLoginServiceTests
{
    [Fact]
    public void CreateViewModelFromContext_WhenAnonymous_ReturnsAnonymousModel()
    {
        var service = CreateService(new ClaimsPrincipal());

        var model = service.CreateViewModelFromContext();

        Assert.False(model.IsAuthenticated);
        Assert.Equal("U", model.GeneratedInitials);
    }

    [Fact]
    public void CreateViewModelFromContext_WithStandardClaims_PopulatesUserDisplay()
    {
        var principal = CreateAuthenticatedPrincipal(
            new Claim(ClaimTypes.Name, "John Smith"),
            new Claim(ClaimTypes.Email, "john.smith@example.ca"),
            new Claim(ClaimTypes.GivenName, "John"),
            new Claim(ClaimTypes.Surname, "Smith"),
            new Claim(ClaimTypes.Role, "Administrator"),
            new Claim("department", "TBS"));
        var service = CreateService(principal);

        var model = service.CreateViewModelFromContext();

        Assert.True(model.IsAuthenticated);
        Assert.Equal("John Smith", model.DisplayName);
        Assert.Equal("JS", model.GeneratedInitials);
        Assert.Equal("john.smith@example.ca", model.UserEmail);
        Assert.Equal("Administrator", model.UserRole);
        Assert.Equal("TBS", model.Department);
    }

    [Fact]
    public void CreateViewModelFromContext_WithTimeClaims_UsesClaimValues()
    {
        var loginTime = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);
        var sessionExpiry = loginTime.AddMinutes(45);
        var principal = CreateAuthenticatedPrincipal(
            new Claim(ClaimTypes.Name, "John Smith"),
            new Claim("login_time", new DateTimeOffset(loginTime).ToUnixTimeSeconds().ToString()),
            new Claim("exp", new DateTimeOffset(sessionExpiry).ToUnixTimeSeconds().ToString()));
        var service = CreateService(principal);

        var model = service.CreateViewModelFromContext();

        Assert.Equal(loginTime, model.LoginTime);
        Assert.Equal(sessionExpiry, model.SessionExpiry);
    }

    [Fact]
    public void CreateAccountMenuItems_AppendsLocalizedSignOutToCustomItems()
    {
        using var culture = new CultureScope("en-CA");
        var settings = new GCFoundationUserLoginSettings
        {
            ShowLogoutButton = true,
            LogoutUrl = "/account/logout"
        };
        settings.MenuItems.Add(new GCFoundationUserLoginMenuItemSettings
        {
            TextEn = "Account settings",
            TextFr = "Paramètres du compte",
            Url = "/account/settings",
            Icon = "profile"
        });
        var service = CreateService(new ClaimsPrincipal(), settings);

        var menuItems = service.CreateAccountMenuItems();

        Assert.Collection(
            menuItems,
            item =>
            {
                Assert.Equal("Account settings", item.Text);
                Assert.Equal("/account/settings", item.Url);
            },
            item =>
            {
                Assert.Equal("Sign out", item.Text);
                Assert.Equal("/account/logout", item.Url);
                Assert.Equal("sign-out", item.Icon);
            });
    }

    [Fact]
    public void CreateAccountMenuItems_DoesNotDuplicateApplicationProvidedSignOut()
    {
        using var culture = new CultureScope("fr-CA");
        var settings = new GCFoundationUserLoginSettings
        {
            ShowLogoutButton = true,
            LogoutUrl = "/account/logout"
        };
        settings.MenuItems.Add(new GCFoundationUserLoginMenuItemSettings
        {
            TextEn = "Leave",
            TextFr = "Quitter",
            Url = "/account/logout",
            Icon = "sign-out"
        });
        var service = CreateService(new ClaimsPrincipal(), settings);

        var menuItems = service.CreateAccountMenuItems();

        var menuItem = Assert.Single(menuItems);
        Assert.Equal("Quitter", menuItem.Text);
    }

    [Fact]
    public void CreateAccountMenuItems_WhenProfileEnabled_AppendsLocalizedProfile()
    {
        using var culture = new CultureScope("fr-CA");
        var settings = new GCFoundationUserLoginSettings
        {
            ShowProfileLink = true,
            ProfileUrl = "/account/profile",
            ShowLogoutButton = false
        };
        var service = CreateService(new ClaimsPrincipal(), settings);

        var menuItems = service.CreateAccountMenuItems();

        var menuItem = Assert.Single(menuItems);
        Assert.Equal("Profil", menuItem.Text);
        Assert.Equal("/account/profile", menuItem.Url);
    }

    private static UserLoginService CreateService(
        ClaimsPrincipal principal,
        GCFoundationUserLoginSettings? loginSettings = null)
    {
        var context = new DefaultHttpContext { User = principal };
        var accessor = new HttpContextAccessor { HttpContext = context };

        return new UserLoginService(
            accessor,
            Options.Create(loginSettings ?? new GCFoundationUserLoginSettings()));
    }

    private static ClaimsPrincipal CreateAuthenticatedPrincipal(params Claim[] claims)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _originalCulture = CultureInfo.CurrentCulture;
        private readonly CultureInfo _originalUiCulture = CultureInfo.CurrentUICulture;

        public CultureScope(string cultureName)
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _originalCulture;
            CultureInfo.CurrentUICulture = _originalUiCulture;
        }
    }
}
