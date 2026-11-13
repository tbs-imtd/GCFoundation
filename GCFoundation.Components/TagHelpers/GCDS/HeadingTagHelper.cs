using GCFoundation.Components.Enums;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// A tag helper for generating heading elements with customizable properties.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;gcds-heading tag=&quot;h2&quot;&gt;
    ///     Section title
    /// &lt;/gcds-heading&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("gcds-heading")]
    public class HeadingTagHelper : BaseTagHelper
    {
        /// <summary>
        /// The HTML heading tag to be used (e.g., h1, h2, etc.).
        /// Default is <see cref="HeadingTag.h2"/>.
        /// </summary>
        public required HeadingTag Tag { get; set; } = HeadingTag.h2;

        /// <summary>
        /// Whether to apply a character limit for the heading.
        /// </summary>
        public bool? CharacterLimit { get; set; }

        /// <summary>
        /// The margin-bottom CSS property value for the heading.
        /// </summary>
        public string? MarginBottom { get; set; }

        /// <summary>
        /// The margin-top CSS property value for the heading.
        /// </summary>
        public string? MarginTop { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            AddAttributeIfNotNull(output, "tag", Tag);
            if (CharacterLimit != null)
                AddAttributeIfNotNull(output, "character-limit", CharacterLimit);
            AddAttributeIfNotNull(output, "margin-bottom", MarginBottom);
            AddAttributeIfNotNull(output, "margin-top", MarginTop);

            base.Process(context, output);
        }

    }
}
