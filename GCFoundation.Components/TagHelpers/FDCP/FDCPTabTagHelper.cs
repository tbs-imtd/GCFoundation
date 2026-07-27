using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// Represents one tab and its panel content inside an <c>&lt;fdcp-tabs&gt;</c> component.
    /// </summary>
    [HtmlTargetElement("fdcp-tab", ParentTag = "fdcp-tabs")]
    public class FDCPTabTagHelper : TagHelper
    {
        /// <summary>
        /// Gets or sets whether this tab is selected when the page loads.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets an optional ID used as the base for the generated tab and panel IDs.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets an optional URL used to lazy-load the tab panel content when selected.
        /// </summary>
        public Uri? LoadUrl { get; set; }

        /// <summary>
        /// Gets or sets the tab button label.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <inheritdoc/>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("data-fdcp-tab", "true");
            output.Attributes.SetAttribute("data-title", Title);
            output.Attributes.SetAttribute("data-active", Active ? "true" : "false");

            if (!string.IsNullOrWhiteSpace(Id))
            {
                output.Attributes.SetAttribute("data-id", Id);
            }

            if (LoadUrl != null)
            {
                output.Attributes.SetAttribute("data-load-url", LoadUrl.ToString());
            }

#pragma warning disable CA2007 // Razor context: ConfigureAwait(false) is not safe here.
            var childContent = await output.GetChildContentAsync();
#pragma warning restore CA2007

            output.Content.SetHtmlContent(childContent.GetContent());
        }
    }
}
