using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// A tag helper for rendering a single checkbox using the gcds-checkboxes component.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;gcds-checkboxes for=&quot;@Model.Interests&quot; legend=&quot;Select your interests&quot; options='[{&quot;label&quot;:&quot;Research&quot;,&quot;value&quot;:&quot;research&quot;},{&quot;label&quot;:&quot;Policy&quot;,&quot;value&quot;:&quot;policy&quot;}]'&gt;&lt;/gcds-checkboxes&gt;
    /// </code>
    /// </example>
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