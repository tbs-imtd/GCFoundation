using GCFoundation.Components.Models;
using GCFoundation.Components.Resources;
using GCFoundation.Components.Settings;
using GCFoundation.Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Claims;

namespace GCFoundation.Components.Services
{
    /// <summary>
    /// Service for creating and managing account view models.
    /// Provides helper methods to build AccountViewModel instances from various sources.
    /// </summary>
    public class AccountService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GCFoundationAccountSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountService"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="accountSettings">The account settings.</param>
        public AccountService(
            IHttpContextAccessor httpContextAccessor,
            IOptions<GCFoundationAccountSettings> accountSettings)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _settings = accountSettings?.Value ?? throw new ArgumentNullException(nameof(accountSettings));
        }

        /// <summary>
        /// Creates a AccountViewModel from the current HTTP context.
        /// Extracts user information from claims and session data.
        /// </summary>
        /// <returns>A configured AccountViewModel instance.</returns>
        public AccountViewModel CreateViewModelFromContext()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
            {
                return CreateAnonymousViewModel();
            }

            var user = context.User;
            var isAuthenticated = user?.Identity?.IsAuthenticated ?? false;

            if (!isAuthenticated)
            {
                return CreateAnonymousViewModel();
            }

            var viewModel = new AccountViewModel
            {
                IsAuthenticated = true,
                Settings = _settings,
                CurrentCulture = LanguageUtility.GetCurrentApplicationLanguage(),
                UserName = GetClaimValue(user, ClaimTypes.Name),
                UserEmail = GetClaimValue(user, ClaimTypes.Email),
                FirstName = GetClaimValue(user, ClaimTypes.GivenName),
                LastName = GetClaimValue(user, ClaimTypes.Surname),
                UserRole = GetClaimValue(user, ClaimTypes.Role),
                Department = GetClaimValue(user, "department") ?? GetClaimValue(user, "dept")
            };

            // Try to get login time from claims (stateless approach)
            var loginTimeClaim = GetClaimValue(user, "login_time") ?? GetClaimValue(user, "auth_time");
            if (!string.IsNullOrEmpty(loginTimeClaim))
            {
                if (long.TryParse(loginTimeClaim, out var unixTime))
                {
                    // Handle Unix timestamp (common in JWT tokens)
                    viewModel.LoginTime = DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime;
                }
                else if (DateTime.TryParse(loginTimeClaim, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var loginTime))
                {
                    // Handle ISO datetime string
                    viewModel.LoginTime = loginTime;
                }
            }

            // Calculate session expiry based on JWT expiration or configured timeout
            var expClaim = GetClaimValue(user, "exp");
            if (!string.IsNullOrEmpty(expClaim) && long.TryParse(expClaim, out var expUnixTime))
            {
                // Use JWT expiration time
                viewModel.SessionExpiry = DateTimeOffset.FromUnixTimeSeconds(expUnixTime).UtcDateTime;
            }

            return viewModel;
        }

        /// <summary>
        /// Creates the account menu items configured by the application and appends
        /// the standard profile and sign-out actions when enabled.
        /// </summary>
        /// <returns>The account menu items to render.</returns>
        public IReadOnlyList<AccountMenuItemViewModel> CreateAccountMenuItems()
        {
            var isEnglish = LanguageUtility.IsEnglish();
            var menuItems = _settings.MenuItems
                .Where(item => item.Enabled && !string.IsNullOrWhiteSpace(item.Url))
                .Select(item => new AccountMenuItemViewModel
                {
                    Text = !isEnglish && !string.IsNullOrWhiteSpace(item.TextFr)
                        ? item.TextFr
                        : item.TextEn,
                    Url = item.Url,
                    Icon = item.Icon
                })
                .ToList();

            if (_settings.ShowProfileLink
                && !string.IsNullOrWhiteSpace(_settings.ProfileUrl)
                && !ContainsUrl(menuItems, _settings.ProfileUrl))
            {
                menuItems.Add(new AccountMenuItemViewModel
                {
                    Text = Authentication.ProfileButton,
                    Url = _settings.ProfileUrl,
                    Icon = "profile"
                });
            }

            if (_settings.ShowLogoutButton
                && !string.IsNullOrWhiteSpace(_settings.LogoutUrl)
                && !ContainsUrl(menuItems, _settings.LogoutUrl))
            {
                menuItems.Add(new AccountMenuItemViewModel
                {
                    Text = Authentication.AccountSignOut,
                    Url = _settings.LogoutUrl,
                    Icon = "sign-out"
                });
            }

            return menuItems;
        }

        /// <summary>
        /// Creates a AccountViewModel for a specific user with provided information.
        /// Useful for testing or when user information is available from other sources.
        /// </summary>
        /// <param name="userName">The user's full name.</param>
        /// <param name="userEmail">The user's email address.</param>
        /// <param name="firstName">The user's first name.</param>
        /// <param name="lastName">The user's last name.</param>
        /// <param name="userRole">The user's role.</param>
        /// <param name="department">The user's department.</param>
        /// <param name="loginTime">The time the user logged in.</param>
        /// <param name="sessionExpiry">The time the session expires.</param>
        /// <returns>A configured AccountViewModel instance.</returns>
        public AccountViewModel CreateViewModel(
            string? userName = null,
            string? userEmail = null,
            string? firstName = null,
            string? lastName = null,
            string? userRole = null,
            string? department = null,
            DateTime? loginTime = null,
            DateTime? sessionExpiry = null)
        {
            return new AccountViewModel
            {
                IsAuthenticated = true,
                Settings = _settings,
                CurrentCulture = LanguageUtility.GetCurrentApplicationLanguage(),
                UserName = userName,
                UserEmail = userEmail,
                FirstName = firstName,
                LastName = lastName,
                UserRole = userRole,
                Department = department,
                LoginTime = loginTime,
                SessionExpiry = sessionExpiry
            };
        }

        /// <summary>
        /// Creates a AccountViewModel for anonymous (not authenticated) users.
        /// </summary>
        /// <returns>A AccountViewModel configured for anonymous users.</returns>
        public AccountViewModel CreateAnonymousViewModel()
        {
            return new AccountViewModel
            {
                IsAuthenticated = false,
                Settings = _settings,
                CurrentCulture = LanguageUtility.GetCurrentApplicationLanguage()
            };
        }

        /// <summary>
        /// Gets a claim value from the current user's claims.
        /// </summary>
        /// <param name="user">The claims principal.</param>
        /// <param name="claimType">The type of claim to retrieve.</param>
        /// <returns>The claim value or null if not found.</returns>
        private static string? GetClaimValue(ClaimsPrincipal? user, string claimType)
        {
            return user?.FindFirst(claimType)?.Value;
        }

        private static bool ContainsUrl(
            IEnumerable<AccountMenuItemViewModel> menuItems,
            string url)
        {
            return menuItems.Any(item =>
                string.Equals(item.Url, url, StringComparison.OrdinalIgnoreCase));
        }
    }
}

