using GCFoundation.Components.Settings;
using System.Globalization;

namespace GCFoundation.Components.Models
{
    /// <summary>
    /// View model for displaying user login information in the login partial.
    /// Contains user data and configuration settings for display.
    /// </summary>
    public class UserLoginViewModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether the user is currently authenticated.
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// Gets or sets the user's full name.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        public string? UserEmail { get; set; }

        /// <summary>
        /// Gets or sets the user's first name for personalized greetings.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Gets or sets the user's last name.
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Gets or sets the user's initials for avatar display.
        /// </summary>
        public string? UserInitials { get; set; }

        /// <summary>
        /// Gets or sets the login time as a UTC DateTime.
        /// </summary>
        public DateTime? LoginTime { get; set; }

        /// <summary>
        /// Gets or sets the session expiry time as a UTC DateTime.
        /// </summary>
        public DateTime? SessionExpiry { get; set; }

        /// <summary>
        /// Gets or sets the user's role or job title.
        /// </summary>
        public string? UserRole { get; set; }

        /// <summary>
        /// Gets or sets the user's department or organization.
        /// </summary>
        public string? Department { get; set; }

        /// <summary>
        /// Gets or sets the configuration settings for the login display.
        /// </summary>
        public GCFoundationUserLoginSettings Settings { get; set; } = new();

        /// <summary>
        /// Gets or sets the current culture for localization.
        /// </summary>
        public string CurrentCulture { get; set; } = "en-CA";

        /// <summary>
        /// Gets the formatted login time based on the current culture.
        /// </summary>
        public string? FormattedLoginTime
        {
            get
            {
                if (!LoginTime.HasValue) return null;
                
                var culture = new System.Globalization.CultureInfo(CurrentCulture);
                return LoginTime.Value.ToLocalTime().ToString("g", culture);
            }
        }

        /// <summary>
        /// Gets the formatted session expiry time based on the current culture.
        /// </summary>
        public string? FormattedSessionExpiry
        {
            get
            {
                if (!SessionExpiry.HasValue) return null;
                
                var culture = new System.Globalization.CultureInfo(CurrentCulture);
                return SessionExpiry.Value.ToLocalTime().ToString("g", culture);
            }
        }

        /// <summary>
        /// Gets the minutes remaining until session expiry.
        /// </summary>
        public int? MinutesUntilExpiry
        {
            get
            {
                if (!SessionExpiry.HasValue) return null;
                
                var remaining = SessionExpiry.Value - DateTime.UtcNow;
                return remaining.TotalMinutes > 0 ? (int)Math.Ceiling(remaining.TotalMinutes) : 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the session is about to expire (within 5 minutes).
        /// </summary>
        public bool IsSessionExpiringSoon => MinutesUntilExpiry.HasValue && MinutesUntilExpiry <= 5;

        /// <summary>
        /// Gets the user's display name (FirstName + LastName or UserName as fallback).
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName))
                {
                    return $"{FirstName} {LastName}".Trim();
                }
                return UserName ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets a non-empty label for identifying the account menu owner.
        /// </summary>
        public string AccountOwnerLabel =>
            !string.IsNullOrWhiteSpace(DisplayName)
                ? DisplayName
                : !string.IsNullOrWhiteSpace(Settings.MenuButtonLabel)
                    ? Settings.MenuButtonLabel
                    : GeneratedInitials;

        /// <summary>
        /// Gets the user's initials (first letter of first and last name).
        /// </summary>
        public string GeneratedInitials
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(UserInitials))
                    return UserInitials.ToUpperInvariant();

                var initials = string.Empty;
                if (!string.IsNullOrWhiteSpace(FirstName))
                    initials += FirstName[0];
                if (!string.IsNullOrWhiteSpace(LastName))
                    initials += LastName[0];

                return initials.Length > 0 ? initials.ToUpperInvariant() : (UserName?.Length > 0 ? UserName[0].ToString(CultureInfo.InvariantCulture).ToUpperInvariant() : "U");
            }
        }
    }
}

