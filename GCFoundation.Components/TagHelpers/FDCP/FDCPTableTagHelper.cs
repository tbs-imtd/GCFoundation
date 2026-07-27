using GCFoundation.Common.Utilities;
using GCFoundation.Components.Attributes.Table;
using GCFoundation.Components.Enums;
using GCFoundation.Components.Models.TableBuilder;
using GCFoundation.Components.TagHelpers.GCDS;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;      
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// Renders a data table using the gcds-table element.
    /// Use &lt;fdcp-table&gt; in your Razor views to generate a table, either by binding row models
    /// via <c>from</c>, or by supplying explicit <c>columns</c>/<c>data</c> JSON.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="FDCPTableTagHelper"/> class.
    /// </remarks>
    /// <param name="localizerFactory">
    /// The factory used to resolve an <see cref="IStringLocalizer"/> for a column's
    /// <see cref="TableColumnDefinitionAttribute.ResourceType"/>, so column headers can be
    /// localized from resource files rather than hardcoded strings.
    /// </param>
    [HtmlTargetElement("fdcp-table", Attributes = "caption, rows")]
    [HtmlTargetElement("fdcp-table", Attributes = "caption, column-definitions, rows")]
    public class FDCPTableTagHelper(IStringLocalizerFactory localizerFactory) : TableTagHelper
    {

        /// <summary>
        /// The column definitions for the table. If <c>null</c> or empty, columns are resolved
        /// automatically from the properties of the row model in <see cref="Rows"/>.
        /// </summary>
        public ICollection<ColumnDefinition>? ColumnDefinitions { get; set; }

        /// <summary>
        /// The row data to render in the table. Each element represents one row. If
        /// <see cref="ColumnDefinitions"/> is provided, its properties supply the cell values matched
        /// by <see cref="ColumnDefinition.Field"/>; otherwise, its properties are also used to resolve
        /// the columns themselves.
        /// </summary>
        public IEnumerable<Object>? Rows { get; set; }

        /// <summary>
        /// The accessible name given to the table via the <c>caption</c> slot, so assistive technologies
        /// can identify and announce it. Rendered as the table's caption heading.
        /// </summary>
        public string? Caption { get; set; }

        /// <summary>
        /// Additional detail text shown under the caption heading, providing further context about the table.
        /// </summary>
        public string? CaptionDetail { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            if (ColumnDefinitions == null)
                BuildFromRows();

            if (ColumnDefinitions != null && Rows != null)
                BuildFromColsAndRows();            
            
            output.TagName = "gcds-table";
            output.TagMode = TagMode.StartTagAndEndTag;

            base.Process(context, output);
            output.PreContent.SetHtmlContent(BuildHtml());
        }

        #region BuildColumnsAndData
        private void BuildFromRows()
        {
            ResolveColumns();
            BuildFromColsAndRows();
        }

        private void BuildFromColsAndRows()
        {
            Columns = JsonSerializer.Serialize(ColumnDefinitions, JsonOptionsUtility.CamelCaseIgnoreNull);
            Data = JsonSerializer.Serialize(Rows!, JsonOptionsUtility.CamelCase);
        }
        #endregion

        #region BuildHtmlContent
        private string BuildHtml()
        {
            string captionDetailHtml = string.IsNullOrEmpty(CaptionDetail) ? string.Empty : $"<gcds-text>{CaptionDetail}</gcds-text>";
            string html = $"""
                <div slot="caption">
                    <gcds-heading tag="h5">{Caption}</gcds-heading>
                    {captionDetailHtml}
                </div>
                """;
            return html;
        }
           
        #endregion

        #region Resolvers
        private void ResolveColumns()
        {
            if (Rows != null && Rows.Any())
            {
                Type type = Rows.First().GetType();
                var properties = type != null ? type.GetProperties() : null;
                ColumnDefinitions = new List<ColumnDefinition>();
                if (properties == null)
                    return;
                foreach (PropertyInfo prop in properties)
                {
                    TableColumnDefinitionAttribute? attribute = prop.GetCustomAttribute<TableColumnDefinitionAttribute>();
                    if (attribute != null)
                    {
                        if (!attribute.IsHidden)
                        {
                            ColumnDefinitions.Add(new ColumnDefinition()
                            {
                                Field = JsonNamingPolicy.CamelCase.ConvertName(prop.Name),
                                Header = ResolveLocalizedHeader(attribute, prop),
                                Slotted = attribute.Slotted,
                                RowHeader = attribute.RowHeader,
                                Sort = attribute.Sort,
                                SortDirection = attribute.SortDirection == SortDirection.none ? null : attribute.SortDirection,
                                Alignment = attribute.Alignment
                            });
                        }
                    }
                }
            }
            return;
        }

        private string ResolveLocalizedHeader(TableColumnDefinitionAttribute attribute, PropertyInfo property)
        {
            string? name = null;

            if (!string.IsNullOrEmpty(attribute.Name))
            {
                name = attribute.ResourceType == null
                    ? attribute.Name
                    : localizerFactory.Create(attribute.ResourceType)[attribute.Name];
            }

            if (name == null)
            {
                ArgumentNullException.ThrowIfNull(property, nameof(property));

                var displayAttr = property.GetCustomAttribute<DisplayAttribute>();
                name = displayAttr?.GetName() ?? property.Name;
            }

            return name;
        }
    }
        #endregion
}
