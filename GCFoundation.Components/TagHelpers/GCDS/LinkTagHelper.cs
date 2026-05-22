using System.Diagnostics.CodeAnalysis;
using GCFoundation.Components.Enums;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// Represents a custom HTML link element (anchor) with additional attributes for controlling display, external links, and size.
    /// </summary>
    [HtmlTargetElement("gcds-link")]
    public class LinkTagHelper : BaseTagHelper
    {
        /// <summary>
        /// Gets or sets the display property for the link (e.g., inline, block).
        /// </summary>
        public string? Display { get; set; }

        /// <summary>
        /// Gets or sets the download attribute for the link, specifying the name of the file to download.
        /// </summary>
        public string? Download { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the link is external (i.e., opens in a new tab).
        /// </summary>
        public bool External { get; set; }

        /// <summary>
        /// Gets or sets the href attribute, which is the destination URL of the link.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Uri)]
        public required string Href { get; set; } = "";

        /// <summary>
        /// Gets or sets the rel attribute, which specifies the relationship between the current document and the linked resource.
        /// </summary>
        public string? Rel { get; set; }

        /// <summary>
        /// Gets or sets the size variant for the link (e.g., inherit, large).
        /// </summary>
        public LinkSize Size { get; set; } = LinkSize.Inherit;

        /// <summary>
        /// Target of the link (_blank, _self, _parent, framename)
        /// </summary>
        public required string Target { get; set; } = "_self";

        /// <summary>
        /// Gets or sets the type attribute, which specifies the MIME type of the linked resource.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the link role for styling (GCDS v1 <c>link-role</c>: default or light).
        /// </summary>
        public LinkVariant Variant { get; set; } = LinkVariant.Default;

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            AddAttributeIfNotNull(output, "display", Display);
            AddAttributeIfNotNull(output, "download", Download);
            AddAttributeIfNotNull(output, "external", External);
            AddAttributeIfNotNull(output, "href", Href);
            AddAttributeIfNotNull(output, "rel", Rel);
            AddAttributeIfNotNull(output, "size", Size);
            AddAttributeIfNotNull(output, "target", Target);
            AddAttributeIfNotNull(output, "type", Type);
            AddAttributeIfNotNull(output, "link-role", Variant);

            base.Process(context, output);
        }


    }
}
