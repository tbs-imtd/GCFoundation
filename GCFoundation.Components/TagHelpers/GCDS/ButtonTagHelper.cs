using GCFoundation.Components.Enums;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// A tag helper for rendering a custom button component within the application.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;gcds-button type=&quot;submit&quot; size=&quot;regular&quot; button-role=&quot;primary&quot;&gt;
    ///     Save
    /// &lt;/gcds-button&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("gcds-button")]
    public class ButtonTagHelper : BaseTagHelper
    {
        /// <summary>
        /// The ID of the button element.
        /// </summary>
        public string? ButtonId { get; set; }

        /// <summary>
        /// The role of the button element.
        /// </summary>
        public string? ButtonRole { get; set; }

        /// <summary>
        /// Indicates whether the button is disabled.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// The name attribute of the button.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The size of the button. Default is <see cref="ButtonSizeType.regular"/>.
        /// </summary>
        public ButtonSizeType Size { get; set; } = ButtonSizeType.regular;

        /// <summary>
        /// The type of the button. Default is <see cref="ButtonType.button"/>.
        /// </summary>
        public ButtonType Type { get; set; } = ButtonType.button;

        /// <summary>
        /// The value attribute of the button.
        /// </summary>
        public string? Value { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            output.TagName = "gcds-button";

            AddAttributeIfNotNull(output, "button-id", ButtonId);
            AddAttributeIfNotNull(output, "button-role", ButtonRole);
            AddAttributeIfNotNull(output, "disabled", Disabled);
            AddAttributeIfNotNull(output, "name", Name);
            AddAttributeIfNotNull(output, "size", Size);
            AddAttributeIfNotNull(output, "type", Type);
            AddAttributeIfNotNull(output, "value", Value);

            base.Process(context, output);
        }
    }
}