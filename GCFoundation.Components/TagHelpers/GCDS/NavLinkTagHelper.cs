using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// Represents a navigation link component for use in a webpage's navigation structure.
    /// </summary>
    [HtmlTargetElement("gcds-nav-link")]
    public class NavLinkTagHelper : BaseTagHelper
    {
        /// <summary>
        /// Gets or sets the URL that the navigation link points to.
        /// </summary>
        /// <value>The URL to navigate to when the link is clicked.</value>
        [StringSyntax(StringSyntaxAttribute.Uri)]
        public required string Href { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this navigation link represents the current page.
        /// </summary>
        /// <value>True if the link corresponds to the current page; otherwise, false. The default is null.</value>
        public bool? Current { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            AddAttributeIfNotNull(output, "href", Href);
            AddAttributeIfNotNull(output, "current", Current);
            base.Process(context, output);
        }

    }
}
