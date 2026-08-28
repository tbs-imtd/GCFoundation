using GCFoundation.Components.Enums;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// A tag helper for rendering a container component with optional styling and layout properties.
    /// This component can be customized with attributes like border, centering, margin, padding, and size.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;gcds-container size=&quot;lg&quot; tag=&quot;div&quot;&gt;
    ///     Page content
    /// &lt;/gcds-container&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("gcds-container")]
    public class ContainerTagHelper : BaseTagHelper
    {
        /// <summary>
        /// If set to true, adds a border around the container.
        /// </summary>
        public bool Border { get; set; }

        /// <summary>
        /// If set to true, centers the content (GCDS v1: emitted as <c>alignment="center"</c>).
        /// </summary>
        public bool Centered { get; set; }

        /// <summary>
        /// If set to true, marks the container as the main page layout (GCDS v1: emitted as <c>layout="page"</c>).
        /// </summary>
        public bool MainContainer { get; set; }

        /// <summary>
        /// Defines the margin of the container (can be a CSS unit like "px", "em", etc.).
        /// </summary>
        public string? Margin { get; set; }

        /// <summary>
        /// Defines the padding of the container. Default value is "300".
        /// </summary>
        public string? Padding { get; set; }

        /// <summary>
        /// Defines the size of the container. Default is <see cref="SizeTypeEmum.lg"/>. Use with <see cref="MainContainer"/> / <c>layout="page"</c> per current GCDS guidance.
        /// </summary>
        public SizeTypeEmum Size { get; set; } = SizeTypeEmum.lg;

        /// <summary>
        /// Specifies the tag name to be used for the container (e.g., div, section, etc.).
        /// </summary>
        public string? Tag { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            AddAttributeIfNotNull(output, "border", Border);
            if (Centered)
                AddAttributeIfNotNull(output, "alignment", "center");

            if (MainContainer)
                AddAttributeIfNotNull(output, "layout", "page");

            AddAttributeIfNotNull(output, "margin", Margin);
            AddAttributeIfNotNull(output, "padding", Padding);
            AddAttributeIfNotNull(output, "size", Size);
            AddAttributeIfNotNull(output, "tag", Tag);
            base.Process(context, output);
        }

    }
}
