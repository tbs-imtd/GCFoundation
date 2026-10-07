using System.Collections.ObjectModel;

namespace GCFoundation.Components.Settings
{
    /// <summary>
    /// Defines how the user login component is rendered.
    /// </summary>
    public enum UserLoginDisplayMode
    {
        /// <summary>
        /// Renders the existing compact signed-in text with an optional sign-out button.
        /// </summary>
        Inline,

        /// <summary>
        /// Renders an account dropdown in the header.
        /// </summary>
        Dropdown
    }

    /// <summary>
    /// Configuration for a user account menu link.
    /// </summary>
    public class GCFoundationUserLoginMenuItemSettings
    {
        /// <summary>
        /// Gets or sets the English link text.
        /// </summary>
        public string TextEn { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the French link text.
        /// </summary>
        public string TextFr { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the destination URL.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "String is more suitable for configuration and view helpers")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the icon name used for styling the menu item.
        /// </summary>
        public string Icon { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether this menu item is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;
    }

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
        public string LogoutUrl { get; set; } = "/authentication/logout";

        /// <summary>
        /// Gets or sets the URL for the login action.
        /// Default points to /authentication/login.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "String is more suitable for configuration and view helpers")]
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

        /// <summary>
        /// Gets or sets how the component should render in header position.
        /// </summary>
        public UserLoginDisplayMode DisplayMode { get; set; } = UserLoginDisplayMode.Inline;

        /// <summary>
        /// Gets or sets the optional text displayed inside the dropdown trigger.
        /// When empty, the user's generated initials are shown.
        /// </summary>
        public string? MenuButtonLabel { get; set; }

        /// <summary>
        /// Gets or sets additional application-provided account menu items.
        /// Standard profile and sign-out actions are appended when enabled.
        /// </summary>
        public Collection<GCFoundationUserLoginMenuItemSettings> MenuItems { get; } = new Collection<GCFoundationUserLoginMenuItemSettings>();
    }
}

