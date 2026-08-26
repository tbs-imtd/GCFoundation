using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// A tag helper for rendering a details element with a title and an optional open state.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;gcds-details details-title=&quot;More information&quot;&gt;
    ///     Hidden details.
    /// &lt;/gcds-details&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("gcds-details")]
    public class DetailsTagHelper : BaseTagHelper
    {
        /// <summary>
        /// The title of the details element that will be displayed in the summary.
        /// </summary>
        public required string DetailsTitle { get; set; }

        /// <summary>
        /// A boolean indicating whether the details element should be open by default.
        /// </summary>
        public bool Open { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            AddAttributeIfNotNull(output, "details-title", DetailsTitle);
            AddAttributeIfNotNull(output, "open", Open);
            base.Process(context, output);
        }
    }
}
