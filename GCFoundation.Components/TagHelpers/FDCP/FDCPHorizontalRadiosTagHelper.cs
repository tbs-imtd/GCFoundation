using GCFoundation.Components.Resources;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using ValidationResources = GCFoundation.Components.Resources.Validation;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// Renders a responsive horizontal radio group using cards around its options.
    /// </summary>
    [HtmlTargetElement("fdcp-horizontal-radios", Attributes = "for, items")]
    [HtmlTargetElement("fdcp-horizontal-radios", Attributes = "items, name")]
    public class FDCPHorizontalRadiosTagHelper : FDCPBaseFormComponentTagHelper
    {
        /// <summary>
        /// Gets or sets the options rendered by the radio group.
        /// </summary>
        [HtmlAttributeName("items")]
        public IEnumerable<SelectListItem> Items { get; set; } = [];

        /// <summary>
        /// Gets or sets the radio group legend.
        /// </summary>
        [HtmlAttributeName("legend")]
        public string? Legend { get; set; }

        /// <summary>
        /// Gets or sets the optional index text rendered above the legend.
        /// </summary>
        [HtmlAttributeName("index")]
        public string? Index { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the index text is displayed.
        /// </summary>
        [HtmlAttributeName("display-index")]
        public bool DisplayIndex { get; set; }

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            FormFieldContext field = ResolveFormField(new FormFieldResolveOptions
            {
                Label = Legend,
                Hint = Hint,
                Value = Value
            });

            string componentId = TagBuilder.CreateSanitizedId(field.Id, "_");
            string legendId = $"{componentId}_legend";
            string hintId = $"{componentId}_hint";
            string errorId = $"{componentId}_error";
            string requiredMessage = ValidationResources.Field_Required_Generic;
            string requiredSummaryMessage = ValidationResources.Field_Required_Summary.Replace(
                "{0}",
                field.Label,
                StringComparison.Ordinal);
            string? selectedValue = ResolveSelectedValue(field);
            bool isRequired = field.Required && !field.Disabled;
            string? existingClasses = output.Attributes["class"]?.Value?.ToString();

            output.TagName = "fieldset";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("id", componentId);
            output.Attributes.SetAttribute("class", "fdcp-horizontal-radios");
            output.Attributes.SetAttribute("aria-labelledby", legendId);
            output.Attributes.SetAttribute("data-fdcp-horizontal-radios", string.Empty);
            output.Attributes.SetAttribute("data-required-message", requiredMessage);
            output.Attributes.SetAttribute("data-required-summary-message", requiredSummaryMessage);
            output.Attributes.SetAttribute("lang", Lang.ToString());

            string outerClasses = string.IsNullOrWhiteSpace(existingClasses)
                ? "fdcp-horizontal-radios__outer"
                : $"fdcp-horizontal-radios__outer {existingClasses}";

            output.PreElement.SetHtmlContent($"""
                <gcds-container class="{Encode(outerClasses)}"
                                padding="300">
                """);
            output.PostElement.SetHtmlContent("</gcds-container>");

            if (!string.IsNullOrWhiteSpace(field.Hint))
            {
                output.Attributes.SetAttribute("aria-describedby", hintId);
            }

            if (field.Disabled)
            {
                output.Attributes.SetAttribute("disabled", string.Empty);
            }

            var html = new StringBuilder();
            AppendLegend(html, field, legendId, isRequired, Index, DisplayIndex);
            AppendHint(html, field.Hint, hintId);
            if (isRequired)
            {
                AppendError(html, requiredMessage, errorId);
            }
            html.AppendLine("""<div class="fdcp-horizontal-radios__grid d-grid">""");

            int optionIndex = 0;
            foreach (SelectListItem item in Items)
            {
                bool disabled = field.Disabled || item.Disabled;
                bool selected = selectedValue != null
                    ? string.Equals(selectedValue, item.Value, StringComparison.Ordinal)
                    : item.Selected;

                AppendOption(
                    html,
                    field,
                    item,
                    componentId,
                    optionIndex,
                    disabled,
                    selected,
                    isRequired);

                optionIndex++;
            }

            html.AppendLine("</div>");
            output.Content.SetHtmlContent(html.ToString());
        }

        private string? ResolveSelectedValue(FormFieldContext field)
        {
            if (ViewContext?.ModelState?.TryGetValue(field.Name, out var entry) == true &&
                !string.IsNullOrEmpty(entry.AttemptedValue))
            {
                return entry.AttemptedValue;
            }

            return field.Value;
        }

        private static void AppendLegend(
            StringBuilder html,
            FormFieldContext field,
            string legendId,
            bool isRequired,
            string? index,
            bool displayIndex)
        {
            html.Append(CultureInfo.InvariantCulture, $"""
                <legend class="fdcp-horizontal-radios__legend" id="{Encode(legendId)}">
                """);

            if (displayIndex && !string.IsNullOrWhiteSpace(index))
            {
                html.Append(CultureInfo.InvariantCulture, $"""
                    <span class="fdcp-horizontal-radios__index">{Encode(index)}</span>
                    """);
            }

            html.Append(CultureInfo.InvariantCulture, $"""
                    {Encode(field.Label)}
                """);

            if (isRequired)
            {
                html.Append(CultureInfo.InvariantCulture, $"""
                    <span class="fdcp-horizontal-radios__required"> ({Encode(Localization.Required)})</span>
                    """);
            }

            html.AppendLine("</legend>");
        }

        private static void AppendHint(StringBuilder html, string hint, string hintId)
        {
            if (string.IsNullOrWhiteSpace(hint))
            {
                return;
            }

            html.AppendLine(CultureInfo.InvariantCulture, $"""
                <p class="fdcp-horizontal-radios__hint" id="{Encode(hintId)}">{Encode(hint)}</p>
                """);
        }

        private static void AppendError(
            StringBuilder html,
            string message,
            string errorId)
        {
            html.AppendLine(CultureInfo.InvariantCulture, $"""
                <gcds-error-message class="fdcp-horizontal-radios__error"
                                    id="{Encode(errorId)}"
                                    message-id="{Encode(errorId)}"
                                    data-fdcp-horizontal-radios-error hidden>{Encode(message)}</gcds-error-message>
                """);
        }

        private static void AppendOption(
            StringBuilder html,
            FormFieldContext field,
            SelectListItem item,
            string componentId,
            int optionIndex,
            bool disabled,
            bool selected,
            bool isRequired)
        {
            string optionId = TagBuilder.CreateSanitizedId( $"{componentId}_{optionIndex}_{item.Value}", "_");
            string requiredAttribute = isRequired ? " required" : string.Empty;
            string disabledAttribute = disabled ? " disabled" : string.Empty;
            string checkedAttribute = selected ? " checked" : string.Empty;

            html.AppendLine("""
                <div class="fdcp-card fdcp-horizontal-radios__card">
                    <div class="fdcp-card-body">
                """);

            html.AppendLine(CultureInfo.InvariantCulture, $"""
                <div class="fdcp-horizontal-radios__option">
                    <input class="fdcp-horizontal-radios__input"
                           type="radio"
                           id="{Encode(optionId)}"
                           name="{Encode(field.Name)}"
                           value="{Encode(item.Value)}"{requiredAttribute}{disabledAttribute}{checkedAttribute}>
                    <label class="fdcp-horizontal-radios__label" for="{Encode(optionId)}">
                        <span class="fdcp-horizontal-radios__label-text">{Encode(item.Text)}</span>
                    </label>
                </div>
                """);

            html.AppendLine("""
                    </div>
                </div>
                """);
        }

        private static string Encode(string? value)
        {
            return HtmlEncoder.Default.Encode(value ?? string.Empty);
        }

    }
}
