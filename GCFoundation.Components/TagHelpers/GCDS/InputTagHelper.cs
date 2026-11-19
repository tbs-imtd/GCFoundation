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
        private GcdsInputType? _type;

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
        /// Gets or sets the maximum value (GCDS <c>max</c>). Only applies to the <c>number</c> input type.
        /// </summary>
        public string? Max { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of characters (GCDS <c>maxlength</c>).
        /// </summary>
        [HtmlAttributeName("maxlength")]
        public int? MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the minimum value (GCDS <c>min</c>). Only applies to the <c>number</c> input type.
        /// </summary>
        public string? Min { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of characters (GCDS <c>minlength</c>).
        /// </summary>
        [HtmlAttributeName("minlength")]
        public int? MinLength { get; set; }

        /// <summary>
        /// Gets or sets the virtual keyboard hint (GCDS <c>inputmode</c>): none, text, decimal, numeric, tel, search, email or url.
        /// </summary>
        [HtmlAttributeName("inputmode")]
        public string? InputMode { get; set; }

        /// <summary>
        /// Gets or sets a regular expression the value must match (GCDS <c>pattern</c>).
        /// </summary>
        public string? Pattern { get; set; }

        /// <summary>
        /// Gets or sets whether the input cannot be modified (GCDS <c>readonly</c>).
        /// </summary>
        public bool Readonly { get; set; }

        /// <summary>
        /// Gets or sets the size of the input field in characters.
        /// </summary>
        public int? Size { get; set; }

        /// <summary>
        /// Gets or sets the value granularity for number inputs (GCDS <c>step</c>): a number or <c>any</c>.
        /// </summary>
        public string? Step { get; set; }

        /// <summary>
        /// Gets or sets the suggestion options rendered as a datalist, as a JSON array (GCDS <c>suggestions</c>).
        /// </summary>
        public string? Suggestions { get; set; }

        /// <summary>
        /// Gets or sets the input type (GCDS <c>type</c>). Not rendered unless set; GCDS defaults to <see cref="GcdsInputType.text"/>.
        /// </summary>
        public GcdsInputType Type
        {
            get => _type ?? GcdsInputType.text;
            set => _type = value;
        }

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
            AddAttributeIfNotNull(output, "min", Min);
            AddAttributeIfNotNull(output, "max", Max);
            AddAttributeIfNotNull(output, "minlength", MinLength);
            AddAttributeIfNotNull(output, "maxlength", MaxLength);
            AddAttributeIfNotNull(output, "inputmode", InputMode);
            AddAttributeIfNotNull(output, "pattern", Pattern);
            AddBooleanAttribute(output, "readonly", Readonly);
            AddAttributeIfNotNull(output, "step", Step);
            AddAttributeIfNotNull(output, "suggestions", Suggestions);
            AddAttributeIfNotNull(output, "type", _type);

            base.Process(context, output);
        }
    }
}