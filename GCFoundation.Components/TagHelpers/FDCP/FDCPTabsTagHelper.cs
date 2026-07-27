using HtmlAgilityPack;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// Renders a tab navigation component for switching between in-page content panels.
    /// </summary>
    [HtmlTargetElement("fdcp-tabs")]
    public class FDCPTabsTagHelper : TagHelper
    {
        /// <summary>
        /// Gets or sets a unique ID for the tabs component.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the accessible label for the tab list.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Gets or sets the zero-based selected tab index. When omitted, the first active child tab is selected.
        /// </summary>
        public int? SelectedIndex { get; set; }

        /// <inheritdoc/>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

#pragma warning disable CA2007 // Razor context: ConfigureAwait(false) is not safe here.
            var childContent = await output.GetChildContentAsync();
#pragma warning restore CA2007

            var tabs = ExtractTabs(childContent.GetContent());
            var componentId = SanitizeId(Id) ?? $"fdcp-tabs-{SanitizeId(context?.UniqueId) ?? "component"}";
            var selectedIndex = ResolveSelectedIndex(tabs);
            var renderedTabs = CreateRenderedTabs(componentId, tabs);
            var existingClass = output.Attributes["class"]?.Value?.ToString();
            var wrapperClass = string.IsNullOrWhiteSpace(existingClass) ? "fdcp-tabs" : $"fdcp-tabs {existingClass}";
            var effectiveLabel = string.IsNullOrWhiteSpace(Label)
                ? GCFoundation.Components.Resources.Localization.Tabs
                : Label;

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("id", componentId);
            output.Attributes.SetAttribute("class", wrapperClass);
            output.Attributes.SetAttribute("data-fdcp-tabs", "true");
            output.Attributes.SetAttribute("data-loading-text", GCFoundation.Components.Resources.Localization.Tabs_Loading);
            output.Attributes.SetAttribute("data-load-error-text", GCFoundation.Components.Resources.Localization.Tabs_LoadError);

            var html = new StringBuilder();
            html.AppendLine(
                CultureInfo.InvariantCulture,
                $"<div class='fdcp-tabs__tablist' role='tablist' aria-label='{HtmlEncoder.Default.Encode(effectiveLabel)}'>");

            for (var index = 0; index < renderedTabs.Count; index++)
            {
                var renderedTab = renderedTabs[index];
                var tab = renderedTab.Definition;
                var isSelected = index == selectedIndex;
                var tabIndex = isSelected ? "0" : "-1";

                var ariaSelected = isSelected ? "true" : "false";
                var loadUrlAttribute = string.IsNullOrWhiteSpace(tab.LoadUrl)
                    ? string.Empty
                    : $" data-load-url='{HtmlEncoder.Default.Encode(tab.LoadUrl)}'";
                html.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"  <button type='button' class='fdcp-tabs__tab' id='{renderedTab.TabId}' role='tab' aria-selected='{ariaSelected}' aria-controls='{renderedTab.PanelId}' tabindex='{tabIndex}'{loadUrlAttribute}>{HtmlEncoder.Default.Encode(tab.Title)}</button>");
            }

            html.AppendLine("</div>");

            for (var index = 0; index < renderedTabs.Count; index++)
            {
                var renderedTab = renderedTabs[index];
                var tab = renderedTab.Definition;
                var isSelected = index == selectedIndex;
                var hiddenAttribute = isSelected ? string.Empty : " hidden";
                var tabIndexAttribute = HasFocusableContent(tab.Content) ? string.Empty : " tabindex='0'";
                var liveAttributes = string.IsNullOrWhiteSpace(tab.LoadUrl)
                    ? string.Empty
                    : " aria-live='polite' aria-atomic='true'";

                html.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"<div class='fdcp-tabs__panel' id='{renderedTab.PanelId}' role='tabpanel' aria-labelledby='{renderedTab.TabId}'{tabIndexAttribute}{liveAttributes}{hiddenAttribute}>");
                html.AppendLine(tab.Content);
                html.AppendLine("</div>");
            }

            output.Content.SetHtmlContent(html.ToString());
        }

        private int ResolveSelectedIndex(IReadOnlyList<TabDefinition> tabs)
        {
            if (tabs.Count == 0)
            {
                return -1;
            }

            if (SelectedIndex is int selectedIndex && selectedIndex >= 0 && selectedIndex < tabs.Count)
            {
                return selectedIndex;
            }

            for (var index = 0; index < tabs.Count; index++)
            {
                if (tabs[index].Active)
                {
                    return index;
                }
            }

            return 0;
        }

        private static List<RenderedTab> CreateRenderedTabs(string componentId, IReadOnlyList<TabDefinition> tabs)
        {
            var usedIds = new HashSet<string>(StringComparer.Ordinal);
            var renderedTabs = new List<RenderedTab>(tabs.Count);

            for (var index = 0; index < tabs.Count; index++)
            {
                var tab = tabs[index];
                var childId = SanitizeId(tab.Id) ?? $"tab-{index + 1}";
                var baseId = $"{componentId}-{childId}";
                var uniqueBaseId = baseId;
                var duplicateIndex = 2;

                while (!usedIds.Add(uniqueBaseId))
                {
                    uniqueBaseId = $"{baseId}-{duplicateIndex}";
                    duplicateIndex++;
                }

                renderedTabs.Add(new RenderedTab(tab, $"{uniqueBaseId}-tab", $"{uniqueBaseId}-panel"));
            }

            return renderedTabs;
        }

        private static List<TabDefinition> ExtractTabs(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            return doc.DocumentNode
                .ChildNodes
                .Where(IsTabNode)
                .Select(node => new TabDefinition(
                    System.Net.WebUtility.HtmlDecode(node.Attributes["data-title"]?.Value ?? node.Attributes["title"]?.Value ?? string.Empty),
                    node.InnerHtml.Trim(),
                    IsActive(node),
                    node.Attributes["data-id"]?.Value ?? node.Attributes["id"]?.Value,
                    node.Attributes["data-load-url"]?.Value ?? node.Attributes["load-url"]?.Value))
                .Where(tab => !string.IsNullOrWhiteSpace(tab.Title))
                .ToList();
        }

        private static bool HasFocusableContent(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            return doc.DocumentNode.SelectSingleNode(
                ".//a[@href] | .//button[not(@disabled)] | .//input[not(@disabled) and not(@type='hidden')] | " +
                ".//select[not(@disabled)] | .//textarea[not(@disabled)] | .//summary | " +
                ".//*[@tabindex and @tabindex != '-1']") != null;
        }

        private static bool IsActive(HtmlNode node)
        {
            var activeAttribute = node.Attributes["active"];
            var activeValue = node.Attributes["data-active"]?.Value ?? activeAttribute?.Value;
            return string.Equals(activeValue, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(activeValue, "active", StringComparison.OrdinalIgnoreCase)
                || (activeAttribute != null && string.IsNullOrEmpty(activeValue));
        }

        private static bool IsTabNode(HtmlNode node)
        {
            return string.Equals(node.Name, "fdcp-tab", StringComparison.OrdinalIgnoreCase)
                || string.Equals(node.Attributes["data-fdcp-tab"]?.Value, "true", StringComparison.OrdinalIgnoreCase);
        }

        private static string? SanitizeId(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var builder = new StringBuilder();
            foreach (var character in value.Trim())
            {
                if (char.IsLetterOrDigit(character) || character == '-' || character == '_' || character == ':')
                {
                    builder.Append(character);
                }
                else
                {
                    builder.Append('-');
                }
            }

            return builder.Length == 0 ? null : builder.ToString();
        }

        private sealed record TabDefinition(string Title, string Content, bool Active, string? Id, string? LoadUrl);

        private sealed record RenderedTab(TabDefinition Definition, string TabId, string PanelId);
    }
}
