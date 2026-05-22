using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace GCFoundation.Components.Models
{
    /// <summary>
    /// Represents the model for selecting a language and related application links.
    /// </summary>
    public class LanguageChooserModel
    {
        /// <summary>
        /// Title of the application in english
        /// </summary>
        public string ApplicationTitleEn { get; set; } = string.Empty;

        /// <summary>
        /// Title of the application in French
        /// </summary>
        public string ApplicationTitleFr { get; set; } = string.Empty;

        /// <summary>
        /// Link to the english version of the application
        /// </summary>
        public string EnglishAction { get; set; } = string.Empty;

        /// <summary>
        /// Link to the frech version of the application
        /// </summary>
        public string FrenchAction { get; set; } = string.Empty;

        /// <summary>
        /// Link to the term of service of the application in english
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Uri)]
        public string TermLinkEn { get; set; } = string.Empty;

        /// <summary>
        /// Link to the term of service of the application in french
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Uri)]
        public string TermLinkFr { get; set; } = string.Empty;

        /// <summary>
        /// Array of image paths for the background images.
        /// </summary>
        private readonly ReadOnlyCollection<string> _backgroundImagePaths = new ReadOnlyCollection<string>(
            new[] {
                "splash/sp-bg-1.jpg",
                "splash/sp-bg-2.jpg",
                "splash/sp-bg-3.jpg",
                "splash/sp-bg-4.jpg",
                "splash/sp-bg-5.jpg"
            }
        );

        /// <summary>
        /// Array of image path for the background image
        /// </summary>
        public ReadOnlyCollection<string> BackgroundImagePaths => _backgroundImagePaths;
    }
}
