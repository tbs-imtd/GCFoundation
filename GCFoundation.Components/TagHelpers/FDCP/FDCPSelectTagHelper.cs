using GCFoundation.Common.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;
using System.Text;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// Renders a custom dropdown (select) component.
    /// Use &lt;fdcp-select&gt; in your Razor views to generate a dropdown list.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fdcp-select for=&quot;@Model.SelectedCountry&quot; items=&quot;@(new[] { new SelectListItem { Text = &quot;Canada&quot;, Value = &quot;CA&quot; }, new SelectListItem { Text = &quot;United States&quot;, Value = &quot;US&quot; } })&quot;&gt;
    /// &lt;/fdcp-select&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("fdcp-select", Attributes = "for, items")]
    [HtmlTargetElement("fdcp-select", Attributes = "items, name")]
    public class FDCPSelectTagHelper : FDCPBaseFormComponentTagHelper
    {
        /// <summary>
        /// Gets or sets the default selected value in the dropdown.
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// Label text for the select. Used when <c>for</c> is not specified,
        /// or overrides the model display name when <c>for</c> is specified.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// The list of selectable options for the dropdown.
        /// </summary>
        [HtmlAttributeName("items")]
        public IEnumerable<SelectListItem> Items { get; set; } = new List<SelectListItem>();

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            FormFieldContext field = ResolveFormField(new FormFieldResolveOptions
            {
                Label = Label,
                Hint = Hint,
                Value = Value
            });

            output.TagName = "gcds-select";
            output.TagMode = TagMode.StartTagAndEndTag;

            AddAttributeIfNotNull(output, "name", field.Name);
            AddAttributeIfNotNull(output, "select-id", field.Id);
            AddAttributeIfNotNull(output, "class", "gcds-select");
            AddAttributeIfNotNull(output, "label", field.Label);
            AddAttributeIfNotNull(output, "lang", Lang);
            AddAttributeIfNotNull(output, "hint", field.Hint);
            AddAttributeIfNotNull(output, "default-value", DefaultValue);

            AddBooleanAttribute(output, "required", field.Required);
            AddAttributeIfNotNull(output, "validate-on", "blur");

            string? errorMessage = ResolveModelStateError(field.Name);
            AddAttributeIfNotNull(output, "error-message", errorMessage);

            string? selectedValue = field.Value ?? field.Model?.ToString();
            var sb = new StringBuilder();

            foreach (var item in Items)
            {
                var selected = selectedValue == item.Value ? " selected" : "";
                sb.AppendLine(CultureInfo.InvariantCulture, $"<option value='{item.Value}'{selected}>{item.Text}</option>");
            }

            output.Content.SetHtmlContent(sb.ToString());
        }
    }
}