using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// Represents a custom tag helper for rendering a <c>&lt;gcds-fieldset&gt;</c> element.
    /// Used to generate a fieldset with a legend, optional hint, error message, and other attributes for form inputs.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;gcds-fieldset fieldset-id=&quot;contact&quot; legend=&quot;Contact information&quot; required=&quot;true&quot;&gt;
    /// &lt;/gcds-fieldset&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("gcds-fieldset")]
    public class FieldsetTagHelper : BaseTagHelper
    {
        /// <summary>
        /// The unique identifier for the fieldset element.
        /// </summary>
        public required string FieldsetId { get; set; }

        /// <summary>
        /// The legend text displayed inside the fieldset.
        /// </summary>
        public required string Legend { get; set; }

        /// <summary>
        /// Specifies whether the fieldset should be disabled. 
        /// When set to true, the fieldset and all its child elements will be disabled.
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// The error message associated with the fieldset, displayed if validation fails.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// The hint text displayed beneath the fieldset for additional guidance.
        /// </summary>
        public string? Hint { get; set; }

        /// <summary>
        /// Specifies whether the fieldset is required. If true, the fieldset will be marked as required.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Defines when to validate the fieldset, based on user interaction. Default is "blur".
        /// Possible values include "blur", "change", or "submit".
        /// </summary>
        public string? ValidateOn { get; set; } = "blur";

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            AddAttributeIfNotNull(output, "fieldset-id", FieldsetId);
            AddAttributeIfNotNull(output, "legend", Legend);
            AddAttributeIfNotNull(output, "disabled", Disabled);
            AddAttributeIfNotNull(output, "error-message", ErrorMessage);
            AddAttributeIfNotNull(output, "hint", Hint);
            AddAttributeIfNotNull(output, "lang", Lang);
            AddAttributeIfNotNull(output, "required", Required);
            AddAttributeIfNotNull(output, "validate-on", ValidateOn);
            base.Process(context, output);
        }
    }
}
