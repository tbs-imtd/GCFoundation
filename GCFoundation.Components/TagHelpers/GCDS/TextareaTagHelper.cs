using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// Represents a tag helper for rendering a textarea input element with customizable properties like label, row count, and maximum length (GCDS v1).
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;gcds-textarea for=&quot;Notes&quot; label=&quot;Notes&quot; textarea-id=&quot;notes&quot; rows=&quot;5&quot;&gt;&lt;/gcds-textarea&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("gcds-textarea")]
    public class TextareaTagHelper : BaseFormComponentTagHelper
    {
        /// <summary>
        /// Gets or sets the label for the textarea input element. This field is required.
        /// </summary>
        public required string Label { get; set; }

        /// <summary>
        /// Gets or sets the ID for the textarea element. This field is required.
        /// </summary>
        public required string TextareaId { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of characters (GCDS v1 <c>maxlength</c>).
        /// </summary>
        [HtmlAttributeName("maxlength")]
        public int MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the legacy maximum length binding; use <see cref="MaxLength"/> instead.
        /// </summary>
        [Obsolete("Use MaxLength (maxlength). character-count was removed in GCDS v1.")]
        [HtmlAttributeName("character-count")]
        public int CharacterCount
        {
            get => MaxLength;
            set => MaxLength = value;
        }

        /// <summary>
        /// When true, hides the character counter while still applying <see cref="MaxLength"/> (GCDS v1 <c>hide-limit</c>).
        /// </summary>
        public bool HideLimit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to hide the label for the textarea input element. Default is <c>false</c>.
        /// </summary>
        public bool HideLabel { get; set; }

        /// <summary>
        /// Gets or sets the number of rows for the textarea element. This controls the visible height of the textarea.
        /// </summary>
        public int Rows { get; set; }

        /// <summary>
        /// Processes the tag helper by adding the relevant attributes to the output based on the properties.
        /// </summary>
        /// <param name="context">The context of the tag helper.</param>
        /// <param name="output">The output to which the attributes will be added.</param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            AddAttributeIfNotNull(output, "label", Label);
            AddAttributeIfNotNull(output, "textarea-id", TextareaId);
            if (MaxLength > 0)
                AddAttributeIfNotNull(output, "maxlength", MaxLength);

            if (HideLimit)
                AddAttributeIfNotNull(output, "hide-limit", true);

            AddAttributeIfNotNull(output, "hide-label", HideLabel);
            AddAttributeIfNotNull(output, "rows", Rows);

            base.Process(context, output);
        }
    }
}