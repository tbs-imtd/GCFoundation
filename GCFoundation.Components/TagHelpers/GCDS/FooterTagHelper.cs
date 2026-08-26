using GCFoundation.Common.Models;
using GCFoundation.Common.Utilities;
using GCFoundation.Components.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Json;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// Represents a custom tag helper for rendering a footer section with contextual and sub-links.
    /// This tag helper generates a footer with support for customizable headings, links, and display options.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;gcds-footer display=&quot;full&quot; contextual-heading=&quot;Related links&quot;&gt;&lt;/gcds-footer&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("gcds-footer")]
    public class FooterTagHelper(IUrlHelperFactory urlHelperFactory) : BaseTagHelper
    {
        private readonly IUrlHelperFactory UrlHelperFactory = urlHelperFactory;

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// The optional heading text to display in the footer's contextual section.
        /// </summary>
        public string? ContextualHeading { get; set; }

        /// <summary>
        /// Optional contextual footer links (shown in the footer's contextual section).
        /// </summary>
        public IEnumerable<FooterLink>? ContextualLinks { get; set; }

        /// <summary>
        /// The display type of the footer. Determines how the footer is rendered (e.g., full, minimal).
        /// Default is <see cref="FooterDisplayType.full"/>.
        /// </summary>
        public FooterDisplayType Display { get; set; } = FooterDisplayType.full;

        /// <summary>
        /// The collection of sub-links to display in the footer.
        /// </summary>
        public IEnumerable<FooterLink>? SubLinks { get; set; }


        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var urlHelper = UrlHelperFactory.GetUrlHelper(ViewContext);

            AddAttributeIfNotNull(output, "contextual-heading", ContextualHeading);

            if (SerializeFooterLinksDictionary(ContextualLinks, urlHelper) is { } contextualLinksJson)
                output.Attributes.SetAttribute("contextual-links", contextualLinksJson);

            AddAttributeIfNotNull(output, "display", Display);
            AddAttributeIfNotNull(output, "lang", Lang);

            if (SerializeFooterLinksDictionary(SubLinks, urlHelper) is { } subLinksJson)
                output.Attributes.SetAttribute("sub-links", subLinksJson);

            base.Process(context, output);
        }

        /// <summary>
        /// Builds a JSON object keyed by localized label for the GCDS footer attribute, or <c>null</c> when there is nothing to emit.
        /// </summary>
        private static string? SerializeFooterLinksDictionary(IEnumerable<FooterLink>? links, IUrlHelper urlHelper)
        {
            if (links is null)
                return null;

            var pairs = links
                .Select(link => (Label: link.GetLocalizedLabel(), Link: urlHelper.Content(link.GetLocalizedLink())))
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Label) && !string.IsNullOrWhiteSpace(pair.Link))
                .ToList();

            if (pairs.Count == 0)
                return null;

            return JsonSerializer.Serialize(
                pairs.ToDictionary(pair => pair.Label!, pair => pair.Link!),
                JsonOptionsUtility.CamelCase);
        }
    }
}
