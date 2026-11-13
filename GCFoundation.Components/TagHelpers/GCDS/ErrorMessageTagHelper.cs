using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// A tag helper for rendering an error message with a specified message ID.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;gcds-error-message message-id=&quot;email-error&quot;&gt;
    ///     Enter a valid email address.
    /// &lt;/gcds-error-message&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("gcds-error-message")]
    public class ErrorMessageTagHelper : BaseTagHelper
    {
        /// <summary>
        /// The ID of the error message to be displayed.
        /// This is typically used to reference a localized message or an error message associated with a specific field.
        /// </summary>
        public required string MessageId { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            AddAttributeIfNotNull(output, "message-id", MessageId);
            base.Process(context, output);
        }
    }
}
