using System.Diagnostics.CodeAnalysis;

namespace GCFoundation.Components.Models
{
    /// <summary>
    /// Represents an error link that contains a URL (Href) and an associated error message.
    /// </summary>
    public class ErrorLink
    {
        /// <summary>
        /// Gets or sets the URL that is associated with the error link.
        /// This URL may be used to navigate to more information about the error.
        /// </summary>
        /// <value>A string representing the URL for the error link.</value>
        [StringSyntax(StringSyntaxAttribute.Uri)]
        public required string Href { get; set; }

        /// <summary>
        /// Gets or sets the message that describes the error.
        /// This message provides context for the error or further explanation.
        /// </summary>
        /// <value>A string representing the error message.</value>
        public required string Message { get; set; }
    }
}
