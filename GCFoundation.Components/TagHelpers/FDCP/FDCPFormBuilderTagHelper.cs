using GCFoundation.Common.Utilities;
using GCFoundation.Components.Enums;
using GCFoundation.Components.Helpers;
using GCFoundation.Components.Models.FormBuilder;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text;
using System.Linq;
using TextJson = System.Text.Json.JsonSerializer;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// TagHelper for rendering a dynamic form builder using the GC Design System components.
    /// Generates form markup based on the provided <see cref="FormDefinition"/> model.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fdcp-form-builder form=&quot;@Model.SampleFormBuilder.Form&quot;&gt;
    /// &lt;/fdcp-form-builder&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("fdcp-form-builder", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class FDCPFormBuilderTagHelper : TagHelper
    {
        /// <summary>
        /// Gets or sets the form definition used to generate the form UI.
        /// </summary>
        public required FormDefinition Form { get; set; }

        /// <summary>
        /// Options for serializing JSON property names in camel case.
        /// </summary>
        private static readonly JsonSerializerSettings CamelCaseSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = { new StringEnumConverter(new CamelCaseNamingStrategy()) }
        };

        /// <summary>
        /// Options for serializing enums as integers.
        /// </summary>
        private static readonly JsonSerializerSettings DependencySerializerSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        /// <summary>
        /// Processes the tag helper and generates the HTML output for the form builder.
        /// </summary>
        /// <param name="context">The context for the tag helper.</param>
        /// <param name="output">The output for the tag helper.</param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output, nameof(output));

            output.TagName = "div";
            output.Attributes.SetAttribute("class", "gc-form");

            var content = new StringBuilder();
            BuildFormContent(content);
            output.Content.SetHtmlContent(content.ToString());
        }

        private void BuildFormContent(StringBuilder content)
        {
            // Form wrapper with validation attributes for GCDS v0.39.0+ compatibility
            content.AppendFormat(CultureInfo.InvariantCulture,
                "<form action='{0}' method='{1}' class='gc-form' data-gcds-validation='true' novalidate='true'>",
                Form.Action, Form.Method);

            // Error summary component
            content.AppendLine(CultureInfo.InvariantCulture, $"<gcds-error-summary lang='{LanguageUtility.GetCurrentApplicationLanguage()}'></gcds-error-summary>");

            // Form sections
            foreach (var section in Form.Sections)
            {
                content.AppendLine(CultureInfo.InvariantCulture, $@"<gcds-fieldset 
                    fieldset-id='{section.Title}' 
                    legend='{section.Title}' 
                    legend-size='h3' 
                    hint='{section.Hint}'>");

                foreach (var question in section.Questions)
                {
                    content.AppendLine(RenderQuestion(question));
                }

                content.AppendLine("</gcds-fieldset>");
            }

            // Submit button
            content.AppendFormat(CultureInfo.InvariantCulture, 
                @"<gcds-button type='submit' button-role='primary'>{0}</gcds-button>", 
                Form.SubmitButtonText);
            content.AppendLine();

            content.AppendLine("</form>");
        }

        private static string RenderQuestion(FormQuestion question)
        {
            string language = LanguageUtility.GetCurrentApplicationLanguage();
            string isRequired = question.IsRequired ? "required" : "";

            // Base attributes that all components should have
            string baseAttributes = $@"
                id='{question.Id}'
                lang='{language}'
                {isRequired}";

            // Add dependencies attribute if question has dependencies
            if (question.Dependencies?.Any() == true)
            {
                var serializedDeps = JsonConvert.SerializeObject(question.Dependencies, DependencySerializerSettings);
                baseAttributes += $@" data-dependencies='{serializedDeps}'";
            }

            // Add validation rules if present
            if (question.ValidationRules?.Any() == true)
            {
                var validationRules = question.ValidationRules.Select(rule => new
                {
                    type = rule.Type.ToString().ToLowerInvariant(),
                    pattern = rule.Pattern,
                    min = rule.Min,
                    max = rule.Max,
                    errorMessages = rule.ErrorMessages
                });
                var serializedRules = JsonConvert.SerializeObject(validationRules, CamelCaseSettings);
                baseAttributes += $@" data-validation-rules='{serializedRules}'";
            }

            // Add default validation event (GCDS documentation specifies blur as default)
            if (question.IsRequired || question.ValidationRules?.Any() == true)
            {
                string validateOn = question.ValidateOnBlur ? "blur" : "blur"; // Default to blur as per GCDS
                baseAttributes += $@" validate-on=""{validateOn}""";
            }

            // Add error message if present
            if (!string.IsNullOrEmpty(question.ErrorMessage))
            {
                baseAttributes += $@" error-message=""{question.ErrorMessage}""";
            }

            // Common attributes for all input types
            string commonAttributes = $@"
                name='{question.Id}'
                label='{question.Label}'
                hint='{question.Hint}'
                {baseAttributes}";

            return $@"<div class='gc-form-group'>{question.Type switch
            {
                QuestionType.Text => $@"<gcds-input 
                    type='text'
                    input-id='{question.Id}'
                    value='{question.Value ?? ""}'
                    {(question.Size.HasValue ? $"size='{question.Size}'" : "")}
                    {commonAttributes}>
                </gcds-input>",

                QuestionType.Email => $@"<gcds-input 
                    type='email'
                    input-id='{question.Id}'
                    value='{question.Value ?? ""}'
                    {(question.Size.HasValue ? $"size='{question.Size}'" : "")}
                    {commonAttributes}>
                </gcds-input>",

                QuestionType.Password => $@"<gcds-input 
                    type='password'
                    input-id='{question.Id}'
                    value='{question.Value ?? ""}'
                    {(question.Size.HasValue ? $"size='{question.Size}'" : "")}
                    {commonAttributes}>
                </gcds-input>",

                QuestionType.Url => $@"<gcds-input 
                    type='url'
                    input-id='{question.Id}'
                    value='{question.Value ?? ""}'
                    {(question.Size.HasValue ? $"size='{question.Size}'" : "")}
                    {commonAttributes}>
                </gcds-input>",

                QuestionType.Number => $@"<gcds-input 
                    type='number'
                    input-id='{question.Id}'
                    value='{question.Value ?? ""}'
                    {(question.Size.HasValue ? $"size='{question.Size}'" : "")}
                    {commonAttributes}>
                </gcds-input>",

                QuestionType.Radio => BuildRadioGroup(question, language, commonAttributes),

                QuestionType.Checkbox => BuildCheckboxes(question, language, commonAttributes),

                QuestionType.Dropdown => $@"<gcds-select
                    select-id='{question.Id}'
                    default-value='Select option'
                    {commonAttributes}>
                    {BuildOptions(question.Options)}
                </gcds-select>",

                QuestionType.TextArea => $@"<gcds-textarea 
                    textarea-id='{question.Id}'
                    rows='{question.Size ?? 3}'
                    {GetTextareaMaxLengthAttribute(question)}
                    {commonAttributes}>
                    {question.Value ?? ""}
                </gcds-textarea>",

                QuestionType.Date => $@"<gcds-date-input
                    legend='{question.Label}'
                    name='{question.Id}'
                    format='{question.Format ?? "full"}'
                    value='{question.Value ?? ""}'
                    {baseAttributes}>
                </gcds-date-input>",

                QuestionType.FileUpload => $@"<gcds-input 
                    type='file'
                    input-id='{question.Id}'
                    {commonAttributes}>
                </gcds-input>",
                QuestionType.RichText => BuildRichText(question, language),

                _ => throw new ArgumentException($"Unsupported question type: {question.Type}")
            }}</div>";
        }

        private static string BuildRadioGroup(FormQuestion question, string lang, string commonAttributes)
        {
            // Convert options to the required format for gcds-radios
            var options = question.Options?.Select(option => new
            {
                id = $"{question.Id}_{option.Id}",
                label = option.Label,
                value = option.Value,
                //@checked = (option.Value?.ToString() == question.Value?.ToString()),
                //hint = option.Hint,
            });

            var optionsJson = JsonConvert.SerializeObject(options, CamelCaseSettings);

            return $@"<gcds-radios
                name='{question.Id}'
                legend='{question.Label}'
                legend-size='h3'
                options='{optionsJson}'
                {(question.IsRequired ? "required" : "")}
                {(!string.IsNullOrEmpty(question.ErrorMessage) ? $@"error-message=""{question.ErrorMessage}""" : "")}
                {(!string.IsNullOrEmpty(question.Hint) ? $@"hint=""{question.Hint}""" : "")}
                lang='{lang}'
                id='{question.Id}'>
            </gcds-radios>";
        }

        private static string BuildOptions(IEnumerable<QuestionOption>? options)
        {
            if (options == null) return string.Empty;

            var sb = new StringBuilder();
            foreach (var option in options)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"<option value='{option.Value}'>{option.Label}</option>");
            }
            return sb.ToString();
        }

        private static string BuildCheckboxes(FormQuestion question, string lang, string commonAttributes)
        {
            ArgumentNullException.ThrowIfNull(question.Options, nameof(question.Options));

            // Convert selected values to array of strings for value attribute
            var selectedValues = question.Value is IEnumerable<object> values
                ? values.Select(v => v.ToString()).ToArray()
                : Array.Empty<string>();

            // Convert options to the required format for gcds-checkboxes
            var options = question.Options.Select(option => new
            {
                id = $"{question.Id}_{option.Id}",
                label = option.Label,
                value = option.Value,
                //@checked = selectedValues.Contains(option.Value?.ToString()),
                //hint = option.Hint,
            });

            var optionsJson = JsonConvert.SerializeObject(options, CamelCaseSettings);

            // For multiple checkboxes case
            return $@"<gcds-checkboxes
                name='{question.Id}'
                legend='{question.Label}'
                {(!string.IsNullOrEmpty(question.LegendSize) ? $@"legend-size=""{question.LegendSize}""" : "legend-size=\"h3\"")}
                options='{optionsJson}'
                {(question.IsRequired ? "required" : "")}
                {(!string.IsNullOrEmpty(question.ErrorMessage) ? $@"error-message=""{question.ErrorMessage}""" : "")}
                {(!string.IsNullOrEmpty(question.Hint) ? $@"hint=""{question.Hint}""" : "")}
                validate-on='blur'
                lang='{lang}'
                {commonAttributes}>
            </gcds-checkboxes>";
        }

        /// <summary>
        /// Returns an HTML attribute string if the value is not null; otherwise, returns an empty string.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">The attribute value.</param>
        /// <returns>The attribute string or empty if value is null.</returns>
        private static string AttributeIfNotNull(string name, string? value)
            => value is not null ? $" {name}='{value}'" : string.Empty;

        /// <summary>
        /// Returns a <c>maxlength</c> attribute for <c>gcds-textarea</c> from <see cref="FormQuestion.MaxLength"/> or a <see cref="ValidationRuleType.MaxLength"/> rule.
        /// </summary>
        private static string GetTextareaMaxLengthAttribute(FormQuestion question)
        {
            int? max = question.MaxLength;
            if (max is null or <= 0)
            {
                var rule = question.ValidationRules?.FirstOrDefault(static r =>
                    r.Type == ValidationRuleType.MaxLength && r.Max.HasValue && r.Max > 0);
                if (rule != null)
                {
                    max = (int)rule.Max!.Value;
                }
            }

            return max is > 0 ? $"maxlength='{max.Value}' " : string.Empty;
        }

        private static string BuildRichText(FormQuestion question, string language)
        {
            var labelId = $"{question.Id}_label";
            var hintId = $"{question.Id}_hint";
            var editorId = $"{question.Id}_editor";
            var errorId = $"{question.Id}_error";

            var value = question.Value?.ToString() ?? string.Empty;
            var encodedValue = HtmlEncoder.Default.Encode(value);
            var placeholderAttr = !string.IsNullOrEmpty(question.Placeholder)
                ? $" data-placeholder='{HtmlEncoder.Default.Encode(question.Placeholder)}'"
                : string.Empty;
            var styleAttr = !string.IsNullOrEmpty(question.Height)
                ? $" style='height: {question.Height};'"
                : string.Empty;
            var templatesAttr = string.Empty;
            if (question.Templates?.Count > 0)
            {
                var serializedTemplates = TextJson.Serialize(question.Templates);
                templatesAttr = $" data-templates='{HtmlEncoder.Default.Encode(serializedTemplates)}'";
            }

            var toolbar = question.RichTextToolbar.ToString().ToLowerInvariant();

            var sb = new StringBuilder();
            sb.AppendLine("<div class='gc-form-group fdcp-rich-text-container'>");

            // Use a span (not <label for>) because the editable control is a contenteditable div.
            // The hidden input is for form submission only; association is via aria-labelledby on .ql-editor.
            sb.AppendFormat(CultureInfo.InvariantCulture,
                "<span class='fdcp-rich-text-label gcds-label' id='{0}' lang='{3}'>{1}{2}</span>",
                labelId,
                HtmlEncoder.Default.Encode(question.Label),
                question.IsRequired ? "<span class='required'>*</span>" : string.Empty,
                language);
            sb.AppendLine();

            if (!string.IsNullOrEmpty(question.Hint))
            {
                sb.AppendFormat(CultureInfo.InvariantCulture,
                    "<p class='fdcp-rich-text-hint gcds-hint' id='{0}'>{1}</p>",
                    hintId,
                    HtmlEncoder.Default.Encode(question.Hint));
                sb.AppendLine();
            }

            sb.AppendLine("<div class='fdcp-rich-text-wrapper'>");
            // The outer div is a layout container only. The textbox role and ARIA
            // associations (aria-labelledby, aria-describedby, aria-required) are applied
            // at runtime to the inner .ql-editor by fdcp-rich-text.js, so they must NOT be
            // duplicated here (avoids two nested textboxes sharing one label - WCAG 4.1.2).
            sb.AppendFormat(CultureInfo.InvariantCulture,
                "<div id='{0}' class='fdcp-rich-text-editor' data-fdcp-rich-text='true' data-for='{1}' data-toolbar='{2}' data-error-id='{6}' lang='{7}'{3}{4}{5}></div>",
                editorId,
                question.Id,
                toolbar,
                placeholderAttr,
                styleAttr,
                templatesAttr,
                errorId,
                language);
            sb.AppendLine();
            sb.AppendLine("</div>");

            sb.AppendFormat(CultureInfo.InvariantCulture,
                "<input type='hidden' id='{0}' name='{0}' value='{1}' aria-hidden='true' {2} />",
                question.Id,
                encodedValue,
                question.IsRequired ? "required='required'" : string.Empty);
            sb.AppendLine();

            if (!string.IsNullOrEmpty(question.ErrorMessage))
            {
                sb.AppendFormat(CultureInfo.InvariantCulture,
                    "<gcds-error-message message-id='{0}' id='{0}'>{1}</gcds-error-message>",
                    errorId,
                    HtmlEncoder.Default.Encode(question.ErrorMessage));
                sb.AppendLine();
            }

            sb.AppendLine("</div>");
            return sb.ToString();
        }
    }
}
