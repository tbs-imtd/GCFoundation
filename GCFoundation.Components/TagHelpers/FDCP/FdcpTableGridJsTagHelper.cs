using System.Diagnostics.CodeAnalysis;
using GCFoundation.Components.Enums;
using GCFoundation.Components.Models;
using GCFoundation.Components.Models.TableGridJs;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// Renders an accessible Grid.js-powered table container with progressive enhancement.
    /// Outputs a semantic table skeleton for no-JS environments and a data bootstrap for JS.
    /// Server-side sorting, searching, and pagination are enforced.
    /// </summary>
    [HtmlTargetElement("fdcp-table-gridjs")]
    public sealed class FDCPTableGridJsTagHelper : TagHelper
    {
        private static readonly JsonSerializerOptions CamelCaseJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Gets or sets the current ViewContext for accessing HTTP context and request information.
        /// </summary>
        [ViewContext]
        public ViewContext ViewContext { get; set; } = null!;

        /// <summary>
        /// Unique id for the grid container. If not provided, one will be generated.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Required. AJAX endpoint returning the standard envelope: items,total,page,pageSize.
        /// </summary>
        [HtmlAttributeName("ajax-url")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "Endpoint path")] 
        [StringSyntax(StringSyntaxAttribute.Uri)]
        public string AjaxUrl { get; set; } = string.Empty;

        /// <summary>
        /// (Optional) Aria-label for the table element.
        /// </summary>
        public string? AriaLabel { get; set; }

        /// <summary>
        /// Visible caption text (required for WCAG 2.1 (AAA) Standards).
        /// </summary>
        public string Caption { get; set; } = string.Empty;

        /// <summary>
        /// (Optional) CSS classes to add to the enhanced table element.
        /// </summary>
        public string? Class { get; set; }

        /// <summary>
        /// Column definitions.
        /// </summary>
        public IEnumerable<TableGridJsColumn>? Columns { get; set; }

        /// <summary>
        /// Debounce for search in milliseconds (default 300).
        /// </summary>
        public int DebounceMs { get; set; } = 300;

        /// <summary>
        /// Language for client messages.
        /// </summary>
        public Language Lang { get; set; }

        /// <summary>
        /// Localized loading text.
        /// </summary>
        public string? LoadingText { get; set; }

        /// <summary>
        /// Localized string when no records found (fallbacks applied client-side as well).
        /// </summary>
        public string? NoDataText { get; set; }

        /// <summary>
        /// Page size (default 25). Server-side enforced.
        /// </summary>
        public int PageSize { get; set; } = 25;

        /// <summary>
        /// Enable pagination (default true). Always server-side.
        /// </summary>
        public bool PaginationEnabled { get; set; } = true;

        /// <summary>
        /// Enable search (default true). Always server-side.
        /// </summary>
        public bool SearchEnabled { get; set; } = true;

        /// <summary>
        /// Enable sorting (default true). Always server-side.
        /// </summary>
        public bool SortingEnabled { get; set; } = true;

        /// <summary>
        /// (Optional) Summary/description referenced via aria-describedby.
        /// </summary>
        public string? Summary { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output);
            if (string.IsNullOrWhiteSpace(AjaxUrl))
                throw new InvalidOperationException("ajax-url is required for fdcp-table-gridjs.");
            if (string.IsNullOrWhiteSpace(Caption))
                throw new InvalidOperationException("Caption is required for fdcp-table-gridjs to meet WCAG 2.1 (AAA) Standards.");

            var id = string.IsNullOrWhiteSpace(Id) ? $"fdcp-gridjs-{Guid.NewGuid():N}" : Id!;

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("id", id);
            output.Attributes.SetAttribute("class", "fdcp-gridjs-container");

            // Build config payload.
            var config = new TableGridJsConfiguration();
            config.AriaLabel = AriaLabel;
            config.Class = Class;
            config.Columns = Columns;
            config.DataUrl = AjaxUrl;
            config.PageSize = PageSize;
            config.PaginationEnabled = PaginationEnabled;
            config.SearchEnabled = SearchEnabled;
            config.SortingEnabled = SortingEnabled;

            // Localize messages.
            config.Localization.Localize(Lang);
            
            // Overwrite defaults messages (if defined).
            if (!string.IsNullOrEmpty(LoadingText))
                config.Localization.LoadingText = LoadingText;
            if (!string.IsNullOrEmpty(NoDataText))
                config.Localization.NoDataText = NoDataText;

            var cfgJson = JsonSerializer.Serialize(config, CamelCaseJsonOptions);

            // Output markup: live region, controls, semantic table fallback
            var summaryId = !string.IsNullOrWhiteSpace(Summary) ? $"{id}-summary" : null;
            var captionHtml = $"<caption>{System.Net.WebUtility.HtmlEncode(Caption)}</caption>";
            var summaryHtml = summaryId != null ? $"<div id=\"{summaryId}\" class=\"visibility-sr-only\">{System.Net.WebUtility.HtmlEncode(Summary)}</div>" : string.Empty;

            output.Attributes.SetAttribute("data-fdcp-grid", cfgJson);
            output.Content.AppendHtml($@"
{summaryHtml}
<div class='fdcp-gridjs-controls'>
  <!-- Grid.js will render its own search and pagination; this container exists for structure/fallback -->
</div>
<noscript>
  <table class='fdcp-table fdcp-table-hover fdcp-table-striped' role='table' aria-describedby='{summaryId}'>
    {captionHtml}
    <thead>
      <tr>
        {RenderHeaders(Columns)}
      </tr>
    </thead>
    <tbody>
      <tr><td>{System.Net.WebUtility.HtmlEncode(NoDataText ?? (Lang == Language.fr ? "Aucune donnée" : "No data"))}</td></tr>
    </tbody>
  </table>
</noscript>");
        }

        private static string RenderHeaders(IEnumerable<TableGridJsColumn>? columns)
        {
            if (columns == null) return string.Empty;
            return string.Join(string.Empty, columns.Select(c => $"<th scope='col'>{System.Net.WebUtility.HtmlEncode(c.Name)}</th>"));
        }
    }
}