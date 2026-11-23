using GCFoundation.Components.Enums;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.GCDS
{
    /// <summary>
    /// Represents a custom tag helper for rendering a <c>&lt;gcds-fieldset&gt;</c> element.
    /// Used to group related form inputs under a legend with an optional hint.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;gcds-fieldset legend=&quot;Contact information&quot; legend-size=&quot;h3&quot; hint=&quot;Tell us how to reach you.&quot;&gt;
    /// &lt;/gcds-fieldset&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("gcds-fieldset")]
    public class FieldsetTagHelper : BaseTagHelper
    {
        private LegendSize? _legendSize;

        /// <summary>
        /// The legend text displayed inside the fieldset (GCDS <c>legend</c>).
        /// </summary>
        public required string Legend { get; set; }

        /// <summary>
        /// The font size of the legend (GCDS <c>legend-size</c>). Not rendered unless set.
        /// </summary>
        public LegendSize LegendSize
        {
            get => _legendSize ?? default;
            set => _legendSize = value;
        }

        /// <summary>
        /// The hint text displayed below the legend (GCDS <c>hint</c>).
        /// </summary>
        public string? Hint { get; set; }

        /// <summary>
        /// The fieldset identifier. GCDS v1 removed <c>fieldset-id</c>; this value is ignored.
        /// </summary>
        [Obsolete("GCDS v1 no longer supports fieldset-id on gcds-fieldset; this value is ignored. Use id instead.")]
        public string? FieldsetId { get; set; }

        /// <summary>
        /// Whether the fieldset is disabled. GCDS v1 removed <c>disabled</c> from <c>gcds-fieldset</c>; this value is ignored.
        /// </summary>
        [Obsolete("GCDS v1 no longer supports disabled on gcds-fieldset; this value is ignored. Disable the child form components instead.")]
        public bool Disabled { get; set; }

        /// <summary>
        /// The fieldset error message. GCDS v1 removed <c>error-message</c> from <c>gcds-fieldset</c>; this value is ignored.
        /// </summary>
        [Obsolete("GCDS v1 no longer supports error-message on gcds-fieldset; this value is ignored. Set error-message on the child form components instead.")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Whether the fieldset is required. GCDS v1 removed <c>required</c> from <c>gcds-fieldset</c>; this value is ignored.
        /// </summary>
        [Obsolete("GCDS v1 no longer supports required on gcds-fieldset; this value is ignored. Set required on the child form components instead.")]
        public bool Required { get; set; }

        /// <summary>
        /// When to validate the fieldset. GCDS v1 removed <c>validate-on</c> from <c>gcds-fieldset</c>; this value is ignored.
        /// </summary>
        [Obsolete("GCDS v1 no longer supports validate-on on gcds-fieldset; this value is ignored. Set validate-on on the child form components instead.")]
        public string? ValidateOn { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            AddAttributeIfNotNull(output, "legend", Legend);
            AddAttributeIfNotNull(output, "legend-size", _legendSize);
            AddAttributeIfNotNull(output, "hint", Hint);
            AddAttributeIfNotNull(output, "lang", Lang);
            base.Process(context, output);
        }
    }
}
