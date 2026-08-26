using GCFoundation.Components.Enums;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// A TagHelper for generating elements that are visually hidden but still accessible to screen readers.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;gcds-sr-only tag=&quot;h1&quot;&gt;Skip to main content&lt;/gcds-sr-only&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("gcds-sr-only")]
    public class SrOnlyTagHelper : BaseTagHelper
    {
        /// <summary>
        /// Specifies the tag that should be used for the element (e.g., <code>h1</code>, <code>p</code>).
        /// </summary>
        public SrOnlyTag Tag { get; set; }

        /// <summary>
        /// Processes the TagHelper to add the appropriate attributes to the output element.
        /// This method is called during the rendering of the tag.
        /// </summary>
        /// <param name="context">The <see cref="TagHelperContext"/> for the current tag.</param>
        /// <param name="output">The <see cref="TagHelperOutput"/> representing the HTML element being processed.</param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            AddAttributeIfNotNull(output, "tag", Tag);
            base.Process(context, output);
        }
    }
}
