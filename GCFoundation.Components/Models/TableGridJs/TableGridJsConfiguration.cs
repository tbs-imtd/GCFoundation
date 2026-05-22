using System.Diagnostics.CodeAnalysis;

namespace GCFoundation.Components.Models.TableGridJs
{
    /// <summary>
    /// Represents the configuration of a Grid.js table.
    /// </summary>
    public class TableGridJsConfiguration
    {
        /// <summary>
        /// Aria-label attribute to be added to the wrapper div and table.
        /// </summary>
        public string? AriaLabel { get; set; }

        /// <summary>
        /// Column definitions.
        /// </summary>
        public IEnumerable<TableGridJsColumn>? Columns { get; set; }

        /// <summary>
        /// (Optional) Url of the API endpoint that will be queried to return the data.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Uri)]
#pragma warning disable CA1056 // URI-like properties should not be strings
        public string? DataUrl { get; set; }
#pragma warning restore CA1056 // URI-like properties should not be strings

        /// <summary>
        /// Size of the page (if paging is enabled).
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Determines whether or not pagination will be enabled for the table.
        /// </summary>
        public bool PaginationEnabled { get; set; }

        /// <summary>
        /// Determines whether or not search will be enabled for the table.
        /// </summary>
        public bool SearchEnabled { get; set; }

        /// <summary>
        /// Determines whether or not sorting will be enabled for the table.
        /// </summary>
        public bool SortingEnabled { get; set; }

        /// <summary>
        /// (Optional) CSS classes to add to the enhanced table element.
        /// </summary>
        public string? Class { get; set; }

        /// <summary>
        /// Localized UI strings.
        /// </summary>
        public TableGridJsLocalization Localization { get; set; } = new TableGridJsLocalization();
    }
}