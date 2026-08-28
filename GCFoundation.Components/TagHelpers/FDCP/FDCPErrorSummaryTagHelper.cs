using GCFoundation.Common.Utilities;
using GCFoundation.Components.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Json;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// A tag helper for rendering an error summary. It binds to a model and generates an HTML element
    /// displaying a summary of validation errors, if any. The model should contain a collection of errors
    /// that will be displayed in the error summary.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fdcp-error-summary for=&quot;@Model&quot;&gt;
    /// &lt;/fdcp-error-summary&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("fdcp-error-summary", Attributes = "for")]
    public class FDCPErrorSummaryTagHelper : TagHelper
    {
        /// <summary>
        /// The model that contains validation errors to be displayed in the error summary.
        /// </summary>
        [HtmlAttributeName("for")]
        public BaseViewModel Model { get; set; } = default!;

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            if (Model == null)
            {
                output.SuppressOutput();
                return;
            }

            var errorJson = JsonSerializer.Serialize(Model.Errors.ToDictionary(
                    kvp => $"#{kvp.Key}",
                    kvp => string.Join(" ", kvp.Value)
                ));
            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = "gcds-error-summary";
            output.Attributes.SetAttribute("lang", LanguageUtility.GetCurrentApplicationLanguage());

            if (!Model.IsValid)
            {
                output.Attributes.SetAttribute("error-links", errorJson);
            }
            output.Attributes.SetAttribute("listen", true);

        }
    }
}
