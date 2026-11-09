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
    /// Example controller demonstrating how to use the Account partial.
    /// This controller shows different ways to configure and display account information.
    /// </summary>
    [Route("examples/account")]
    public class AccountExampleController : GCFoundationBaseController
    {
        private readonly AccountService _accountService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountExampleController"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="accountService">The account service.</param>
        public AccountExampleController(
            ILogger<AccountExampleController> logger,
            AccountService accountService) 
            : base(logger)
        {
            _accountService = accountService;
        }

        /// <summary>
        /// Displays the main account examples page.
        /// </summary>
        /// <returns>The view with account examples.</returns>
        [HttpGet("")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Account Partial Examples";
            
            // Set the account partial to be used in the layout
            ViewData["AccountPartialViewName"] = "_ExampleAccount";
            
            return View();
        }

        /// <summary>
        /// Demonstrates how to create a custom account model programmatically.
        /// </summary>
        /// <returns>JSON result with the account model.</returns>
        [HttpGet("api/user-info")]
        public IActionResult GetUserInfo()
        {
            // Example: Create user info from authentication context or database
            var isAuthenticated = User?.Identity?.IsAuthenticated ?? false;
            
            AccountViewModel userModel;
            
            if (isAuthenticated)
            {
                userModel = _accountService.CreateViewModel(
                    userName: "Jane Smith",
                    userEmail: "jane.smith@canada.ca",
                    firstName: "Jane",
                    lastName: "Smith",
                    userRole: "Product Manager",
                    department: "Digital Services",
                    loginTime: DateTime.UtcNow.AddMinutes(-30),
                    sessionExpiry: DateTime.UtcNow.AddMinutes(10)
                );
            }
            else
            {
                userModel = _accountService.CreateAnonymousViewModel();
            }

            return Json(new
            {
                isAuthenticated = userModel.IsAuthenticated,
                displayName = userModel.DisplayName,
                userEmail = userModel.UserEmail,
                userRole = userModel.UserRole,
                department = userModel.Department,
                loginTime = userModel.FormattedLoginTime,
                sessionExpiry = userModel.FormattedSessionExpiry,
                minutesUntilExpiry = userModel.MinutesUntilExpiry,
                isSessionExpiringSoon = userModel.IsSessionExpiringSoon
            });
        }

        /// <summary>
        /// Example of how to render the account partial in different positions.
        /// </summary>
        /// <param name="position">The position where to render the account partial (header, sidebar, content, footer).</param>
        /// <returns>The view configured for the specified position.</returns>
        [HttpGet("position/{position}")]
        public IActionResult Position(string position)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(position, nameof(position));
            
            var validPositions = new[] { "header", "sidebar", "content", "footer" };
            
            if (!validPositions.Contains(position, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest($"Invalid position. Valid positions are: {string.Join(", ", validPositions)}");
            }

            ViewData["Title"] = $"Account in {position.ToUpperInvariant()} Position";
            #pragma warning disable CA1308 // Lowercase needed for CSS classes
            ViewData["AccountPosition"] = position.ToLowerInvariant(); // Lowercase needed for CSS classes
            #pragma warning restore CA1308
            ViewData["AccountPartialViewName"] = "_ExampleAccount";
            
            return View("PositionExample");
        }

        /// <summary>
        /// Simulates account for demonstration purposes using temporary cookie authentication.
        /// In a real application, this would be handled by your authentication system (Azure AD, JWT, etc.).
        /// </summary>
        /// <returns>Redirect to the examples page with user logged in.</returns>
        [HttpPost("simulate-login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SimulateLogin()
        {
            // Create temporary claims for demonstration
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, "Demo User"),
                new(ClaimTypes.Email, "demo.user@canada.ca"),
                new(ClaimTypes.GivenName, "Demo"),
                new(ClaimTypes.Surname, "User"),
                new(ClaimTypes.Role, "Example User"),
                new("department", "Demonstration Team"),
                new("auth_time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)),
                new("login_time", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)),
                new("exp", DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)) // 1 hour demo session
            };

            var identity = new ClaimsIdentity(claims, "DemoAuth");
            var principal = new ClaimsPrincipal(identity);

            // Sign in for demo (this creates a temporary authentication cookie)
            await HttpContext.SignInAsync("DemoAuth", principal, new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
            }).ConfigureAwait(false);
            
            SetPageNotification(new Components.Models.PageNotification
            {
                Title = "Demo Login Successful",
                Message = "Demo user has been authenticated. You can now see the account partial in action!",
                AlertType = Components.Enums.AlertType.success
            });

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Simulates user logout for demonstration purposes.
        /// </summary>
        /// <returns>Redirect to the examples page with user logged out.</returns>
        [HttpPost("simulate-logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SimulateLogout()
        {
            // Sign out the demo user
            await HttpContext.SignOutAsync("DemoAuth").ConfigureAwait(false);
            
            SetPageNotification(new Components.Models.PageNotification
            {
                Title = "Demo Logout Successful",
                Message = "Demo user has been signed out. The account partial now shows the anonymous state.",
                AlertType = Components.Enums.AlertType.info
            });

            return RedirectToAction("Index");
        }
    }
}

