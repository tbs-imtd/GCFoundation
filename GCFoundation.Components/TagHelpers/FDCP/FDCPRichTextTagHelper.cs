using GCFoundation.Common.Utilities;
using GCFoundation.Components.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// A tag helper for rendering a rich text editor using Quill.js.
    /// Adheres to GCDS guidelines and ensures accessibility (WCAG 2.1 AAA).
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fdcp-rich-text for=&quot;@Model.Bio&quot; label=&quot;Biography&quot; toolbar=&quot;Standard&quot; height=&quot;240px&quot; placeholder=&quot;Enter your biography&quot;&gt;
    /// &lt;/fdcp-rich-text&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("fdcp-rich-text", Attributes = "for")]
    [HtmlTargetElement("fdcp-rich-text", Attributes = "name")]
    public class FDCPRichTextTagHelper : FDCPBaseFormComponentTagHelper
    {
        /// <summary>
        /// Label text for the editor. Used when <c>for</c> is not specified,
        /// or overrides the model display name when <c>for</c> is specified.
        /// </summary>
        [HtmlAttributeName("label")]
        public string? Label { get; set; }

        /// <summary>
        /// Gets or sets the toolbar configuration (Basic, Standard, Full).
        /// </summary>
        public FDCPRichTextToolbar Toolbar { get; set; } = FDCPRichTextToolbar.Basic;

        /// <summary>
        /// Gets or sets the height of the editor. Default is "200px".
        /// </summary>
        public string Height { get; set; } = "200px";

        /// <summary>
        /// Gets or sets the placeholder text.
        /// </summary>
        public string? Placeholder { get; set; }

        /// <summary>
        /// Gets or sets the set of templates available for insertion within the editor.
        /// The dictionary key is the template name, the value is the HTML snippet.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Property is set by tag helper binding")]
        public IDictionary<string, string>? Templates { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            FormFieldContext field = ResolveFormField(new FormFieldResolveOptions
            {
                Label = Label,
                Hint = Hint,
                Value = Value
            });

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            AppendClass(output, "gcds-input-wrapper fdcp-rich-text-container gc-form-group");

            string editorId = $"{field.Id}_editor";
            string hintId = $"{field.Id}_hint";
            string labelId = $"{field.Id}_label";
            string errorId = $"{field.Id}_error";
            string lang = LanguageUtility.GetCurrentApplicationLanguage();

            // Check for validation errors
            bool hasError = ViewContext?.ModelState?.ContainsKey(field.Name) == true &&
                            ViewContext.ModelState[field.Name]?.Errors?.Count > 0;
            string? errorMessage = hasError
                ? ViewContext!.ModelState[field.Name]!.Errors[0].ErrorMessage
                : null;

            // Build aria-describedby IDs list
            var describedByIds = new List<string>();
            if (!string.IsNullOrEmpty(field.Hint))
            {
                describedByIds.Add(hintId);
            }
            if (hasError && !string.IsNullOrEmpty(errorMessage))
            {
                describedByIds.Add(errorId);
            }

            // 1. Render label element
            // Note: We don't use 'for' attribute because the editor is a contenteditable div, not an input.
            // The association is made via aria-labelledby on the editor instead.
            var label = new TagBuilder("span");
            label.AddCssClass("fdcp-rich-text-label");
            label.Attributes.Add("id", labelId);
            label.Attributes.Add("lang", lang);

            var labelTextSpan = new TagBuilder("span");
            labelTextSpan.InnerHtml.Append(field.Label);
            label.InnerHtml.AppendHtml(labelTextSpan);

            if (field.Required)
            {
                var requiredText = GCFoundation.Components.Resources.Localization.Required;
                var requiredSpan = new TagBuilder("span");
                requiredSpan.Attributes.Add("aria-hidden", "true");
                requiredSpan.AddCssClass("label--required");
                requiredSpan.InnerHtml.Append($" ({requiredText})");
                label.InnerHtml.AppendHtml(requiredSpan);
            }

            output.Content.AppendHtml(label);
            
            // 2. Render Hint (if any) - Placed after label following GCDS pattern
            if (!string.IsNullOrEmpty(field.Hint))
            {
                var hintBuilder = new TagBuilder("gcds-hint");
                hintBuilder.Attributes.Add("hint-id", hintId);
                hintBuilder.Attributes.Add("id", hintId); // Required for aria-describedby
                hintBuilder.InnerHtml.Append(field.Hint);
                output.Content.AppendHtml(hintBuilder);
            }

            // 3. Render error message (before the editor, following GCDS pattern)
            if (hasError && !string.IsNullOrEmpty(errorMessage))
            {
                var errorBuilder = new TagBuilder("gcds-error-message");
                errorBuilder.Attributes.Add("message-id", errorId);
                errorBuilder.Attributes.Add("id", errorId); // Required for aria-describedby
                errorBuilder.InnerHtml.Append(errorMessage);
                output.Content.AppendHtml(errorBuilder);
            }

            // 4. Build the editor container with proper ARIA attributes
            var editorBuilder = new TagBuilder("div");
            editorBuilder.Attributes.Add("id", editorId);
            editorBuilder.AddCssClass("fdcp-rich-text-editor");
            editorBuilder.Attributes.Add("data-fdcp-rich-text", "true");
            editorBuilder.Attributes.Add("data-for", field.Id);
            editorBuilder.Attributes.Add("data-toolbar", Toolbar.ToString().ToLowerInvariant());
            editorBuilder.Attributes.Add("data-error-id", errorId);
            editorBuilder.Attributes.Add("style", $"height: {Height};");
            editorBuilder.Attributes.Add("lang", lang);

            if (!string.IsNullOrEmpty(Placeholder))
            {
                editorBuilder.Attributes.Add("data-placeholder", Placeholder);
            }

            if (Templates?.Any() == true)
            {
                string templatesJson = JsonSerializer.Serialize(Templates);
                editorBuilder.Attributes.Add("data-templates", templatesJson);
            }

            // Set aria-invalid if there's an error
            if (hasError)
            {
                editorBuilder.Attributes.Add("aria-invalid", "true");
            }

            // 5. Build the wrapper
            var wrapperBuilder = new TagBuilder("div");
            wrapperBuilder.AddCssClass("fdcp-rich-text-wrapper");
            if (hasError)
            {
                wrapperBuilder.AddCssClass("has-error");
            }
            wrapperBuilder.InnerHtml.AppendHtml(editorBuilder);
            output.Content.AppendHtml(wrapperBuilder);

            // 6. Hidden input for form submission
            var inputBuilder = new TagBuilder("input");
            inputBuilder.Attributes.Add("type", "hidden");
            inputBuilder.Attributes.Add("id", field.Id);
            inputBuilder.Attributes.Add("name", field.Name);
            inputBuilder.Attributes.Add("lang", lang);
            inputBuilder.Attributes.Add("aria-hidden", "true");
            inputBuilder.Attributes.Add("data-error-id", errorId);
            
            if (field.Required)
            {
                inputBuilder.Attributes.Add("required", "required");
                // Store required error message for client-side validation
#pragma warning disable CA1863 // Use CompositeFormat - not a performance-critical path
                var requiredErrorMsg = string.Format(
                    System.Globalization.CultureInfo.CurrentCulture,
                    GCFoundation.Components.Resources.Validation.Field_Required,
                    field.Label);
#pragma warning restore CA1863
                inputBuilder.Attributes.Add("data-required-error", requiredErrorMsg);
            }

            // Store the error message for client-side access if present
            if (hasError && !string.IsNullOrEmpty(errorMessage))
            {
                inputBuilder.Attributes.Add("data-error-message", errorMessage);
            }

            if (!string.IsNullOrEmpty(field.Value))
            {
                inputBuilder.Attributes.Add("value", field.Value);
            }
            output.Content.AppendHtml(inputBuilder);

            // 7. Store label and describedBy info for JavaScript accessibility enhancement
            output.Attributes.SetAttribute("data-label-id", labelId);
            output.Attributes.SetAttribute("data-hint-id", !string.IsNullOrEmpty(field.Hint) ? hintId : "");
            output.Attributes.SetAttribute("data-error-id", errorId);
            output.Attributes.SetAttribute("data-described-by", string.Join(' ', describedByIds));
        }

        private static void AppendClass(TagHelperOutput output, string classNames)
        {
            if (output.Attributes.TryGetAttribute("class", out var existing))
            {
                var merged = string.IsNullOrWhiteSpace(existing.Value?.ToString())
                    ? classNames
                    : $"{existing.Value} {classNames}";
                output.Attributes.SetAttribute("class", merged.Trim());
            }
            else
            {
                output.Attributes.SetAttribute("class", classNames);
            }
        }
    }
}


