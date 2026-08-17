using GCFoundation.Components.Enums;
using GCFoundation.Components.Resources;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// Renders an <c>&lt;fdcp-accordion&gt;</c> element as a <c>&lt;div class="fdcp-accordion"&gt;</c>
    /// container with an injected "Open all / Close all" toggle button. Works together with the
    /// <c>FDCPAccordion</c> JavaScript class, which binds to elements with the <c>fdcp-accordion</c>
    /// class to provide single-open-at-a-time behaviour for child <c>gcds-details</c> elements,
    /// plus bulk open/close via the toggle button.
    /// </summary>
    [HtmlTargetElement("fdcp-accordion", Attributes = "accordion-id")]
    public class FdcpAccordionTagHelper(IStringLocalizer<Localization> localizer) : TagHelper
    {
        private readonly IStringLocalizer<Localization> _localizer = localizer;
        /// <summary>
        /// The unique identifier applied to the rendered accordion container. Used by
        /// <c>FDCPAccordion</c> (JavaScript) to bind open/close behaviour and by the
        /// injected toggle button to control all panels within this accordion.
        /// </summary>
        public string AccordionId { get; set; } = string.Empty;

        /// <summary>
        /// When <c>true</c>, multiple sections can remain expanded simultaneously. When <c>false</c>, only one section can be expanded at a time — opening a section automatically collapses any other open section. Defaults to <c>false</c>.
        /// </summary>
        public bool AlwaysOpen { get; set; }

        /// <summary>
        /// Determines where the expand-all/collapse-all toggle buttons are displayed relative to the accordion's sections. Defaults to <see cref="AccordionButtonsPosition.top"/>.
        /// </summary>
        public AccordionButtonsPosition ButtonsPosition { get; set; } = AccordionButtonsPosition.top;
      

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output);

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            var existingClass = output.Attributes["class"]?.Value?.ToString();
            var mergedClass = string.IsNullOrWhiteSpace(existingClass)
                ? "fdcp-accordion"
                : $"fdcp-accordion {existingClass}";

            if (!AlwaysOpen)
            {
                mergedClass += " fdcp-accordion-not-always-open";
            }

            output.Attributes.SetAttribute("class", mergedClass);
            output.Attributes.SetAttribute("id", AccordionId);

            string toggleButtons;
            if (ButtonsPosition == AccordionButtonsPosition.top || ButtonsPosition == AccordionButtonsPosition.both)
            {
                toggleButtons = BuildToggleButtons("top");
                output.PreContent.SetHtmlContent(toggleButtons);
            }
            if (ButtonsPosition == AccordionButtonsPosition.bottom || ButtonsPosition == AccordionButtonsPosition.both)
            {
                toggleButtons = BuildToggleButtons("bottom");
                output.PostContent.SetHtmlContent(toggleButtons);
            }

        }

        private string BuildToggleButtons(string position)
        {
            var expandText = _localizer["Expand_All"];
            var collapseText = _localizer["Collapse_All"];

            string marginClass;
            if (position == "top")
                marginClass = "mb-200";
            else
                marginClass = "mt-200";
                
            
            return $@"<div class=""fdcp-accordion-toggle-buttons {marginClass} justify-content-start"">
                <gcds-button
                    button-id=""fdcp-accordion-{position}-expand-all-button""
                    button-role=""secondary""
                    data-accordion-id=""{AccordionId}"">
                    {expandText}
                </gcds-button>
                <gcds-button
                    button-id=""fdcp-accordion-{position}-collapse-all-button""
                    button-role=""secondary""
                    data-accordion-id=""{AccordionId}"">
                    {collapseText}
                </gcds-button>
            </div>";
        }
    }
}
