using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Reflection.Emit;
using System.Text.Json;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// A tag helper for rendering a group of checkboxes using the gcds-checkboxes component.
    /// It binds to a model property and renders checkboxes based on the provided items.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fdcp-checkboxes for=&quot;@Model.SelectedInterests&quot; items=&quot;@(new[] { new SelectListItem { Text = &quot;Grants&quot;, Value = &quot;grants&quot; }, new SelectListItem { Text = &quot;Contributions&quot;, Value = &quot;contributions&quot; } })&quot; legend=&quot;Interests&quot;&gt;
    /// &lt;/fdcp-checkboxes&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("fdcp-checkboxes", Attributes = "for, items")]
    [HtmlTargetElement("fdcp-checkboxes", Attributes = "items, name")]
    public class FDCPCheckboxesTagHelper : FDCPBaseFormComponentTagHelper
    {
        /// <summary>
        /// The list of items to be rendered as checkboxes.
        /// Each item should have a text (label) and value (for the checkbox).
        /// </summary>
        [HtmlAttributeName("items")]
        public IEnumerable<SelectListItem> Items { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// Legend text for the checkbox group.
        /// </summary>
        [HtmlAttributeName("legend")]
        public string? Legend { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            FormFieldContext field = ResolveFormField(new FormFieldResolveOptions
            {
                Label = Legend,
                Hint = Hint,
                Value = Value
            });

            var selectedValues = GetSelectedValues(field);

            output.TagName = "gcds-checkboxes";
            output.TagMode = TagMode.StartTagAndEndTag;

            var options = Items.Select(item => new
            {
                id = $"{field.Id}_{item.Value}",
                label = item.Text,
                value = item.Value,
                @checked = selectedValues.Contains(item.Value),
            });

            AddAttributeIfNotNull(output, "name", field.Name);
            AddAttributeIfNotNull(output, "legend", field.Label);
            AddAttributeIfNotNull(output, "hint", field.Hint);
            AddAttributeIfNotNull(output, "options", JsonSerializer.Serialize(options));
            AddBooleanAttribute(output, "required", field.Required);

            output.Content.SetHtmlContent(string.Empty);
        }

        private static List<string> GetSelectedValues(FormFieldContext field)
        {
            if (field.Model is List<string> list)
            {
                return list;
            }

            if (field.Model is IEnumerable<string> values)
            {
                return values.ToList();
            }

            if (string.IsNullOrWhiteSpace(field.Value))
            {
                return new List<string>();
            }

            return field.Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
        }
    }
}
