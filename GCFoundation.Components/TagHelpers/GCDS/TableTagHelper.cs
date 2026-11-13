using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// TagHelper for rendering a GC Design System compliant table component.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;gcds-table columns='[{&quot;field&quot;:&quot;name&quot;,&quot;header&quot;:&quot;Name&quot;}]' data='[{&quot;name&quot;:&quot;Alice&quot;}]' filter=&quot;true&quot;&gt;&lt;/gcds-table&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("gcds-table")]
    public class TableTagHelper : BaseTagHelper
    {
        /// <summary>
        /// The columns to display in the table, as a JSON array of column definition objects.
        /// Each object maps to a field in <see cref="Data"/> and can specify header text, alignment, and sort behavior.
        /// </summary>
        public string? Columns { get; set; }

        /// <summary>
        /// The rows to display in the table, as a JSON array of row objects.
        /// Each object's keys must match the <c>field</c> values defined in <see cref="Columns"/>.
        /// </summary>
        public string? Data { get; set; }

        /// <summary>
        /// Whether a filter input is displayed above the table, allowing users to narrow rows by keyword.
        /// </summary>
        public bool Filter { get; set; }

        /// <summary>
        /// The default keyword applied to the filter input when the table loads.
        /// Only used when <see cref="Filter"/> is <c>true</c>.
        /// </summary>
        public string? FilterValue { get; set; }

        /// <summary>
        /// Whether the table's rows are split across multiple pages.
        /// </summary>
        public bool Pagination { get; set; }

        /// <summary>
        /// The page displayed when the table first loads.
        /// Only used when <see cref="Pagination"/> is <c>true</c>.
        /// </summary>
        public int? PaginationCurrentPage { get; set; }

        /// <summary>
        /// The number of rows displayed per page.
        /// Only used when <see cref="Pagination"/> is <c>true</c>.
        /// </summary>
        public int? PaginationSize { get; set; }

        /// <summary>
        /// The page size choices available in the pagination control.
        /// Including <c>0</c> adds an "All" option that displays every row at once.
        /// </summary>
        public string? PaginationSizeOptions { get; set; }

        /// <summary>
        /// Whether users can sort the table by clicking column headers.
        /// Individual columns can override this via their <c>sort</c> property in <see cref="Columns"/>.
        /// </summary>
        public bool Sort { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            AddAttributeIfNotNull(output, "columns", Columns);
            AddAttributeIfNotNull(output, "data", Data);

            AddAttributeIfNotNull(output, "filter", Filter);
            if (Filter)
                AddAttributeIfNotNull(output, "filter-value", FilterValue);
            
            AddAttributeIfNotNull(output, "pagination", Pagination);
            if (Pagination)
            {    
                AddAttributeIfNotNull(output, "pagination-current-page", PaginationCurrentPage);
                AddAttributeIfNotNull(output, "pagination-size", PaginationSize);
                AddAttributeIfNotNull(output, "pagination-size-options", PaginationSizeOptions);
            }

            AddAttributeIfNotNull(output, "sort", Sort);

            base.Process(context, output);
        }
    }
}
