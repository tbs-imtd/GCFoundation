using GCFoundation.Components.Controllers;
using GCFoundation.Components.Models;
using GCFoundation.Components.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace GCFoundation.Web.Controllers
{
    /// <summary>
    /// Example controller demonstrating stateless authentication approaches for Azure cloud.
    /// Shows JWT, Azure AD, and cookie-based authentication without sessions.
    /// </summary>
    [Route("examples/stateless-auth")]
    public class StatelessAuthExampleController : GCFoundationBaseController
    {
        private readonly AccountService _accountService;
        private static readonly string[] LoginOptions = [
            "Cookie-based authentication",
            "JWT token authentication", 
            "Azure AD authentication",
            "External OAuth providers"
        ];
        
        private static readonly string[] BenefitsArray = [
            "No server-side session storage needed",
            "Scales automatically in Azure cloud",
            "Integrated with Microsoft identity platform",
            "Single sign-on across applications",
            "Multi-factor authentication support"
        ];

        /// <summary>
        /// Initializes a new instance of the <see cref="StatelessAuthExampleController"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="accountService">The account service.</param>
        public StatelessAuthExampleController(
            ILogger<StatelessAuthExampleController> logger,
            AccountService accountService) 
            : base(logger)
        {
            _accountService = accountService;
        }

        /// <summary>
        /// Shows stateless authentication examples.
        /// </summary>
        /// <returns>The view with authentication examples.</returns>
        [HttpGet("")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Stateless Authentication Examples";
            ViewData["AccountPartialViewName"] = "_RealWorldAccount";
            
            return View();
        }

        /// <summary>
        /// Example: Simulate cookie-based authentication without sessions.
        /// This is suitable for Azure cloud where you don't want session state.
        /// </summary>
        /// <returns>Redirect with authentication cookie set.</returns>
        [HttpPost("cookie-login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CookieLogin()
        {
            // Create claims for the user (these would normally come from your authentication system)
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, "John Doe"),
                new(ClaimTypes.Email, "john.doe@canada.ca"),
                new(ClaimTypes.GivenName, "John"),
                new(ClaimTypes.Surname, "Doe"),
                new(ClaimTypes.Role, "Senior Developer"),
                new("department", "Digital Services"),
                new("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)),
                new("login_time", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)),
                // Set expiration time (e.g., 8 hours from now)
                new("exp", DateTimeOffset.UtcNow.AddHours(8).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture))
            };

            var claimsIdentity = new ClaimsIdentity(claims, "DemoAuth");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false, // Don't persist across browser sessions
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                "DemoAuth",
                new ClaimsPrincipal(claimsIdentity),
                authProperties).ConfigureAwait(false);

            SetPageNotification(new PageNotification
            {
                Title = "Cookie Authentication Successful",
                Message = "User authenticated using stateless cookie approach. No server-side session storage required.",
                AlertType = Components.Enums.AlertType.success
            });

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Example: Logout from cookie authentication.
        /// </summary>
        /// <returns>Redirect after logout.</returns>
        [HttpPost("cookie-logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CookieLogout()
        {
            await HttpContext.SignOutAsync("DemoAuth").ConfigureAwait(false);

            SetPageNotification(new PageNotification
            {
                Title = "Logout Successful",
                Message = "User has been logged out. Authentication cookie removed.",
                AlertType = Components.Enums.AlertType.info
            });

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Example: Shows how JWT claims would work.
        /// In a real app, the JWT would be created by your authentication service.
        /// </summary>
        /// <returns>JSON with example JWT claims structure.</returns>
        [HttpGet("jwt-example")]
        public IActionResult JwtExample()
        {
            // Example of what JWT claims would look like
            var jwtClaims = new
            {
                sub = "user123", // Subject (user ID)
                name = "Jane Smith",
                email = "jane.smith@canada.ca",
                given_name = "Jane",
                family_name = "Smith",
                role = "Product Manager",
                department = "Digital Innovation",
                iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), // Issued at
                exp = DateTimeOffset.UtcNow.AddHours(8).ToUnixTimeSeconds(), // Expires
                auth_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() // Authentication time
            };

            return Json(new
            {
                message = "Example JWT claims structure for stateless authentication",
                claims = jwtClaims,
                note = "In a real application, these claims would be embedded in a signed JWT token"
            });
        }

        /// <summary>
        /// Example: Shows Azure AD integration approach.
        /// </summary>
        /// <returns>Information about Azure AD integration.</returns>
        [HttpGet("azure-ad-example")]
        public IActionResult AzureAdExample()
        {
            return Json(new
            {
                message = "Azure AD Integration Example",
                setup = new
                {
                    authentication_scheme = "AzureAD",
                    redirect_for_login = "Challenge(\"AzureAD\")",
                    redirect_for_logout = "SignOut(\"Cookies\", \"AzureAD\")",
                    user_claims_source = "Azure AD token",
                    session_management = "Not required - Azure AD handles token lifecycle"
                },
                claims_mapping = new
                {
                    name = "name or preferred_username",
                    email = "email or upn",
                    given_name = "given_name",
                    family_name = "family_name",
                    role = "roles array",
                    department = "department",
                    auth_time = "auth_time",
                    exp = "exp"
                },
                benefits = BenefitsArray
            });
        }

        /// <summary>
        /// Gets current user information from claims (stateless approach).
        /// </summary>
        /// <returns>JSON with current user information.</returns>
        [HttpGet("current-user")]
        public IActionResult GetCurrentUser()
        {
            var userModel = _accountService.CreateViewModelFromContext();

            return Json(new
            {
                isAuthenticated = userModel.IsAuthenticated,
                displayName = userModel.DisplayName,
                email = userModel.UserEmail,
                role = userModel.UserRole,
                department = userModel.Department,
                loginTime = userModel.FormattedLoginTime,
                sessionExpiry = userModel.FormattedSessionExpiry,
                minutesUntilExpiry = userModel.MinutesUntilExpiry,
                claims = User.Claims.Select(c => new { c.Type, c.Value }).ToArray()
            });
        }

        /// <summary>
        /// Example of checking authentication status without sessions.
        /// </summary>
        /// <returns>Authentication status information.</returns>
        [HttpGet("auth-status")]
        public IActionResult GetAuthStatus()
        {
            var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
            
            if (!isAuthenticated)
            {
                return Json(new
                {
                    authenticated = false,
                    message = "User not authenticated",
                    loginOptions = LoginOptions
                });
            }

            var authTime = User?.FindFirst("auth_time")?.Value;
            var exp = User?.FindFirst("exp")?.Value;
            
            DateTime? loginTime = null;
            DateTime? expiryTime = null;

            if (!string.IsNullOrEmpty(authTime) && long.TryParse(authTime, out var authUnixTime))
            {
                loginTime = DateTimeOffset.FromUnixTimeSeconds(authUnixTime).DateTime;
            }

            if (!string.IsNullOrEmpty(exp) && long.TryParse(exp, out var expUnixTime))
            {
                expiryTime = DateTimeOffset.FromUnixTimeSeconds(expUnixTime).DateTime;
            }

            return Json(new
            {
                authenticated = true,
                userName = User?.Identity?.Name,
                authenticationTime = loginTime,
                expirationTime = expiryTime,
                minutesUntilExpiry = expiryTime.HasValue ? 
                    (int?)Math.Max(0, (int)(expiryTime.Value - DateTime.UtcNow).TotalMinutes) : null,
                authenticationMethod = User?.Identity?.AuthenticationType
            });
        }
    }
}
