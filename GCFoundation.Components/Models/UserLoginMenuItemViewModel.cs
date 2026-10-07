namespace GCFoundation.Components.Models
{
    /// <summary>
    /// Represents an account menu item localized for the current request.
    /// </summary>
    public sealed class UserLoginMenuItemViewModel
    {
        /// <summary>
        /// Gets or sets the localized link text.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the destination URL.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "The value is rendered directly by Razor and may be application-relative")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the icon name.
        /// </summary>
        public string Icon { get; set; } = string.Empty;
    }
}
