using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// Tag helper for rendering a single checkbox using the gcds-checkboxes component.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fdcp-checkbox for=&quot;@Model.AgreeToTerms&quot; legend=&quot;I agree to the terms&quot;&gt;
    /// &lt;/fdcp-checkbox&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("fdcp-checkbox", Attributes = "for")]
    [HtmlTargetElement("fdcp-checkbox", Attributes = "name")]
    public class FDCPCheckboxTagHelper : FDCPBaseFormComponentTagHelper
    {
        /// <summary>
        /// Legend or label text for the checkbox.
        /// </summary>
        [HtmlAttributeName("legend")]
        public string? Legend { get; set; }

        /// <summary>
        /// Whether the checkbox is checked when <c>for</c> is not specified,
        /// or overrides the bound model value when <c>for</c> is specified.
        /// </summary>
        [HtmlAttributeName("checked")]
        public bool Checked { get; set; }

        private sealed class CheckboxOption
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("label")]
            public string Label { get; set; } = string.Empty;

            [JsonPropertyName("value")]
            public string Value { get; set; } = string.Empty;

            [JsonPropertyName("checked")]
            public bool Checked { get; set; }

            [JsonPropertyName("hint")]
            public string? Hint { get; set; }
        }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            FormFieldContext field = ResolveFormField(new FormFieldResolveOptions
            {
                Label = Legend,
                Hint = Hint
            });

            bool currentValue = field.Model is bool modelValue
                ? modelValue
                : Checked;

            output.TagName = "gcds-checkboxes";
            output.TagMode = TagMode.StartTagAndEndTag;

            var option = new CheckboxOption
            {
                Id = field.Id,
                Label = field.Label,
                Value = "true",
                @Checked = currentValue,
                Hint = field.Hint
            };

            AddAttributeIfNotNull(output, "legend", field.Label);
            AddAttributeIfNotNull(output, "name", field.Name);
            AddAttributeIfNotNull(output, "options", JsonSerializer.Serialize(new[] { option }, CamelCaseOptions));
            AddBooleanAttribute(output, "required", field.Required);

            output.Content.SetHtmlContent(string.Empty);
        }
    }
}
