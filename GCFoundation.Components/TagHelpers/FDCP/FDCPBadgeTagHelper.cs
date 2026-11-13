using GCFoundation.Components.Enums;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// A tag helper that renders a customizable badge component using the GC Design System.
    /// Supports styles, dismissible button, inversion, and start/end content via props or slot elements.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fdcp-badge style=&quot;info&quot;&gt;
    ///     New
    /// &lt;/fdcp-badge&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("fdcp-badge")]
    public class FDCPBadgeTagHelper : TagHelper
    {

        /// <summary>
        /// Gets or sets the HTML ID attribute for the badge.
        /// </summary>
        public string? TagId { get; set; }

        /// <summary>
        /// Gets or sets the visual style of the badge (e.g., Success, Danger, Info, etc.).
        /// </summary>
        public BadgeStyle Style { get; set; }

        /// <summary>
        /// Gets or sets whether the badge should render with inverted colors.
        /// </summary>
        public bool Inverted { get; set; }

        /// <summary>
        /// Gets or sets optional HTML content to render before the main badge content.
        /// Used only if no <c>slot="start-content"</c> is provided.
        /// </summary>
        public string? StartContent { get; set; }

        /// <summary>
        /// Gets or sets optional HTML content to render after the main badge content.
        /// Used only if no <c>slot="end-content"</c> is provided.
        /// </summary>
        public string? EndContent { get; set; }

        /// <inheritdoc />
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            output.TagName = "span";
            output.TagMode = TagMode.StartTagAndEndTag;

            var classes = new StringBuilder("fdcp-badge");
#pragma warning disable CA1308 // Normalize strings to uppercase
            classes.Append(' ').Append("fdcp-badge-").Append(Style);
#pragma warning restore CA1308 // Normalize strings to uppercase


            if (Inverted)
                classes.Append(" inverted");

            output.Attributes.SetAttribute("class", classes.ToString());

            if (!string.IsNullOrEmpty(TagId))
                output.Attributes.SetAttribute("id", TagId);

            // Get child content to parse slot elements
            var childContentRaw = (await output.GetChildContentAsync().ConfigureAwait(true)).GetContent();

            var slotStart = ExtractSlotContent(childContentRaw, "start-content");
            var slotEnd = ExtractSlotContent(childContentRaw, "end-content");
            var cleanedContent = RemoveSlotElements(childContentRaw);

            var contentBuilder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(slotStart) || !string.IsNullOrWhiteSpace(StartContent))
            {
                contentBuilder
                    .Append("<span class='fdcp-badge-start'>")
                    .Append(!string.IsNullOrWhiteSpace(slotStart) ? slotStart : StartContent)
                    .Append("</span>");
            }

            contentBuilder
                .Append("<span class='fdcp-badge-content'>")
                .Append(cleanedContent.Trim())
                .Append("</span>");

            if (!string.IsNullOrWhiteSpace(slotEnd) || !string.IsNullOrWhiteSpace(EndContent))
            {
                contentBuilder
                    .Append("<span class='fdcp-badge-end'>")
                    .Append(!string.IsNullOrWhiteSpace(slotEnd) ? slotEnd : EndContent)
                    .Append("</span>");
            }

            output.Content.SetHtmlContent(contentBuilder.ToString());
        }

        /// <summary>
        /// Extracts the inner HTML content of an element with the specified slot name.
        /// </summary>
        /// <param name="html">The full HTML content.</param>
        /// <param name="slotName">The name of the slot to extract (e.g., "start-content").</param>
        /// <returns>The inner content of the slot element, or an empty string.</returns>
        private static string ExtractSlotContent(string html, string slotName)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var slotNode = doc.DocumentNode
                .Descendants()
                .FirstOrDefault(n => n.Attributes["slot"]?.Value == slotName);

            return slotNode?.InnerHtml.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Removes all slot-marked elements from the HTML string.
        /// </summary>
        /// <param name="html">The HTML string containing slot elements.</param>
        /// <returns>Cleaned HTML without slot elements.</returns>
        private static string RemoveSlotElements(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var nodesToRemove = doc.DocumentNode
                .Descendants()
                .Where(n => n.Attributes["slot"] != null)
                .ToList();

            foreach (var node in nodesToRemove)
            {
                node.ParentNode.RemoveChild(node, keepGrandChildren: false);
            }

            return doc.DocumentNode.InnerHtml.Trim();
        }
    }
}
