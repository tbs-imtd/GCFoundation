using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// Gets or sets the name of the radio group. This name is used to group the radio buttons and associate them together.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;gcds-radios for=&quot;@Model.ContactMethod&quot; legend=&quot;How should we contact you?&quot; options='[{&quot;label&quot;:&quot;Email&quot;,&quot;value&quot;:&quot;email&quot;},{&quot;label&quot;:&quot;Phone&quot;,&quot;value&quot;:&quot;phone&quot;}]'&gt;&lt;/gcds-radios&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("gcds-radios")]
    public class RadiosTagHelper : BaseFormComponentTagHelper
    {
        /// <summary>
        /// The label for the checkbox element.
        /// </summary>
        public required string Legend { get; set; }

        /// <summary>
        /// Gets or sets the options for the radio buttons, provided as a JSON string.
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

            output.TagName = "gcds-radios";

            AddAttributeIfNotNull(output, "name", field.Name);
            AddAttributeIfNotNull(output, "legend", field.Label);
            AddAttributeIfNotNull(output, "hint", field.Hint);
            AddAttributeIfNotNull(output, "value", field.Value);

            AddAttributeIfNotNull(output, "options", Options);
        }
    }
}