using GCFoundation.Components.Enums;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;
using System.Text;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// TagHelper for rendering a GC-style page header with title, description, and optional background image.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fdcp-page-heading title=&quot;Funding Opportunities&quot; description=&quot;Discover and apply for funding opportunities to support your projects and initiatives.&quot; src=&quot;/images/founding-opportunity-heading-bg.jpg&quot;&gt;
    /// &lt;/fdcp-page-heading&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("fdcp-page-heading")]
    public class FDCPPageHeadingTagHelper : TagHelper
    {
        /// <summary>
        /// The main heading text to display in the page header.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Sets the colour of the background of the text container to emphasize the content.
        /// </summary>
        public BackgroundColour BackgroundColour { get; set; } = BackgroundColour.primary;

        /// <summary>
        /// The description text displayed below the title.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Sets the size of the page header. Default, or Large.
        /// </summary>
        public PageHeadingSize Size { get; set; } = PageHeadingSize.regular;

        /// <summary>
        /// The URL of the background image for the page header.
        /// </summary>
        public string? Src { get; set; }

        /// <summary>
        /// Sets the colour of the text content.
        /// </summary>
        public TextColour TextColour { get; set; } = TextColour.light;

        /// <summary>
        /// Adds a light background and a border around the text container to emphasize the content.
        /// </summary>
        public bool TextEmphasis { get; set; }

        /// <summary>
        /// Processes the tag helper and renders the page header markup.
        /// </summary>
        /// <param name="context">The context for the tag helper.</param>
        /// <param name="output">The output for the tag helper.</param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            var articleClass = "text-container";
            var containerClass = "fdcp-page-heading-container";
            var pageHeadingBgClass = "fdcp-page-heading-bg";

            if (!string.IsNullOrWhiteSpace(Src))
            {
                containerClass += " fdcp-page-heading-has-bg";
                output.Attributes.SetAttribute("data-bg-src", Src);
            }

            switch (Size)
            {
                case PageHeadingSize.compact:
                    articleClass += " sm:py-350 py-200 xl:ps-0 sm:ps-600 ps-450 sm:pe-750 pe-450";
                    containerClass += " fdcp-page-heading-compact";
                    pageHeadingBgClass += " md:py-500 py-250";
                    break;
                case PageHeadingSize.large:
                    articleClass += " sm:py-750 py-450 xl:ps-0 sm:ps-600 ps-450 sm:pe-750 pe-450";
                    containerClass += " fdcp-page-heading-large";
                    pageHeadingBgClass += " md:py-1250 py-900";
                    break;
                case PageHeadingSize.regular:
                default:
                    articleClass += " sm:py-600 py-300 xl:ps-0 sm:ps-600 ps-450 sm:pe-750 pe-450";
                    pageHeadingBgClass += " md:py-900 py-600";
                    break;
            }

            output.Attributes.SetAttribute("class", containerClass);

            var content = new StringBuilder();

            content.AppendLine(CultureInfo.InvariantCulture, $"<div class='{pageHeadingBgClass}'>");
            content.AppendLine(CultureInfo.InvariantCulture, $"<div class='container-xl mx-auto'>");

            if (TextEmphasis)
            {
                // Using GCDS CSS Shortcuts for background colours
                switch (BackgroundColour)
                {
                    case BackgroundColour.dark:
                        articleClass += " bg-dark";
                        break;
                    case BackgroundColour.light:
                        articleClass += " bg-light";
                        break;
                    case BackgroundColour.white:
                        articleClass += " bg-white";
                        break;
                    case BackgroundColour.primary:
                    default:
                        articleClass += " bg-primary";
                        break;
                }
            }
            switch (TextColour)
            {
                case TextColour.primary:
                    articleClass += " text-primary";
                    break;
                case TextColour.secondary:
                    articleClass += " text-secondary";
                    break;
                case TextColour.light:
                default:
                    articleClass += " text-light";
                    break;
            }
            content.AppendLine(CultureInfo.InvariantCulture, $"<article class='{articleClass}'>");
            content.AppendLine(CultureInfo.InvariantCulture, $"<gcds-heading tag='h1' heading-role='{TextColour}'>{Title}</gcds-heading>");

            if (!string.IsNullOrWhiteSpace(Description))
            {
                var descriptionTextRole = string.Empty;
                switch (TextColour)
                {
                    case TextColour.primary:
                        descriptionTextRole = " text-role='primary'";
                        break;
                    case TextColour.secondary:
                        descriptionTextRole = " text-role='secondary'";
                        break;
                    case TextColour.light:
                    default:
                        descriptionTextRole = " text-role='light'";
                        break;
                }
                content.AppendLine(CultureInfo.InvariantCulture, $"<gcds-text{descriptionTextRole}>{Description}</gcds-text>");
            }

            content.AppendLine("</article>");
            content.AppendLine("</div>");
            content.AppendLine("</div>");

            output.Content.SetHtmlContent(content.ToString());
        }
    }
}
