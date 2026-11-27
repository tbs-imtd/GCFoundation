using GCFoundation.Components.Resources;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// Renders an autocomplete text input that looks up suggestions from a remote data source and submits the value of the selected suggestion.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fdcp-autocomplete for=&quot;@Model.EmployeeId&quot; src=&quot;@Url.Action(&quot;Search&quot;, &quot;Employees&quot;)&quot; selected-text=&quot;@Model.EmployeeName&quot;&gt;
    /// &lt;/fdcp-autocomplete&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("fdcp-autocomplete", Attributes = "for, src")]
    [HtmlTargetElement("fdcp-autocomplete", Attributes = "name, src")]
    public class FDCPAutocompleteTagHelper : FDCPBaseFormComponentTagHelper
    {
        /// <summary>
        /// URL of the data source. Receives the search term, <c>skip</c>, <c>take</c> and <c>lang</c> as query-string parameters
        /// and returns JSON shaped like <see cref="Models.FDCPAutocompleteResult"/>.
        /// </summary>
#pragma warning disable CA1056 // Razor attribute values are strings; relative URLs are expected.
        public string Src { get; set; } = string.Empty;
#pragma warning restore CA1056

        /// <summary>
        /// Label text. Used when <c>for</c> is not specified, or overrides the model display name when it is.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Text displayed for the current value, so an existing selection can be shown without calling the data source.
        /// </summary>
        public string? SelectedText { get; set; }

        /// <summary>
        /// Name of the query-string parameter that carries the search term.
        /// </summary>
        public string QueryParam { get; set; } = "q";

        /// <summary>
        /// Minimum number of characters typed before the data source is called.
        /// </summary>
        public int MinLength { get; set; } = 2;

        /// <summary>
        /// Delay, in milliseconds, between the last keystroke and the data source call.
        /// </summary>
        public int Debounce { get; set; } = 250;

        /// <summary>
        /// Number of records requested per call.
        /// </summary>
        public int PageSize { get; set; } = 50;

        /// <summary>
        /// Maximum number of records the list will load for a single search term.
        /// </summary>
        public int MaxResults { get; set; } = 1000;

        /// <summary>
        /// Whether pages of results are cached in memory for the life of the page.
        /// </summary>
        public bool Cache { get; set; } = true;

        /// <summary>
        /// Lifetime of a cached page of results, in seconds.
        /// </summary>
        public int CacheTtl { get; set; } = 300;

        /// <summary>
        /// Maximum number of pages of results kept in the cache.
        /// </summary>
        public int CacheSize { get; set; } = 50;

        /// <summary>
        /// Placeholder text for the input.
        /// </summary>
        public string? Placeholder { get; set; }

        /// <summary>
        /// Overrides the localized text announced while results load.
        /// </summary>
        public string? LoadingText { get; set; }

        /// <summary>
        /// Overrides the localized text shown when nothing matches.
        /// </summary>
        public string? NoResultsText { get; set; }

        /// <summary>
        /// Overrides the localized text announced when one result matches.
        /// </summary>
        public string? OneResultText { get; set; }

        /// <summary>
        /// Overrides the localized text announced when several results match. Use <c>{0}</c> for the count.
        /// </summary>
        public string? MultipleResultsText { get; set; }

        /// <summary>
        /// Overrides the localized text shown until enough characters are typed. Use <c>{0}</c> for the minimum length.
        /// </summary>
        public string? MinLengthText { get; set; }

        /// <summary>
        /// Overrides the localized text shown when the data source fails.
        /// </summary>
        public string? LoadErrorText { get; set; }

        /// <summary>
        /// Overrides the localized text announced to screen readers when an option is chosen. Use <c>{0}</c> for the option text.
        /// </summary>
        public string? SelectedAnnouncementText { get; set; }

        /// <summary>
        /// Overrides the localized accessible name of the clear button.
        /// </summary>
        public string? ClearText { get; set; }

        /// <summary>
        /// Overrides the localized keyboard instructions announced to assistive technology.
        /// </summary>
        public string? InstructionsText { get; set; }

        /// <inheritdoc/>
        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            if (!IsSafeUrl(Src))
            {
                throw new InvalidOperationException("fdcp-autocomplete requires a 'src' that is a relative, http or https URL.");
            }

            FormFieldContext field = ResolveFormField(new FormFieldResolveOptions
            {
                Label = Label,
                Hint = Hint,
                Value = Value
            });

            int minLength = Math.Max(1, MinLength);
            int pageSize = Math.Max(1, PageSize);
            int maxResults = Math.Max(pageSize, MaxResults);

            string componentId = SanitizeId(field.Id);
            string labelId = $"{componentId}_label";
            string inputId = $"{componentId}_input";
            string listboxId = $"{componentId}_listbox";
            string statusId = $"{componentId}_status";
            string messageId = $"{componentId}_message";
            string instructionsId = $"{componentId}_instructions";
            string hintId = $"{componentId}_hint";
            string errorId = $"{componentId}_error";

            string selectedValue = field.Value ?? string.Empty;
            string selectedText = string.IsNullOrEmpty(selectedValue) ? string.Empty : SelectedText ?? selectedValue;
            string? errorMessage = ResolveModelStateError(field.Name);
            string requiredSummary = string.Format(CultureInfo.CurrentCulture, GCFoundation.Components.Resources.Validation.Field_Required_Summary ?? string.Empty, field.Label);
            string invalidSummary = string.Format(CultureInfo.CurrentCulture, GCFoundation.Components.Resources.Validation.Autocomplete_InvalidSelection_Summary ?? string.Empty, field.Label);
            string minLengthText = string.Format(CultureInfo.CurrentCulture, MinLengthText ?? Localization.Autocomplete_MinLength, minLength);

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", "fdcp-autocomplete");
            output.Attributes.SetAttribute("id", componentId);
            output.Attributes.SetAttribute("lang", Lang.ToString());
            output.Attributes.SetAttribute("data-fdcp-autocomplete", string.Empty);
            output.Attributes.SetAttribute("data-src", Src);
            output.Attributes.SetAttribute("data-query-param", QueryParam);
            output.Attributes.SetAttribute("data-min-length", Invariant(minLength));
            output.Attributes.SetAttribute("data-debounce", Invariant(Math.Max(0, Debounce)));
            output.Attributes.SetAttribute("data-page-size", Invariant(pageSize));
            output.Attributes.SetAttribute("data-max-results", Invariant(maxResults));
            output.Attributes.SetAttribute("data-cache", Cache ? "true" : "false");
            output.Attributes.SetAttribute("data-cache-ttl", Invariant(Math.Max(0, CacheTtl)));
            output.Attributes.SetAttribute("data-cache-size", Invariant(Math.Max(1, CacheSize)));
            output.Attributes.SetAttribute("data-required", field.Required ? "true" : "false");
            output.Attributes.SetAttribute("data-loading-text", LoadingText ?? Localization.Autocomplete_Loading);
            output.Attributes.SetAttribute("data-no-results-text", NoResultsText ?? Localization.Autocomplete_NoResults);
            output.Attributes.SetAttribute("data-one-result-text", OneResultText ?? Localization.Autocomplete_OneResult);
            output.Attributes.SetAttribute("data-multiple-results-text", MultipleResultsText ?? Localization.Autocomplete_MultipleResults);
            output.Attributes.SetAttribute("data-min-length-text", minLengthText);
            output.Attributes.SetAttribute("data-load-error-text", LoadErrorText ?? Localization.Autocomplete_LoadError);
            output.Attributes.SetAttribute("data-selected-announcement-text", SelectedAnnouncementText ?? Localization.Autocomplete_Selected);
            output.Attributes.SetAttribute("data-required-message", GCFoundation.Components.Resources.Validation.Field_Required_Generic);
            output.Attributes.SetAttribute("data-required-summary-message", requiredSummary);
            output.Attributes.SetAttribute("data-invalid-message", GCFoundation.Components.Resources.Validation.Autocomplete_InvalidSelection);
            output.Attributes.SetAttribute("data-invalid-summary-message", invalidSummary);

            var sb = new StringBuilder();
            string labelContent = Encode(field.Label);
            if (field.Required)
            {
                labelContent += $" <span class=\"label--required\" aria-hidden=\"true\">({Encode(Localization.Required)})</span>";
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"<label class=\"fdcp-autocomplete__label gcds-label\" id=\"{Encode(labelId)}\" for=\"{Encode(inputId)}\">{labelContent}</label>");

            bool hasHint = !string.IsNullOrWhiteSpace(field.Hint);
            if (hasHint)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"<gcds-hint hint-id=\"{Encode(hintId)}\" id=\"{Encode(hintId)}\">{Encode(field.Hint)}</gcds-hint>");
            }

            bool hasError = !string.IsNullOrWhiteSpace(errorMessage);
            string errorHidden = hasError ? string.Empty : " hidden";
            sb.AppendLine(CultureInfo.InvariantCulture, $"<gcds-error-message id=\"{Encode(errorId)}\" message-id=\"{Encode(errorId)}\" data-fdcp-autocomplete-error{errorHidden}>{Encode(errorMessage)}</gcds-error-message>");

            var describedBy = new List<string>();
            if (hasHint)
            {
                describedBy.Add(hintId);
            }

            describedBy.Add(errorId);
            describedBy.Add(instructionsId);

            string disabled = field.Disabled ? " disabled" : string.Empty;
            string required = field.Required ? " aria-required=\"true\"" : string.Empty;
            string invalid = hasError ? " aria-invalid=\"true\"" : string.Empty;
            string placeholder = string.IsNullOrWhiteSpace(Placeholder) ? string.Empty : $" placeholder=\"{Encode(Placeholder)}\"";
            string clearHidden = string.IsNullOrEmpty(selectedText) ? " hidden" : string.Empty;

            sb.AppendLine("<div class=\"fdcp-autocomplete__control\">");
            sb.AppendLine(CultureInfo.InvariantCulture, $@"<input type=""text""
                   class=""fdcp-autocomplete__input""
                   id=""{Encode(inputId)}""
                   role=""combobox""
                   aria-autocomplete=""list""
                   aria-expanded=""false""
                   aria-controls=""{Encode(listboxId)}""
                   aria-describedby=""{Encode(string.Join(" ", describedBy))}""
                   autocomplete=""off""
                   autocapitalize=""none""
                   spellcheck=""false""
                   value=""{Encode(selectedText)}""
                   data-fdcp-autocomplete-input{placeholder}{required}{invalid}{disabled} />");
            sb.AppendLine(CultureInfo.InvariantCulture, $@"<button type=""button""
                    class=""fdcp-autocomplete__clear""
                    aria-label=""{Encode(ClearText ?? Localization.Autocomplete_Clear)}""
                    data-fdcp-autocomplete-clear{clearHidden}{disabled}><span aria-hidden=""true"">&times;</span></button>");
            sb.AppendLine("<div class=\"fdcp-autocomplete__panel\" hidden data-fdcp-autocomplete-panel>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"<div class=\"fdcp-autocomplete__listbox\" id=\"{Encode(listboxId)}\" role=\"listbox\" aria-labelledby=\"{Encode(labelId)}\" tabindex=\"-1\" data-fdcp-autocomplete-listbox><div class=\"fdcp-autocomplete__sizer\" role=\"presentation\" data-fdcp-autocomplete-sizer></div></div>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"<div class=\"fdcp-autocomplete__message\" id=\"{Encode(messageId)}\" hidden data-fdcp-autocomplete-message></div>");
            sb.AppendLine("</div>");
            sb.AppendLine("</div>");

            sb.AppendLine(CultureInfo.InvariantCulture, $@"<input type=""hidden""
                   name=""{Encode(field.Name)}""
                   value=""{Encode(selectedValue)}""
                   data-fdcp-autocomplete-value{disabled} />");

            sb.AppendLine(CultureInfo.InvariantCulture, $"<span class=\"fdcp-autocomplete__visually-hidden\" id=\"{Encode(instructionsId)}\">{Encode(InstructionsText ?? Localization.Autocomplete_Instructions)}</span>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"<div class=\"fdcp-autocomplete__visually-hidden\" id=\"{Encode(statusId)}\" aria-live=\"polite\" aria-atomic=\"true\" data-fdcp-autocomplete-status></div>");

            output.Content.SetHtmlContent(sb.ToString());
            return Task.CompletedTask;
        }

        /// <summary>
        /// Accepts relative URLs and absolute http/https URLs only, so a caller-supplied value can never be a script or data URL.
        /// </summary>
        private static bool IsSafeUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            string trimmed = url.Trim();
            int colon = trimmed.IndexOf(':', StringComparison.Ordinal);
            int delimiter = trimmed.IndexOfAny(['/', '?', '#']);

            if (colon < 0 || (delimiter >= 0 && delimiter < colon))
            {
                return true;
            }

            string scheme = trimmed[..colon];
            return scheme.Equals("http", StringComparison.OrdinalIgnoreCase)
                || scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
        }

        private static string Invariant(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string SanitizeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "fdcp_autocomplete";
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
