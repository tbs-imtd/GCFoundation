using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Json;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// Renders a custom radio button component using the gcds-radios element.
    /// Use &lt;fdcp-radios&gt; in your Razor views to generate a radio button group.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fdcp-radios for=&quot;@Model.Gender&quot; items=&quot;@(new[] { new SelectListItem { Text = &quot;Woman&quot;, Value = &quot;woman&quot; }, new SelectListItem { Text = &quot;Man&quot;, Value = &quot;man&quot; } })&quot; legend=&quot;Gender&quot;&gt;
    /// &lt;/fdcp-radios&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("fdcp-radios", Attributes = "for, items")]
    [HtmlTargetElement("fdcp-radios", Attributes = "items, name")]
    public class FDCPRadiosTagHelper : FDCPBaseFormComponentTagHelper
    {
        /// <summary>
        /// The list of items to be rendered as radio buttons.
        /// Each item should have a text (label) and value (for the radio button).
        /// </summary>
        [HtmlAttributeName("items")]
        public IEnumerable<SelectListItem> Items { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// Legend text for the radio group. Used when <c>for</c> is not specified,
        /// or overrides the model display name when <c>for</c> is specified.
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

            string selectedValue = field.Value ?? string.Empty;

            output.TagName = "gcds-radios";
            output.TagMode = TagMode.StartTagAndEndTag;

            var options = Items.Select(item => new
            {
                id = $"{field.Id}_{item.Value}",
                label = item.Text,
                value = item.Value,
                @checked = selectedValue == item.Value
            });

            AddAttributeIfNotNull(output, "name", field.Name);
            AddAttributeIfNotNull(output, "legend", field.Label);
            AddAttributeIfNotNull(output, "hint", field.Hint);
            AddAttributeIfNotNull(output, "options", JsonSerializer.Serialize(options));
            AddBooleanAttribute(output, "required", field.Required);

            output.Content.SetHtmlContent(string.Empty);
        }
    }
}
