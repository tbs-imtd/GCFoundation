using GCFoundation.Components.Enums;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// Renders a searchable select that supports single and multiple selection.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fdcp-searchable-select for=&quot;@Model.SearchableCountry&quot; items=&quot;@(new[] { new SelectListItem { Text = &quot;Canada&quot;, Value = &quot;CA&quot; }, new SelectListItem { Text = &quot;France&quot;, Value = &quot;FR&quot; } })&quot; label=&quot;Country&quot; default-value=&quot;Select a country&quot; search-placeholder=&quot;Search&quot;&gt;
    /// &lt;/fdcp-searchable-select&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("fdcp-searchable-select", Attributes = "for, items")]
    [HtmlTargetElement("fdcp-searchable-select", Attributes = "items, name")]
    public class FDCPSearchableSelectTagHelper : FDCPBaseFormComponentTagHelper
    {
        /// <summary>
        /// The text shown in the trigger when no option is selected.
        /// </summary>
        public string DefaultValue { get; set; } = "Select option";

        /// <summary>
        /// Label text for the select. Used when <c>for</c> is not specified,
        /// or overrides the model display name when <c>for</c> is specified.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// The list of selectable options.
        /// </summary>
        [HtmlAttributeName("items")]
        public IEnumerable<SelectListItem> Items { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// Determines whether the select allows one or many selected options.
        /// </summary>
        public FDCPSearchableSelectSelectionMode SelectionMode { get; set; } = FDCPSearchableSelectSelectionMode.Single;

        /// <summary>
        /// Placeholder text for the search input.
        /// </summary>
        public string SearchPlaceholder { get; set; } = "Search";

        /// <summary>
        /// Accessible label for the search input.
        /// </summary>
        public string SearchLabel { get; set; } = "Search options";

        /// <summary>
        /// Text shown when no options match the search term.
        /// </summary>
        public string NoResultsText { get; set; } = "No results found";

        /// <summary>
        /// Text announced when one option matches the search term.
        /// </summary>
        public string OneResultText { get; set; } = "1 result available";

        /// <summary>
        /// Text announced when multiple options match the search term. Use <c>{0}</c> for the result count.
        /// </summary>
        public string MultipleResultsText { get; set; } = "{0} results available";

        /// <summary>
        /// Text shown after the selected count in multiple selection mode.
        /// </summary>
        public string MultipleSelectedText { get; set; } = "selected";

        /// <inheritdoc/>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            FormFieldContext field = ResolveFormField(new FormFieldResolveOptions
            {
                Label = Label,
                Hint = Hint,
                Value = Value
            });

            var selectedValues = GetSelectedValues(field);
            var items = Items.ToList();
            foreach (var selectedItem in items.Where(item => item.Selected))
            {
                selectedValues.Add(selectedItem.Value);
            }

            var selectedLabels = items
                .Where(item => selectedValues.Contains(item.Value))
                .Select(item => item.Text)
                .ToList();

            string componentId = SanitizeId(field.Id);
            string labelId = $"{componentId}_label";
            string triggerId = $"{componentId}_trigger";
            string panelId = $"{componentId}_panel";
            string searchId = $"{componentId}_search";
            string searchLabelId = $"{componentId}_search_label";
            string optionsId = $"{componentId}_options";
            string statusId = $"{componentId}_status";
            string hintId = $"{componentId}_hint";
            string errorId = $"{componentId}_error";
            string footerSlot = await GetSlotContentAsync(output).ConfigureAwait(true);
            string mode = SelectionMode.ToString().ToLowerInvariant();
            string? errorMessage = ResolveModelStateError(field.Name);
            string requiredErrorMessage = GCFoundation.Components.Resources.Validation.Field_Required_Generic;
            string requiredSummaryMessage = string.Format(
                CultureInfo.CurrentCulture,
                GCFoundation.Components.Resources.Validation.Field_Required_Summary ?? string.Empty,
                field.Label);
            string selectedSummary = selectedLabels.Count == 0
                ? DefaultValue
                : SelectionMode == FDCPSearchableSelectSelectionMode.Multiple
                    ? $"{selectedLabels.Count} {MultipleSelectedText}"
                    : string.Join(", ", selectedLabels);

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", $"fdcp-searchable-select fdcp-searchable-select--{mode} gcds-select-wrapper");
            output.Attributes.SetAttribute("id", componentId);
            output.Attributes.SetAttribute("data-fdcp-searchable-select", string.Empty);
            output.Attributes.SetAttribute("data-selection-mode", mode);
            output.Attributes.SetAttribute("data-default-value", DefaultValue);
            output.Attributes.SetAttribute("data-multiple-selected-text", MultipleSelectedText);
            output.Attributes.SetAttribute("data-one-result-text", OneResultText);
            output.Attributes.SetAttribute("data-multiple-results-text", MultipleResultsText);
            output.Attributes.SetAttribute("data-required", field.Required.ToString().ToLowerInvariant());
            output.Attributes.SetAttribute("data-required-message", requiredErrorMessage);
            output.Attributes.SetAttribute("data-required-summary-message", requiredSummaryMessage);

            var sb = new StringBuilder();
            string labelContent = Encode(field.Label);
            if (field.Required)
            {
                labelContent += $" <span class=\"label--required\" aria-hidden=\"true\">({Encode(GCFoundation.Components.Resources.Localization.Required)})</span>";
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"<label class=\"fdcp-searchable-select__label gcds-label\" id=\"{EncodeAttribute(labelId)}\" for=\"{EncodeAttribute(triggerId)}\">{labelContent}</label>");

            if (!string.IsNullOrWhiteSpace(field.Hint))
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"<gcds-hint hint-id=\"{EncodeAttribute(hintId)}\" id=\"{EncodeAttribute(hintId)}\">{Encode(field.Hint)}</gcds-hint>");
            }

            if (field.Required || !string.IsNullOrWhiteSpace(errorMessage))
            {
                string resolvedErrorMessage = errorMessage ?? requiredErrorMessage;
                string hidden = string.IsNullOrWhiteSpace(errorMessage) ? " hidden" : string.Empty;
                sb.AppendLine(CultureInfo.InvariantCulture, $"<gcds-error-message id=\"{EncodeAttribute(errorId)}\" message-id=\"{EncodeAttribute(errorId)}\" data-fdcp-searchable-select-error{hidden}>{Encode(resolvedErrorMessage)}</gcds-error-message>");
            }

            string describedBy = BuildDescribedBy(field.Hint, hintId, errorMessage ?? (field.Required ? requiredErrorMessage : null), errorId);
            string ariaDescribedBy = string.IsNullOrWhiteSpace(describedBy) ? string.Empty : $" aria-describedby=\"{EncodeAttribute(describedBy)}\"";
            string disabled = field.Disabled ? " disabled" : string.Empty;
            string required = field.Required ? " aria-required=\"true\"" : string.Empty;
            string invalid = string.IsNullOrWhiteSpace(errorMessage) ? string.Empty : @" aria-invalid=""true""";
            string ariaHasPopup = SelectionMode == FDCPSearchableSelectSelectionMode.Single ? @" aria-haspopup=""listbox""" : string.Empty;
            string searchComboboxAttributes = SelectionMode == FDCPSearchableSelectSelectionMode.Single
                ? $@" role=""combobox"" aria-autocomplete=""list"" aria-expanded=""false"" aria-controls=""{EncodeAttribute(optionsId)}"" aria-labelledby=""{EncodeAttribute(labelId)} {EncodeAttribute(searchLabelId)}"" aria-describedby=""{EncodeAttribute(statusId)}""{required}"
                : string.Empty;

            sb.AppendLine(CultureInfo.InvariantCulture, $@"<button type=""button""
                class=""fdcp-searchable-select__trigger""
                id=""{EncodeAttribute(triggerId)}""
                aria-expanded=""false""
                aria-controls=""{EncodeAttribute(panelId)}""
                data-fdcp-searchable-select-trigger{ariaHasPopup}{ariaDescribedBy}{required}{invalid}{disabled}>
                <span class=""fdcp-searchable-select__trigger-text"" data-fdcp-searchable-select-selected-text>{Encode(selectedSummary)}</span>
                <span class=""fdcp-searchable-select__trigger-icon"" aria-hidden=""true""></span>
            </button>");
            AppendSizer(sb, items);

            if (SelectionMode == FDCPSearchableSelectSelectionMode.Single)
            {
                string selectedValue = selectedValues.FirstOrDefault() ?? string.Empty;
                sb.AppendLine(CultureInfo.InvariantCulture, $@"<input type=""hidden""
                   name=""{EncodeAttribute(field.Name)}""
                   value=""{EncodeAttribute(selectedValue)}""
                   data-fdcp-searchable-select-single-input />");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"<div class=\"fdcp-searchable-select__panel\" id=\"{EncodeAttribute(panelId)}\" hidden data-fdcp-searchable-select-panel>");
            sb.AppendLine(CultureInfo.InvariantCulture, $@"<div class=""fdcp-searchable-select__search-wrapper"">
            <label class=""visually-hidden"" id=""{EncodeAttribute(searchLabelId)}"" for=""{EncodeAttribute(searchId)}"">{Encode(SearchLabel)}</label>
            <input type=""search""
                   class=""fdcp-searchable-select__search""
                   id=""{EncodeAttribute(searchId)}""
                   placeholder=""{EncodeAttribute(SearchPlaceholder)}""
                   {searchComboboxAttributes}
                   data-fdcp-searchable-select-search />
            <span class=""fdcp-searchable-select__search-icon fa-solid fa-magnifying-glass"" aria-hidden=""true""></span>
            </div>");
            string optionsRole = SelectionMode == FDCPSearchableSelectSelectionMode.Single ? "listbox" : "group";
            sb.AppendLine(CultureInfo.InvariantCulture, $"<div class=\"fdcp-searchable-select__options\" id=\"{EncodeAttribute(optionsId)}\" role=\"{optionsRole}\" aria-labelledby=\"{EncodeAttribute(labelId)}\">");
            AppendOptions(sb, items, selectedValues, field, componentId);
            sb.AppendLine("</div>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"<div class=\"fdcp-searchable-select__no-results\" hidden data-fdcp-searchable-select-no-results>{Encode(NoResultsText)}</div>");
            sb.AppendLine(CultureInfo.InvariantCulture, $"<div class=\"visually-hidden\" id=\"{EncodeAttribute(statusId)}\" aria-live=\"polite\" aria-atomic=\"true\" data-fdcp-searchable-select-status></div>");

            if (!string.IsNullOrWhiteSpace(footerSlot))
            {
                sb.AppendLine("<div class=\"fdcp-searchable-select__footer\" aria-live=\"polite\" aria-atomic=\"true\">");
                sb.AppendLine(footerSlot);
                sb.AppendLine("</div>");
            }

            sb.AppendLine("</div>");
            output.Content.SetHtmlContent(sb.ToString());
        }

        private static void AppendSizer(StringBuilder sb, IEnumerable<SelectListItem> items)
        {
            sb.AppendLine("<div class=\"fdcp-searchable-select__sizer\" aria-hidden=\"true\">");

            foreach (var item in items)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"<span>{Encode(item.Text)}</span>");
            }

            sb.AppendLine("</div>");
        }

        private void AppendOptions(StringBuilder sb, List<SelectListItem> items, HashSet<string> selectedValues, FormFieldContext field, string componentId)
        {
            int optionIndex = 0;
            int groupIndex = 0;

            foreach (var group in GroupItems(items))
            {
                if (!string.IsNullOrWhiteSpace(group.Name))
                {
                    string groupLabelId = SanitizeId($"{componentId}_group_{groupIndex}");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"<div class=\"fdcp-searchable-select__group\" role=\"group\" aria-labelledby=\"{EncodeAttribute(groupLabelId)}\" data-fdcp-searchable-select-group>");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"<div class=\"fdcp-searchable-select__group-label\" id=\"{EncodeAttribute(groupLabelId)}\">{Encode(group.Name)}</div>");
                }

                foreach (var item in group.Items)
                {
                    string optionId = SanitizeId($"{componentId}_{optionIndex}_{item.Value}");
                    bool selected = selectedValues.Contains(item.Value);
                    string isChecked = selected ? " checked" : string.Empty;
                    string isSelected = selected ? " is-selected" : string.Empty;
                    string ariaSelected = selected.ToString().ToLowerInvariant();
                    string disabled = field.Disabled || item.Disabled ? " disabled" : string.Empty;
                    string ariaDisabled = (field.Disabled || item.Disabled).ToString().ToLowerInvariant();

                    if (SelectionMode == FDCPSearchableSelectSelectionMode.Single)
                    {
                        sb.AppendLine(CultureInfo.InvariantCulture, $@"<div
                        class=""fdcp-searchable-select__option fdcp-searchable-select__option-item{isSelected}""
                        id=""{EncodeAttribute(optionId)}""
                        role=""option""
                        aria-selected=""{ariaSelected}""
                        aria-disabled=""{ariaDisabled}""
                        tabindex=""-1""
                        data-fdcp-searchable-select-option
                        data-option-text=""{EncodeAttribute(item.Text)}""
                        data-option-value=""{EncodeAttribute(item.Value)}""
                        data-option-label=""{EncodeAttribute(item.Text)}"">{Encode(item.Text)}</div>");
                    }
                    else
                    {
                        sb.AppendLine(CultureInfo.InvariantCulture, $@"<div class=""fdcp-searchable-select__option"" data-fdcp-searchable-select-option data-option-text=""{EncodeAttribute(item.Text)}"">
                            <input type=""checkbox""
                                   class=""fdcp-searchable-select__input""
                                   name=""{EncodeAttribute(field.Name)}""
                                   id=""{EncodeAttribute(optionId)}""
                                   value=""{EncodeAttribute(item.Value)}""
                                   data-option-label=""{EncodeAttribute(item.Text)}""{isChecked}{disabled} />
                            <label class=""fdcp-searchable-select__option-label"" for=""{EncodeAttribute(optionId)}"">{Encode(item.Text)}</label>
                        </div>");
                    }

                    optionIndex++;
                }

                if (!string.IsNullOrWhiteSpace(group.Name))
                {
                    sb.AppendLine("</div>");
                }

                groupIndex++;
            }
        }

        private static IEnumerable<OptionGroup> GroupItems(IEnumerable<SelectListItem> items)
        {
            var groups = new List<OptionGroup>();

            foreach (var item in items)
            {
                string groupName = item.Group?.Name ?? string.Empty;
                var group = groups.FirstOrDefault(existing => existing.Name == groupName);
                if (group == null)
                {
                    group = new OptionGroup(groupName);
                    groups.Add(group);
                }

                group.Items.Add(item);
            }

            return groups;
        }

        private static HashSet<string> GetSelectedValues(FormFieldContext field)
        {
            if (field.Model is IEnumerable<string> values)
            {
                return values.ToHashSet(StringComparer.Ordinal);
            }

            if (!string.IsNullOrWhiteSpace(field.Value))
            {
                return field.Value
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToHashSet(StringComparer.Ordinal);
            }

            return new HashSet<string>(StringComparer.Ordinal);
        }

        private static async Task<string> GetSlotContentAsync(TagHelperOutput output)
        {
            var childContent = await output.GetChildContentAsync().ConfigureAwait(true);
            string html = childContent.GetContent();

            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            return ExtractSlotContent(html, "selected-options") ?? ExtractSlotContent(html, "footer") ?? string.Empty;
        }

        private static string? ExtractSlotContent(string html, string slotName)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var slotNode = doc.DocumentNode
                .Descendants()
                .FirstOrDefault(node => node.Attributes["slot"]?.Value == slotName);

            return slotNode?.InnerHtml.Trim();
        }

        private static string BuildDescribedBy(string hint, string hintId, string? errorMessage, string errorId)
        {
            var ids = new List<string>();

            if (!string.IsNullOrWhiteSpace(hint))
            {
                ids.Add(hintId);
            }

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                ids.Add(errorId);
            }

            return string.Join(" ", ids);
        }

        private static string SanitizeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "fdcp_searchable_select";
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

        private static string EncodeAttribute(string? value)
        {
            return HtmlEncoder.Default.Encode(value ?? string.Empty);
        }

        private sealed class OptionGroup(string name)
        {
            public string Name { get; } = name;

            public List<SelectListItem> Items { get; } = new();
        }
    }

}
