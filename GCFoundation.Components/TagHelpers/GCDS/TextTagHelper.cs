using GCFoundation.Components.Enums;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// Represents a tag helper for rendering text elements with customizable properties like display style, size, and margins.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;gcds-text text-role=&quot;primary&quot; size=&quot;body&quot;&gt;
    ///     Supporting text.
    /// &lt;/gcds-text&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("gcds-text")]
    public class TextTagHelper : BaseTagHelper
    {
        /// <summary>
        /// Gets or sets a value indicating whether the character limit is enabled.
        /// </summary>
        public bool? CharacterLimit { get; set; }

        /// <summary>
        /// Gets or sets the display style of the text element. Default is <see cref="TextDisplay.Block"/>.
        /// </summary>
        public TextDisplay Display { get; set; } = TextDisplay.Block;

        /// <summary>
        /// Gets or sets the margin-bottom value for the text element. Default is <c>300</c>.
        /// </summary>
        public string? MarginBottom { get; set; } 

        /// <summary>
        /// Gets or sets the margin-top value for the text element. Default is <c>0</c>.
        /// </summary>
        public string? MarginTop { get; set; }

        /// <summary>
        /// Gets or sets the size of the text. Default is <see cref="TextSize.body"/>.
        /// </summary>
        public TextSize Size { get; set; } = TextSize.body;

        /// <summary>
        /// Gets or sets the role of the text, defining its emphasis. Default is <see cref="TextRole.Primary"/>.
        /// </summary>
        public TextRole TextRole { get; set; } = TextRole.Primary;

        /// <summary>
        /// Processes the tag helper by adding the relevant attributes to the output based on the properties.
        /// </summary>
        /// <param name="context">The context of the tag helper.</param>
        /// <param name="output">The output to which the attributes will be added.</param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (CharacterLimit != null)
                AddAttributeIfNotNull(output, "character-limit", CharacterLimit);
            AddAttributeIfNotNullWithCaseConversion(output, "display", Display);
            AddAttributeIfNotNull(output, "display", Display);
            AddAttributeIfNotNull(output, "margin-bottom", MarginBottom);
            AddAttributeIfNotNull(output, "margin-top", MarginTop);
            AddAttributeIfNotNull(output, "size", Size);
            AddAttributeIfNotNull(output, "text-role", TextRole);
            base.Process(context, output);
        }
    }
}
