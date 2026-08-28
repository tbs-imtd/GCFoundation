using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// A tag helper for rendering individual breadcrumb items in the breadcrumbs navigation.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;gcds-breadcrumbs&gt;
    ///     &lt;gcds-breadcrumbs-item href=&quot;/home&quot;&gt;Home&lt;/gcds-breadcrumbs-item&gt;
    /// &lt;/gcds-breadcrumbs&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("gcds-breadcrumbs-item")]
    public class BreadcrumbsItemTagHelper : BaseTagHelper
    {
        /// <summary>
        /// The href (link) for the breadcrumb item.
        /// </summary>
        public string? Href { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            AddAttributeIfNotNull(output, "href", Href);

            base.Process(context, output);
        }
    }
}
