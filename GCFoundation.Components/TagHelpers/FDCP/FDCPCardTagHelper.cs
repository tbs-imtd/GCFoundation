using HtmlAgilityPack;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;
using System.Text;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// A tag helper that renders a flexible card component using the GC Design System.
    /// Supports header, body, footer, images, and various Bootstrap-inspired card features.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fdcp-card&gt;
    ///     &lt;h5 class=&quot;fdcp-card-title&quot;&gt;Card title&lt;/h5&gt;
    ///     &lt;p class=&quot;fdcp-card-text&quot;&gt;Some quick example text to build on the card title.&lt;/p&gt;
    ///     &lt;a href=&quot;#&quot; class=&quot;fdcp-card-link&quot;&gt;Go somewhere&lt;/a&gt;
    /// &lt;/fdcp-card&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("fdcp-card")]
    public class FDCPCardTagHelper : TagHelper
    {
        /// <summary>
        /// Gets or sets the HTML ID attribute for the card.
        /// </summary>
        public string? TagId { get; set; }

        /// <summary>
        /// Gets or sets the width of the card using CSS value (e.g., "18rem", "300px", "100%").
        /// </summary>
        public string? Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the card using CSS value (e.g., "200px", "auto", "100%").
        /// </summary>
        public string? Height { get; set; }

        /// <summary>
        /// Gets or sets whether the card should have a border. Default is true.
        /// </summary>
        public bool Border { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the card should have a shadow. Default is false.
        /// </summary>
        public bool Shadow { get; set; }



        /// <summary>
        /// Gets or sets the URL for the card image that appears at the top.
        /// </summary>
        public string? ImageTop { get; set; }

        /// <summary>
        /// Gets or sets the URL for the card image that appears at the bottom.
        /// </summary>
        public string? ImageBottom { get; set; }

        /// <summary>
        /// Gets or sets the alt text for the card images.
        /// </summary>
        public string? ImageAlt { get; set; }

        /// <summary>
        /// Gets or sets whether the card should be horizontal layout.
        /// </summary>
        public bool Horizontal { get; set; }

        /// <inheritdoc />
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            var classes = new StringBuilder("fdcp-card");

            if (!Border)
                classes.Append(" fdcp-card-no-border");

            if (Shadow)
                classes.Append(" fdcp-card-shadow");

            if (Horizontal)
                classes.Append(" fdcp-card-horizontal");

            var existingClass = output.Attributes["class"]?.Value?.ToString();
            if (!string.IsNullOrWhiteSpace(existingClass))
                classes.Append(' ').Append(existingClass);

            output.Attributes.SetAttribute("class", classes.ToString());

            if (!string.IsNullOrEmpty(TagId))
                output.Attributes.SetAttribute("id", TagId);

            if (!string.IsNullOrEmpty(Width))
                output.Attributes.SetAttribute("style", $"width: {Width}");

            if (!string.IsNullOrEmpty(Height))
            {
                var existingStyle = output.Attributes["style"]?.Value?.ToString() ?? "";
                output.Attributes.SetAttribute("style", $"{existingStyle}; height: {Height}".Trim(';', ' '));
            }

            // Get child content to parse slot elements
            var childContentRaw = (await output.GetChildContentAsync().ConfigureAwait(true)).GetContent();

            var headerSlot = ExtractSlotContent(childContentRaw, "header");
            var bodySlot = ExtractSlotContent(childContentRaw, "body");
            var footerSlot = ExtractSlotContent(childContentRaw, "footer");
            var cleanedContent = RemoveSlotElements(childContentRaw);

            var contentBuilder = new StringBuilder();

            // Add top image if specified
            if (!string.IsNullOrEmpty(ImageTop))
            {
                contentBuilder.Append(CultureInfo.InvariantCulture, $"<img src=\"{ImageTop}\" class=\"fdcp-card-img-top\" alt=\"{ImageAlt ?? ""}\">");
            }

            // Add header if present
            if (!string.IsNullOrWhiteSpace(headerSlot))
            {
                contentBuilder
                    .Append("<div class=\"fdcp-card-header\">")
                    .Append(headerSlot)
                    .Append("</div>");
            }

            // Add body content (either from slot or main content)
            if (!string.IsNullOrWhiteSpace(bodySlot) || !string.IsNullOrWhiteSpace(cleanedContent.Trim()))
            {
                contentBuilder
                    .Append("<div class=\"fdcp-card-body\">");

                if (!string.IsNullOrWhiteSpace(bodySlot))
                {
                    contentBuilder.Append(bodySlot);
                }
                else
                {
                    contentBuilder.Append(cleanedContent.Trim());
                }

                contentBuilder.Append("</div>");
            }

            // Add footer if present
            if (!string.IsNullOrWhiteSpace(footerSlot))
            {
                contentBuilder
                    .Append("<div class=\"fdcp-card-footer\">")
                    .Append(footerSlot)
                    .Append("</div>");
            }

            // Add bottom image if specified
            if (!string.IsNullOrEmpty(ImageBottom))
            {
                contentBuilder.Append(CultureInfo.InvariantCulture, $"<img src=\"{ImageBottom}\" class=\"fdcp-card-img-bottom\" alt=\"{ImageAlt ?? ""}\">");
            }

            output.Content.SetHtmlContent(contentBuilder.ToString());
        }

        /// <summary>
        /// Extracts the inner HTML content of an element with the specified slot name.
        /// </summary>
        /// <param name="html">The full HTML content.</param>
        /// <param name="slotName">The name of the slot to extract (e.g., "header", "body", "footer").</param>
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
