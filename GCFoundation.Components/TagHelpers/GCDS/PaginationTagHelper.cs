using System.Diagnostics.CodeAnalysis;
using GCFoundation.Components.Enums;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// A TagHelper that renders a GC Design System compliant pagination component.
    /// </summary>
    [HtmlTargetElement("gcds-pagination")]
    public class PaginationTagHelper : BaseTagHelper
    {
        /// <summary>
        /// Gets or sets the accessible label for the pagination component.
        /// </summary>
        public required string Label { get; set; }

        /// <summary>
        /// Gets or sets the current active page number (1-based index).
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Gets or sets the display style of the pagination (e.g., list or compact).
        /// </summary>
        public PaginationDisplay Display { get; set; } = PaginationDisplay.List;

        /// <summary>
        /// Gets or sets the hyperlink reference for the "Next" button.
        /// </summary>
        public string NextHref { get; set; } = "#next";

        /// <summary>
        /// Gets or sets the label for the "Next" button.
        /// </summary>
        public string? NextLabel { get; set; }

        /// <summary>
        /// Gets or sets the hyperlink reference for the "Previous" button.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Uri)]
        public string PreviousHref { get; set; } = "#previous";

        /// <summary>
        /// Gets or sets the label for the "Previous" button.
        /// </summary>
        public string? PreviousLabel { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages available.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or sets the optional base URL for constructing pagination links.
        /// </summary>
        public Uri? Url { get; set; }

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            AddAttributeIfNotNull(output, "label", Label);
            AddAttributeIfNotNull(output, "current-page", CurrentPage);
            AddAttributeIfNotNull(output, "display", Display);
            AddAttributeIfNotNull(output, "lang", Lang);
            AddAttributeIfNotNull(output, "next-href", NextHref);
            AddAttributeIfNotNull(output, "next-label", NextLabel);
            AddAttributeIfNotNull(output, "previous-href", PreviousHref);
            AddAttributeIfNotNull(output, "previous-label", PreviousLabel);
            AddAttributeIfNotNull(output, "total-pages", TotalPages);
            AddAttributeIfNotNull(output, "url", Url);

            base.Process(context, output);
        }
    }
}
