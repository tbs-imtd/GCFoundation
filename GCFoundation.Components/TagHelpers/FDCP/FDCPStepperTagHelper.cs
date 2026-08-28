using GCFoundation.Components.Enums;
using GCFoundation.Components.Models;
using GCFoundation.Components.Resources;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// A tag helper that renders a step indicator/progress component for multi-step processes.
    /// Displays numbered steps with labels and indicates completed, active, and upcoming steps.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fdcp-stepper current-step=&quot;2&quot; heading-tag=&quot;h3&quot;
    ///     steps=&quot;@(new List&lt;StepperStep&gt; { new StepperStep { StepNumber = 1, Label = &quot;Step One&quot;, DisplayMode = StepperStepDisplayMode.Number }, new StepperStep { StepNumber = 2, Label = &quot;Step Two&quot;, DisplayMode = StepperStepDisplayMode.Number }, new StepperStep { StepNumber = 3, Label = &quot;Step Three&quot;, DisplayMode = StepperStepDisplayMode.Number } })&quot;&gt;
    /// &lt;/fdcp-stepper&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("fdcp-stepper")]
    public class FDCPStepperTagHelper : TagHelper
    {
        /// <summary>
        /// Gets or sets the current active step number (1-based index).
        /// </summary>
        public int CurrentStep { get; set; } = 1;

        /// <summary>
        /// The HTML heading tag to be used (e.g., h1, h2, etc.) in the stepper's heading.
        /// Default is <see cref="HeadingTag.h2"/>.
        /// </summary>
        public HeadingTag HeadingTag { get; set; } = HeadingTag.h2;

        /// <summary>
        /// The main heading text to display in the stepper's heading.
        /// </summary>
        public string HeadingTitle { get; set; } = Stepper.Title_Default;

        /// <summary>
        /// Gets or sets the collection of steps for the process.
        /// </summary>
        public IEnumerable<StepperStep> Steps { get; set; } = new List<StepperStep>();

        /// <summary>
        /// Processes the tag helper and generates the HTML output for the stepper component.
        /// </summary>
        /// <param name="context">Contains information associated with the current HTML tag.</param>
        /// <param name="output">The output that will be rendered by the tag helper.</param>
        /// <exception cref="ArgumentNullException">Thrown when output is null.</exception>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output);

            output.TagName = "div";
            var html = new StringBuilder();

            html.AppendLine(CultureInfo.InvariantCulture, $"<gcds-heading tag='{HeadingTag}'>{HeadingTitle}</gcds-heading>");
            html.AppendLine("<div class='fdcp-stepper'>");

            foreach (var step in Steps)
            {
                if (step.IsHidden || string.IsNullOrWhiteSpace(step.Label))
                    continue;

                html.AppendLine(CultureInfo.InvariantCulture, $"<div class='fdcp-step {step.GetStatusByCurrentStep(CurrentStep)}'>");

                // Circle.
                html.AppendLine(CultureInfo.InvariantCulture, $"<div class='fdcp-step-circle'>{step.GetDisplayHtml(CurrentStep)}</div>");

                // Label.
                string labelHtml;
                if (step.IsLink && !string.IsNullOrEmpty(step.LinkUrl))
                {
                    labelHtml = string.Format(
                        CultureInfo.InvariantCulture,
                        "<gcds-link href='{0}'>{1}</gcds-link>",
                        step.LinkUrl,
                        step.Label);
                }
                else
                {
                    labelHtml = step.Label;
                }
                html.AppendLine(CultureInfo.InvariantCulture, $"<div class='fdcp-step-label'>{labelHtml}</div>");

                // Status badge (if defined).
                if (!string.IsNullOrEmpty(step.StatusBadgeLabel))
                {
                    var badgeHtml = RenderStatusBadge(step);
                    if (!string.IsNullOrEmpty(badgeHtml))
                        html.AppendLine(badgeHtml);
                }

                html.AppendLine("</div>"); // <div class='fdcp-step'>
            }

            html.AppendLine("</div>");
            output.Content.SetHtmlContent(html.ToString());
        }

        /// <summary>
        /// Renders the status badge for a step by delegating to <see cref="FDCPBadgeTagHelper"/>.
        /// This ensures any changes to the badge tag helper are automatically reflected here.
        /// </summary>
        /// <param name="step">The step whose status badge should be rendered.</param>
        /// <returns>HTML string for the badge, or an empty string if rendering fails.</returns>
        private static string RenderStatusBadge(StepperStep step)
        {
            if (string.IsNullOrEmpty(step.StatusBadgeLabel))
                return string.Empty;

            var badgeHelper = new FDCPBadgeTagHelper
            {
                Style = step.StatusBadgeStyle,
                Inverted = step.StatusBadgeStyleInverted ?? false
            };

            var context = new TagHelperContext(
                tagName: "fdcp-badge",
                allAttributes: new TagHelperAttributeList(),
                items: new Dictionary<object, object?>(),
                uniqueId: string.Create(CultureInfo.InvariantCulture, $"fdcp-badge-{step.StepNumber}")
            );

            var childContent = new DefaultTagHelperContent();
            childContent.SetHtmlContent(step.StatusBadgeLabel);

            var output = new TagHelperOutput(
                "fdcp-badge",
                new TagHelperAttributeList(),
                (useCachedResult, encoder) =>
                {
                    return Task.FromResult<TagHelperContent>(childContent);
                })
            {
                TagMode = TagMode.StartTagAndEndTag
            };

            badgeHelper.ProcessAsync(context, output).GetAwaiter().GetResult();

            using var writer = new StringWriter(CultureInfo.InvariantCulture);
            output.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }
    }
}