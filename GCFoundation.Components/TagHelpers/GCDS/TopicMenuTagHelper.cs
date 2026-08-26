using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// Represents a GC Design System topic menu TagHelper.
    /// Renders a <c>&lt;gcds-topic-menu&gt;</c> element with attributes based on the specified properties.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;gcds-topic-menu home=&quot;true&quot;&gt;&lt;/gcds-topic-menu&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("gcds-topic-menu")]
    public class TopicMenuTagHelper : BaseTagHelper
    {
        /// <summary>
        /// Indicates whether the topic menu should be rendered in a home page context.
        /// If <c>true</c>, the <c>home</c> attribute will be added to the element.
        /// </summary>
        public bool Home { get; set; }

        /// <summary>
        /// Processes the tag helper and adds the <c>home</c> and <c>lang</c> attributes to the output element
        /// if their values are set.
        /// </summary>
        /// <param name="context">Contains information associated with the current HTML tag.</param>
        /// <param name="output">A writer to modify the output HTML element.</param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            AddAttributeIfNotNull(output, "home", Home);
            AddAttributeIfNotNull(output, "lang", Lang);
            base.Process(context, output);
        }
    }
}
