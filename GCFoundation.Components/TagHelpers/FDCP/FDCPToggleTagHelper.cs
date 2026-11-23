using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// Renders an on/off toggle switch bound to a boolean value.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fdcp-toggle for=&quot;@Model.EmailNotifications&quot; label=&quot;Email notifications&quot;&gt;
    /// &lt;/fdcp-toggle&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("fdcp-toggle", Attributes = "for")]
    [HtmlTargetElement("fdcp-toggle", Attributes = "name")]
    public class FDCPToggleTagHelper : FDCPBaseFormComponentTagHelper
    {
        /// <summary>
        /// Label text for the toggle. Used when <c>for</c> is not specified,
        /// or overrides the model display name when <c>for</c> is specified.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Whether the toggle is on. Overrides the bound model value when specified.
        /// </summary>
        [HtmlAttributeName("checked")]
        public bool? Checked { get; set; }

        /// <summary>
        /// Visible state text shown when the toggle is on.
        /// </summary>
        public string? OnText { get; set; }

        /// <summary>
        /// Visible state text shown when the toggle is off.
        /// </summary>
        public string? OffText { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            FormFieldContext field = ResolveFormField(new FormFieldResolveOptions
            {
                Label = Label,
                Hint = Hint
            });

            string componentId = SanitizeId(field.Id);
            string hintId = $"{componentId}_hint";
            string errorId = $"{componentId}_error";
            string? errorMessage = ResolveModelStateError(field.Name);
            bool isOn = ResolveIsOn(field);

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", "fdcp-toggle");
            output.Attributes.SetAttribute("data-fdcp-toggle", string.Empty);

            var sb = new StringBuilder();
            sb.AppendLine(CultureInfo.InvariantCulture, $"<label class=\"fdcp-toggle__label gcds-label\" for=\"{Encode(componentId)}\">{Encode(field.Label)}</label>");

            if (!string.IsNullOrWhiteSpace(field.Hint))
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"<gcds-hint hint-id=\"{Encode(hintId)}\" id=\"{Encode(hintId)}\">{Encode(field.Hint)}</gcds-hint>");
            }

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"<gcds-error-message id=\"{Encode(errorId)}\" message-id=\"{Encode(errorId)}\">{Encode(errorMessage)}</gcds-error-message>");
            }

            var describedByIds = new List<string>();
            if (!string.IsNullOrWhiteSpace(field.Hint))
            {
                describedByIds.Add(hintId);
            }

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                describedByIds.Add(errorId);
            }

            string describedBy = describedByIds.Count == 0 ? string.Empty : $" aria-describedby=\"{Encode(string.Join(" ", describedByIds))}\"";
            string invalid = string.IsNullOrWhiteSpace(errorMessage) ? string.Empty : " aria-invalid=\"true\"";
            string isChecked = isOn ? " checked" : string.Empty;
            string disabled = field.Disabled ? " disabled" : string.Empty;

            sb.AppendLine("<div class=\"fdcp-toggle__control\">");
            sb.AppendLine(CultureInfo.InvariantCulture, $"<input type=\"checkbox\" role=\"switch\" class=\"fdcp-toggle__input\" id=\"{Encode(componentId)}\" name=\"{Encode(field.Name)}\" value=\"true\"{isChecked}{describedBy}{invalid}{disabled} />");
            // hidden keeps "On Off" from showing as plain text when the stylesheet fails to load.
            sb.AppendLine("<span class=\"fdcp-toggle__state\" aria-hidden=\"true\" hidden>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"<span class=\"fdcp-toggle__state-on\">{Encode(OnText ?? Resources.Localization.On)}</span>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"<span class=\"fdcp-toggle__state-off\">{Encode(OffText ?? Resources.Localization.Off)}</span>");
            sb.AppendLine("</span>");
            sb.AppendLine("</div>");

            // A disabled checkbox is never submitted, so posting "false" would overwrite the real value.
            if (!field.Disabled)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"<input type=\"hidden\" name=\"{Encode(field.Name)}\" value=\"false\" />");
            }

            output.Content.SetHtmlContent(sb.ToString());
        }

        private bool ResolveIsOn(FormFieldContext field)
        {
            if (Checked.HasValue)
            {
                return Checked.Value;
            }

            string? attemptedValue = ViewContext?.ModelState?.TryGetValue(field.Name, out var entry) == true
                ? entry?.AttemptedValue
                : null;

            if (!string.IsNullOrWhiteSpace(attemptedValue))
            {
                string firstValue = attemptedValue.Split(',')[0].Trim();
                return bool.TryParse(firstValue, out bool attempted) && attempted;
            }

            return field.Model is bool modelValue
                ? modelValue
                : bool.TryParse(field.Value, out bool value) && value;
        }

        private static string SanitizeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "fdcp_toggle";
            }

            var sb = new StringBuilder(value.Length);
            foreach (char character in value)
            {
                sb.Append(char.IsLetterOrDigit(character) || character is '_' or '-' or ':' ? character : '_');
            }

            return sb.ToString();
        }

        private static string Encode(string? value)
        {
            return HtmlEncoder.Default.Encode(value ?? string.Empty);
        }
    }
}
