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
    /// Renders a responsive Likert scale using radio options displayed in cards.
    /// </summary>
    [HtmlTargetElement("fdcp-likert-scale", Attributes = "for, items")]
    [HtmlTargetElement("fdcp-likert-scale", Attributes = "items, name")]
    public class FDCPLikertScaleTagHelper : FDCPBaseFormComponentTagHelper
    {
        /// <summary>
        /// Gets or sets the options rendered by the Likert scale.
        /// </summary>
        [HtmlAttributeName("items")]
        public IEnumerable<SelectListItem> Items { get; set; } = [];

        /// <summary>
        /// Gets or sets the Likert scale legend.
        /// </summary>
        [HtmlAttributeName("legend")]
        public string? Legend { get; set; }

        /// <summary>
        /// Gets or sets the optional index text rendered above the legend.
        /// </summary>
        [HtmlAttributeName("index")]
        public string? Index { get; set; }

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
            string requiredMessage = ValidationResources.LikertScale_Required;
            string requiredSummaryMessage = ValidationResources.Field_Required_Summary.Replace(
                "{0}",
                field.Label,
                StringComparison.Ordinal);
            string? selectedValue = ResolveSelectedValue(field);
            bool isRequired = field.Required && !field.Disabled;
            string? errorMessage = ResolveModelStateError(field.Name);
            bool hasServerError = !string.IsNullOrWhiteSpace(errorMessage);
            string resolvedErrorMessage = errorMessage ?? requiredMessage;
            string? existingClasses = output.Attributes["class"]?.Value?.ToString();
            List<SelectListItem> items = Items.ToList();

            output.TagName = "fieldset";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("id", componentId);
            string componentClass = "fdcp-likert-scale";
            if (hasServerError) componentClass += " fdcp-likert-scale--error";
            output.Attributes.SetAttribute("class", componentClass);
            output.Attributes.SetAttribute(
                "aria-labelledby",
                string.IsNullOrWhiteSpace(field.Hint) ? legendId : $"{legendId} {hintId}");
            output.Attributes.SetAttribute("data-fdcp-likert-scale", string.Empty);
            output.Attributes.SetAttribute("data-required-message", requiredMessage);
            output.Attributes.SetAttribute("data-required-summary-message", requiredSummaryMessage);
            output.Attributes.SetAttribute("lang", Lang.ToString());

            string outerClasses = string.IsNullOrWhiteSpace(existingClasses)
                ? "fdcp-likert-scale__outer"
                : $"fdcp-likert-scale__outer {existingClasses}";

            output.PreElement.SetHtmlContent($"""
                <gcds-container class="{Encode(outerClasses)}"
                                padding="300">
                """);
            output.PostElement.SetHtmlContent("</gcds-container>");

            if (field.Disabled)
            {
                output.Attributes.SetAttribute("disabled", string.Empty);
            }

            var html = new StringBuilder();
            AppendLegend(html, field, legendId, isRequired, Index);
            AppendHint(html, field.Hint, hintId);
            if (isRequired || hasServerError)
            {
                AppendError(html, resolvedErrorMessage, errorId, hidden: !hasServerError);
            }
            html.AppendLine("""<div class="fdcp-likert-scale__grid">""");

            int optionIndex = 0;
            foreach (SelectListItem item in items)
            {
                bool disabled = field.Disabled || item.Disabled;
                bool selected = !string.IsNullOrEmpty(selectedValue)
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
                    isRequired,
                    hasServerError ? errorMessage : null);

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
            string? index)
        {
            html.Append(CultureInfo.InvariantCulture, $"""
                <legend class="fdcp-likert-scale__legend" id="{Encode(legendId)}">
                """);

            if (!string.IsNullOrWhiteSpace(index))
            {
                html.Append(CultureInfo.InvariantCulture, $"""
                    <span class="fdcp-likert-scale__index">{Encode(index)}</span>
                    """);
            }

            html.Append(CultureInfo.InvariantCulture, $"""
                    {Encode(field.Label)}
                """);

            if (isRequired)
            {
                html.Append(CultureInfo.InvariantCulture, $"""
                    <span class="fdcp-likert-scale__required"> ({Encode(Localization.Required)})</span>
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
                <p class="fdcp-likert-scale__hint" id="{Encode(hintId)}">{Encode(hint)}</p>
                """);
        }

        private static void AppendError(
            StringBuilder html,
            string message,
            string errorId,
            bool hidden = true)
        {
            string hiddenAttribute = hidden ? " hidden" : string.Empty;
            html.AppendLine(CultureInfo.InvariantCulture, $"""
                <gcds-error-message class="fdcp-likert-scale__error"
                                    id="{Encode(errorId)}"
                                    message-id="{Encode(errorId)}"
                                    data-fdcp-likert-scale-error{hiddenAttribute}>{Encode(message)}</gcds-error-message>
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
            bool isRequired,
            string? errorMessage)
        {
            string optionId = TagBuilder.CreateSanitizedId( $"{componentId}_{optionIndex}_{item.Value}", "_");
            string requiredAttribute = isRequired ? " required" : string.Empty;
            string disabledAttribute = disabled ? " disabled" : string.Empty;
            string checkedAttribute = selected ? " checked" : string.Empty;
            string errorAttributes = string.IsNullOrWhiteSpace(errorMessage)
                ? string.Empty
                : $" aria-invalid=\"true\" aria-description=\"{Encode(errorMessage)}\"";

            html.AppendLine(CultureInfo.InvariantCulture, $"""
                <div class="fdcp-likert-scale__card">
                    <input class="fdcp-likert-scale__input"
                           type="radio"
                           id="{Encode(optionId)}"
                           name="{Encode(field.Name)}"
                           value="{Encode(item.Value)}"{requiredAttribute}{disabledAttribute}{checkedAttribute}{errorAttributes}>
                    <label class="fdcp-likert-scale__label" for="{Encode(optionId)}">
                        <span class="fdcp-likert-scale__label-text">{Encode(item.Text)}</span>
                    </label>
                </div>
                """);
        }

        private static string Encode(string? value)
        {
            return HtmlEncoder.Default.Encode(value ?? string.Empty);
        }

    }
}
