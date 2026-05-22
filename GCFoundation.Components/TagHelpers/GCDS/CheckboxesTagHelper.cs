using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// A tag helper for rendering a single checkbox using the gcds-checkboxes component.
    /// </summary>
    [HtmlTargetElement("gcds-checkboxes")]
    public class CheckboxesTagHelper : BaseFormComponentTagHelper
    {
        /// <summary>
        /// The label for the checkbox element.
        /// </summary>
        public required string Legend { get; set; }

        /// <summary>
        /// Gets or sets the options for the checkboxes, provided as a JSON string.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Json)]
        public required string Options { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            FormFieldContext field = ResolveFormField(new FormFieldResolveOptions
            {
                Label = Legend
            });

            output.TagName = "gcds-checkboxes";

            AddAttributeIfNotNull(output, "name", field.Name);
            AddAttributeIfNotNull(output, "legend", field.Label);
            AddAttributeIfNotNull(output, "hint", field.Hint);
            AddAttributeIfNotNull(output, "value", field.Value);

            AddAttributeIfNotNull(output, "options", Options);
        }
    }
}