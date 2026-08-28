using GCFoundation.Components.Enums;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// A tag helper for rendering a date modified element, which can display either the date or version type.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;gcds-date-modified type=&quot;date&quot;&gt;2026-08-19&lt;/gcds-date-modified&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("gcds-date-modified")]
    public class DateModifiedTagHelper : BaseTagHelper
    {
        /// <summary>
        /// The type of the date modified element, either 'date' or 'version'.
        /// </summary>
        public DateModifiedType Type { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            AddAttributeIfNotNull(output, "lang", Lang);
            AddAttributeIfNotNull(output, "type", Type);
            base.Process(context, output);
        }
    }
}
