using System.Diagnostics.CodeAnalysis;

namespace GCFoundation.Components.Settings
{
    /// <summary>
    /// Configuration settings for the user login display partial.
    /// Controls the appearance and behavior of user login information display.
    /// </summary>
    public class GCFoundationUserLoginSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to show the user's full name when logged in.
        /// Default is true.
        /// </summary>
        public bool ShowUserName { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to show the user's email when logged in.
        /// Default is false for privacy.
        /// </summary>
        public bool ShowUserEmail { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the login time.
        /// Default is false.
        /// </summary>
        public bool ShowLoginTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the authentication timeout countdown.
        /// Default is true. Works with JWT expiration or cookie expiration claims.
        /// </summary>
        public bool ShowSessionTimeout { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to show the logout button.
        /// Default is true.
        /// </summary>
        public bool ShowLogoutButton { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to show the user profile link.
        /// Default is false.
        /// </summary>
        public bool ShowProfileLink { get; set; }

        /// <summary>
        /// Gets or sets the URL for the user profile page.
        /// Only used if ShowProfileLink is true.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "String is more suitable for configuration and view helpers")]
        public string? ProfileUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL for the logout action.
        /// Default points to /authentication/logout.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "String is more suitable for configuration and view helpers")]
        [StringSyntax(StringSyntaxAttribute.Uri)]
        public string LogoutUrl { get; set; } = "/authentication/logout";

        /// <summary>
        /// Gets or sets the URL for the login action.
        /// Default points to /authentication/login.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "String is more suitable for configuration and view helpers")]
        [StringSyntax(StringSyntaxAttribute.Uri)]
        public string LoginUrl { get; set; } = "/authentication/login";

        /// <summary>
        /// Gets or sets the CSS classes to apply to the login container.
        /// Default uses GCFoundation and GCDS classes.
        /// </summary>
        public string ContainerCssClasses { get; set; } = "mb-200";

        /// <summary>
        /// Gets or sets a value indicating whether to show user avatar/initials.
        /// Default is false.
        /// </summary>
        public bool ShowUserAvatar { get; set; }

        /// <summary>
        /// Gets or sets the custom greeting message key for localization.
        /// If not set, uses default "WelcomeMessage" key.
        /// </summary>
        public string? CustomGreetingKey { get; set; }

        /// <summary>
        /// Gets or sets the position of the login partial.
        /// Options: "header", "sidebar", "content", "footer".
        /// Default is "header".
        /// </summary>
        public string Position { get; set; } = "header";
    }
}

