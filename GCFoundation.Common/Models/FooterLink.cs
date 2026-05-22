using System.Diagnostics.CodeAnalysis;
using GCFoundation.Common.Utilities;

namespace GCFoundation.Common.Models
{
    /// <summary>
    /// Represents a link in the footer with its label and URL. Supports a single neutral
    /// <see cref="Label"/> / <see cref="Link"/> pair and/or English and French overrides.
    /// </summary>
    public class FooterLink
    {
        /// <summary>
        /// The footer link text, or the language-neutral fallback when
        /// <see cref="LabelEn"/> / <see cref="LabelFr"/> are used.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// English label when the UI culture is English. Falls back to <see cref="Label"/> when unset.
        /// </summary>
        public string? LabelEn { get; set; }

        /// <summary>
        /// French label when the UI culture is French. Falls back to <see cref="Label"/> when unset.
        /// </summary>
        public string? LabelFr { get; set; }

        /// <summary>
        /// The footer link URL, or the language-neutral fallback when
        /// <see cref="LinkEn"/> / <see cref="LinkFr"/> are used.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Uri)]
        public string? Link { get; set; }

        /// <summary>
        /// English URL when the UI culture is English. Falls back to <see cref="Link"/> when unset.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Uri)]
        public string? LinkEn { get; set; }

        /// <summary>
        /// French URL when the UI culture is French. Falls back to <see cref="Link"/> when unset.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Uri)]
        public string? LinkFr { get; set; }

        /// <summary>
        /// Resolves the label for rendering: French and English use locale-specific properties when set,
        /// otherwise <see cref="Label"/>; other cultures use <see cref="Label"/>, then English, then French.
        /// </summary>
        public string? GetLocalizedLabel() => ResolveLocalized(Label, LabelEn, LabelFr);

        /// <summary>
        /// Resolves the URL for rendering: French and English use locale-specific properties when set,
        /// otherwise <see cref="Link"/>; other cultures use <see cref="Link"/>, then English, then French.
        /// </summary>
        public string? GetLocalizedLink() => ResolveLocalized(Link, LinkEn, LinkFr);

        private static string? ResolveLocalized(string? neutral, string? en, string? fr)
        {
            if (LanguageUtility.IsFrench())
            {
                if (!string.IsNullOrWhiteSpace(fr))
                    return fr;
                return neutral;
            }

            if (LanguageUtility.IsEnglish())
            {
                if (!string.IsNullOrWhiteSpace(en))
                    return en;
                return neutral;
            }

            if (!string.IsNullOrWhiteSpace(neutral))
                return neutral;
            if (!string.IsNullOrWhiteSpace(en))
                return en;
            if (!string.IsNullOrWhiteSpace(fr))
                return fr;
            return null;
        }
    }
}
