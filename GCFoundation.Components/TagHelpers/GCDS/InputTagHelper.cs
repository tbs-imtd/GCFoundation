using GCFoundation.Components.Enums;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// Represents a custom TagHelper for rendering an input element with a label and other associated attributes.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;gcds-input for=&quot;Email&quot; label=&quot;Email&quot; autocomplete=&quot;off&quot;&gt;&lt;/gcds-input&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("gcds-input", Attributes = "for")]
    public class InputTagHelper : BaseFormComponentTagHelper
    {
        /// <summary>
        /// Gets or sets the autocomplete behavior for the input element.
        /// Defaults to <see cref="AutocompleteType.off"/>.
        /// </summary>
        public AutocompleteType Autocomplete { get; set; } = AutocompleteType.off;

        /// <summary>
        /// Gets or sets whether to hide the label.
        /// </summary>
        public bool HideLabel { get; set; }

        /// <summary>
        /// Gets or sets the ID for the input element. 
        /// If not set, it is auto-derived from the `For.Name` property.
        /// </summary>
        public string? InputId { get; set; }

        /// <summary>
        /// Gets or sets the label for the input element.
        /// If not set, it is auto-derived from the `DisplayName` attribute or `For.Name`.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Gets or sets the size of the input field in characters.
        /// </summary>
        public int? Size { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            FormFieldContext field = ResolveFormField(new FormFieldResolveOptions
            {
                Id = InputId,
                Label = Label
            });

            AddAttributeIfNotNull(output, "input-id", field.Id);
            AddAttributeIfNotNull(output, "label", field.Label);
            AddAttributeIfNotNull(output, "hide-label", HideLabel);
            AddAttributeIfNotNull(output, "autocomplete", Autocomplete);
            AddAttributeIfNotNull(output, "size", Size);

            base.Process(context, output);
        }
    }
}