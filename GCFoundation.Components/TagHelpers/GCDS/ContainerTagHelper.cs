using GCFoundation.Components.Enums;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// A tag helper for rendering a container component with optional styling and layout properties.
    /// This component can be customized with attributes like border, alignment, layout, margin, padding, and size.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;gcds-container layout=&quot;page&quot; alignment=&quot;center&quot; tag=&quot;main&quot;&gt;
    ///     Page content
    /// &lt;/gcds-container&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("gcds-container")]
    public class ContainerTagHelper : BaseTagHelper
    {
        private HorizontalAlignment? _alignment;
        private ContainerLayout? _layout;

        /// <summary>
        /// Gets or sets the container alignment (GCDS <c>alignment</c>). Not rendered unless set.
        /// Ignored by GCDS when <see cref="Layout"/> is <see cref="ContainerLayout.page"/>.
        /// </summary>
        public HorizontalAlignment Alignment
        {
            get => _alignment ?? default;
            set => _alignment = value;
        }

        /// <summary>
        /// If set to true, adds a border around the container.
        /// </summary>
        public bool Border { get; set; }

        /// <summary>
        /// Gets or sets how the container aligns with the page layout (GCDS <c>layout</c>). Not rendered unless set.
        /// </summary>
        public ContainerLayout Layout
        {
            get => _layout ?? default;
            set => _layout = value;
        }

        /// <summary>
        /// If set to true, centers the container; use <see cref="Alignment"/> (<c>alignment="center"</c>) instead.
        /// </summary>
        [Obsolete("Use Alignment (alignment=\"center\") to match the GCDS gcds-container attribute.")]
        public bool Centered
        {
            get => _alignment == HorizontalAlignment.center;
            set => _alignment = value ? HorizontalAlignment.center : null;
        }

        /// <summary>
        /// If set to true, uses the page layout; use <see cref="Layout"/> (<c>layout="page"</c>) instead.
        /// </summary>
        [Obsolete("Use Layout (layout=\"page\") to match the GCDS gcds-container attribute.")]
        public bool MainContainer
        {
            get => _layout == ContainerLayout.page;
            set => _layout = value ? ContainerLayout.page : null;
        }

        /// <summary>
        /// Defines the margin of the container (can be a CSS unit like "px", "em", etc.).
        /// </summary>
        public string? Margin { get; set; }

        /// <summary>
        /// Defines the padding of the container. Default value is "300".
        /// </summary>
        public string? Padding { get; set; }

        /// <summary>
        /// Defines the size of the container. Default is <see cref="SizeTypeEmum.lg"/>.
        /// </summary>
        public SizeTypeEmum Size { get; set; } = SizeTypeEmum.lg;

        /// <summary>
        /// Specifies the tag name to be used for the container (e.g., div, section, etc.).
        /// </summary>
        public string? Tag { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            AddAttributeIfNotNull(output, "alignment", _alignment);
            AddAttributeIfNotNull(output, "border", Border);
            AddAttributeIfNotNull(output, "layout", _layout);
            AddAttributeIfNotNull(output, "margin", Margin);
            AddAttributeIfNotNull(output, "padding", Padding);
            AddAttributeIfNotNull(output, "size", Size);
            AddAttributeIfNotNull(output, "tag", Tag);
            base.Process(context, output);
        }

    }
}
