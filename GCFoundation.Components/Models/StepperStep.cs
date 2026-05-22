using System.Diagnostics.CodeAnalysis;
using GCFoundation.Components.Enums;
using System.Globalization;

namespace GCFoundation.Components.Models
{
    /// <summary>
    /// Represents a step within a stepper component that can display numbers or icons
    /// and tracks the step's state and display properties.
    /// </summary>
    public class StepperStep
    {
        /// <summary>
        /// Gets or sets the HTML for the FontAwesome icon to display when the step is completed.
        /// Example: "&lt;i class='fa fa-check'&gt;&lt;/i&gt;"
        /// </summary>
        [StringSyntax("Html")]
        public string? CompletedIconHtml { get; set; }

        /// <summary>
        /// Gets or sets how the step should be displayed (as a number or icon).
        /// Defaults to Number display mode.
        /// </summary>
        public StepperStepDisplayMode DisplayMode { get; set; } = StepperStepDisplayMode.Number;

        /// <summary>
        /// Gets or sets the HTML for the FontAwesome icon to display when the step is in progress.
        /// </summary>
        [StringSyntax("Html")]
        public string? InProgressIconHtml { get; set; }

        /// <summary>
        /// Gets or sets whether the step should be hidden from display.
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Gets or sets whether this step should be rendered as a clickable link.
        /// </summary>
        public bool IsLink { get; set; }

        /// <summary>
        /// Gets or sets the text label describing the step.
        /// </summary>
        public required string Label { get; set; }

        /// <summary>
        /// Gets or sets the URL for navigation when the step is clicked.
        /// </summary>
        [StringSyntax(StringSyntaxAttribute.Uri)]
#pragma warning disable CA1056 // URI-like properties should not be strings
        public string? LinkUrl { get; set; }
#pragma warning restore CA1056 // URI-like properties should not be strings

        /// <summary>
        /// Gets or sets the HTML for the FontAwesome icon to display when the step hasn't been started.
        /// </summary>
        [StringSyntax("Html")]
        public string? NotStartedIconHtml { get; set; }

        /// <summary>
        /// Gets or sets the text of the badge describing the status of the step.
        /// </summary>
        [StringSyntax("Html")]
        public string? StatusBadgeLabel { get; set; }

        /// <summary>
        /// Gets or sets the style of the badge describing the status of the step.
        /// </summary>
        public BadgeStyle StatusBadgeStyle { get; set; } = BadgeStyle.primary;

        /// <summary>
        /// Gets or sets the inverted variant of the style of the badge describing the status of the step.
        /// </summary>
        public bool? StatusBadgeStyleInverted { get; set; }

        /// <summary>
        /// Gets or sets the numerical position of the step in the sequence.
        /// </summary>
        public int StepNumber { get; set; }


        /// <summary>
        /// Generates the HTML display content for the step based on its current state and display mode.
        /// </summary>
        /// <param name="currentStep">The current active step number in the sequence.</param>
        /// <returns>HTML string representing the step's display content. Returns empty string if the step is hidden.</returns>
        public string GetDisplayHtml(int currentStep)
        {
            if (IsHidden)
                return string.Empty;

            if (DisplayMode == StepperStepDisplayMode.Icon)
            {
                return GetStatusByCurrentStep(currentStep) switch
                {
                    StepperStepStatus.completed => !string.IsNullOrEmpty(CompletedIconHtml) ? CompletedIconHtml : StepNumber.ToString(CultureInfo.InvariantCulture),
                    StepperStepStatus.active => !string.IsNullOrEmpty(InProgressIconHtml) ? InProgressIconHtml : StepNumber.ToString(CultureInfo.InvariantCulture),
                    _ => !string.IsNullOrEmpty(NotStartedIconHtml) ? NotStartedIconHtml : StepNumber.ToString(CultureInfo.InvariantCulture)
                };
            }
            return StepNumber.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Determines the status of this step relative to the current step in the sequence.
        /// </summary>
        /// <param name="currentStep">The current active step number in the sequence.</param>
        /// <returns>
        /// Returns the StepperStepStatus representing the status of a given step relative to other steps of a stepper.
        /// </returns>
        public StepperStepStatus GetStatusByCurrentStep(int currentStep)
        {
            if (StepNumber < currentStep)
                return StepperStepStatus.completed;
            else if (StepNumber == currentStep)
                return StepperStepStatus.active;
            else
                return StepperStepStatus.incomplete;
        }
    }
}