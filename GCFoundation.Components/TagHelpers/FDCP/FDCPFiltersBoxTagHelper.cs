using GCFoundation.Components.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;
using System.Text;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// A TagHelper that renders a filter panel with collapsible filter categories.
    /// Intended for use in FDCP (Filtered Data Control Panel) scenarios.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fdcp-filters-box title=&quot;Filters&quot; filters=&quot;@(new[] { new SearchFilterCategory { Title = &quot;Funding type&quot;, SearchFilterCategoryId = &quot;funding&quot;, Filters = new[] { new SearchFilterOption { Title = &quot;Grants&quot;, Count = 45, Name = &quot;grants&quot; } } } })&quot;&gt;
    /// &lt;/fdcp-filters-box&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("fdcp-filters-box")]
    public class FDCPFiltersBoxTagHelper : TagHelper
    {
        /// <summary>
        /// The title displayed at the top of the filter panel.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// A collection of filter categories to render in the filter panel.
        /// Each category can contain multiple filter options.
        /// </summary>
        public required IEnumerable<SearchFilterCategory> Filters { get; set; }

        /// <summary>
        /// Generates the HTML output for the filter box.
        /// </summary>
        /// <param name="context">The context in which the tag helper is executed.</param>
        /// <param name="output">The output of the tag helper.</param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "filter-panel");

            StringBuilder sb = new StringBuilder();

            sb.Append(CultureInfo.InvariantCulture, $@"<h3>{Title}</h3>");

            foreach (SearchFilterCategory category in Filters)
            {
                sb.Append(CultureInfo.InvariantCulture, $@"<div class='filter-section'>");

                sb.Append(CultureInfo.InvariantCulture, $@"<button 
                    class='fdcp-collapse-button'
                    data-fdcp-collapse-toggle='collapse-{category.SearchFilterCategoryId}' 
                    aria-expanded='{category.IsOpen.ToString().ToLower(CultureInfo.CurrentCulture)}'
                    aria-controls='collapse-{category.SearchFilterCategoryId}'>
                    {category.Title}
                </button>");

                sb.Append(CultureInfo.InvariantCulture, $@"<div class='fdcp-collapse {(category.IsOpen ? "fdcp-show" : "")}' id='collapse-{category.SearchFilterCategoryId}'>");

                foreach (SearchFilterOption filter in category.Filters)
                {
                    sb.Append("<div class='filter-option'>");
                    sb.Append(CultureInfo.InvariantCulture, $@"<input type='checkbox' name='{filter.Name}' id='{filter.Name}' />");
                    sb.Append(CultureInfo.InvariantCulture, $@"<label for='{filter.Name}' class=''>{filter.Title}</label>");
                    sb.Append(CultureInfo.InvariantCulture, $@"<span class='filter-count'>{filter.Count}</span>");
                    sb.Append($@"</div>");
                }
                sb.Append($@"</div>");
                sb.Append($@"</div>");

            }

            output.Content.SetHtmlContent(sb.ToString());
        }
    }
}
