using GCFoundation.Components.Enums;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// Represents a tag helper for creating a responsive grid layout using gc design system.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;gcds-grid columns=&quot;1fr 1fr&quot; gap=&quot;300&quot; tag=&quot;div&quot;&gt;
    /// &lt;/gcds-grid&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("gcds-grid")]
    public class GridTagHelper : BaseTagHelper
    {
        /// <summary>
        /// Defines the alignment of content in the grid container.
        /// </summary>
        public AlignContent? AlignContent { get; set; }

        /// <summary>
        /// Defines the alignment of items in the grid container.
        /// </summary>
        public AlignItem? AlignItem { get; set; }

        /// <summary>
        /// Specifies the number of columns for the grid.
        /// </summary>
        public string? Columns { get; set; }

        /// <summary>
        /// Specifies the number of columns for the grid on desktop screens.
        /// </summary>
        public string? ColumnsDesktop { get; set; }

        /// <summary>
        /// Specifies the number of columns for the grid on tablet screens.
        /// </summary>
        public string? ColumnsTablet { get; set; }

        /// <summary>
        /// Defines the container width (full, custom, etc.) for the grid.
        /// </summary>
        public SizeTypeEmum? Container { get; set; }

        /// <summary>
        /// Defines the display type of the grid (grid or other options).
        /// </summary>
        public GridDisplay? Display { get; set; }

        /// <summary>
        /// Indicates whether to make all rows have equal height.
        /// </summary>
        public bool EqualRowHeight { get; set; }

        /// <summary>
        /// Defines the gap between items in the grid.
        /// </summary>
        public string? Gap { get; set; }

        /// <summary>
        /// Defines the gap between items in the grid on desktop screens.
        /// </summary>
        public string? GapDesktop { get; set; }

        /// <summary>
        /// Defines the gap between items in the grid on tablet screens.
        /// </summary>
        public string? GapTablet { get; set; }

        /// <summary>
        /// Defines the justification of content in the grid.
        /// </summary>
        public AlignContent? JustifyContent { get; set; }

        /// <summary>
        /// Defines the justification of items in the grid.
        /// </summary>
        public AlignItem? JustifyItems { get; set; }

        /// <summary>
        /// Defines the alignment of content in the grid with both axes.
        /// </summary>
        public AlignContent? PlaceContent { get; set; }

        /// <summary>
        /// Defines the alignment of items in the grid with both axes.
        /// </summary>
        public AlignItem? PlaceItems { get; set; }

        /// <summary>
        /// Specifies the tag used to render the grid container (e.g., "div").
        /// </summary>
        public string? Tag { get; set; } = "div";

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            AddAttributeIfNotNullWithCaseConversion(output, "align-content", AlignContent);
            AddAttributeIfNotNullWithCaseConversion(output, "align-items", AlignItem);
            AddAttributeIfNotNull(output, "columns", Columns);
            AddAttributeIfNotNull(output, "columns-desktop", ColumnsDesktop);
            AddAttributeIfNotNull(output, "columns-tablet", ColumnsTablet);
            AddAttributeIfNotNull(output, "container", Container);
            AddAttributeIfNotNull(output, "display", Display);
            if (EqualRowHeight)
            {
                output.Attributes.SetAttribute(new TagHelperAttribute("equal-row-height"));
            }
            AddAttributeIfNotNull(output, "gap", Gap);
            AddAttributeIfNotNull(output, "gap-desktop", GapDesktop);
            AddAttributeIfNotNull(output, "gap-tablet", GapTablet);

            AddAttributeIfNotNullWithCaseConversion(output, "justify-content", JustifyContent);
            AddAttributeIfNotNullWithCaseConversion(output, "justify-items", JustifyItems);
            AddAttributeIfNotNullWithCaseConversion(output, "place-content", PlaceContent);
            AddAttributeIfNotNullWithCaseConversion(output, "place-items", PlaceItems);
            AddAttributeIfNotNull(output, "tag", Tag);

            base.Process(context, output);
        }

    }
}
