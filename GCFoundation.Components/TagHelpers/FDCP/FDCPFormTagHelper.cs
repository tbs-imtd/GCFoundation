using GCFoundation.Common.Utilities;
using GCFoundation.Components.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Json;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// A tag helper for rendering a form. It binds to a model and adds the necessary form attributes (method, action). 
    /// Additionally, it generates an error summary if the model contains validation errors.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fdcp-form for=&quot;@Model&quot; method=&quot;post&quot; action=&quot;@Url.Action(&quot;Form&quot;, &quot;Components&quot;)&quot;&gt;
    ///     &lt;fdcp-input for=&quot;@Model.FullName&quot;&gt;&lt;/fdcp-input&gt;
    ///     &lt;gcds-button type=&quot;submit&quot;&gt;Submit&lt;/gcds-button&gt;
    /// &lt;/fdcp-form&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("fdcp-form", Attributes = "for, method, action")]
    public class FDCPFormTagHelper : TagHelper
    {
        /// <summary>
        /// The model that the form is bound to. This model should contain any validation errors
        /// that will be displayed in the error summary if present.
        /// </summary>
        [HtmlAttributeName("for")]
        public BaseViewModel Model { get; set; } = default!;

        /// <summary>
        /// The HTTP method used for the form submission (e.g., GET, POST). Defaults to "post".
        /// </summary>
        [HtmlAttributeName("method")]
        public string Method { get; set; } = "post";

        /// <summary>
        /// The action URL for the form submission.
        /// </summary>
        [HtmlAttributeName("action")]
        public string Action { get; set; } = default!;

        /// <inheritdoc/>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            if (Model == null)
            {
                throw new InvalidOperationException("The model cannot be null.");
            }

#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
            var childContent = output.Content.IsModified ? output.Content.GetContent() :
            (await output.GetChildContentAsync()).GetContent();
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task

            // Start the <form> tag
            output.TagName = "form";
            output.Attributes.SetAttribute("method", Method);
            if (!string.IsNullOrEmpty(Action))
            {
                output.Attributes.SetAttribute("action", Action);
            }

            // Add validation attributes for GCDS v0.39.0+ compatibility
            output.Attributes.SetAttribute("data-gcds-validation", "true");
            output.Attributes.SetAttribute("novalidate", "true"); // Disable HTML5 validation

            var errorSummaryTag = new TagBuilder("gcds-error-summary");
            errorSummaryTag.Attributes.Add("lang", LanguageUtility.GetCurrentApplicationLanguage());

            // Add error summary if model has errors (server-side validation)
            if (!Model.IsValid)
            {
                var errorLinks = Model.Errors.ToDictionary(
                    kvp => $"#{kvp.Key}", // Convert field names to anchor links
                    kvp => string.Join(" ", kvp.Value) // Join multiple errors per field
                );

                var errorJson = JsonSerializer.Serialize(errorLinks);
                errorSummaryTag.Attributes.Add("error-links", errorJson);
            }

            // Add error summary at the beginning of form content
            output.Content.AppendHtml(errorSummaryTag);

            // Add form content
            output.Content.AppendHtml(childContent);
        }
    }
}