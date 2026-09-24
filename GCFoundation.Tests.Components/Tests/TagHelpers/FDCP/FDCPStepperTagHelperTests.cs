using GCFoundation.Components.Enums;
using GCFoundation.Components.Models;
using GCFoundation.Components.TagHelpers.FDCP;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.FDCP
{
    public class FDCPStepperTagHelperTests
    {
        private readonly TagHelperContext _context;
        private readonly TagHelperOutput _output;

        public FDCPStepperTagHelperTests()
        {
            _context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test-id");

            _output = new TagHelperOutput("fdcp-stepper",
                new TagHelperAttributeList(),
                (result, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                });
        }

        [Fact]
        public void Process_WithDefaultValues_RendersCorrectly()
        {
            // Arrange
            var tagHelper = new FDCPStepperTagHelper();

            // Act
            tagHelper.Process(_context, _output);

            // Assert
            Assert.Equal("div", _output.TagName);
            var content = _output.Content.GetContent();
            Assert.Contains("<gcds-heading tag='h2'>Current step</gcds-heading>", content);
            Assert.Contains("<nav class='fdcp-stepper' aria-label='Progress'>", content);
            Assert.Contains("<ol class='fdcp-stepper__list' role='list'>", content);
        }

        [Fact]
        public void Process_WithSteps_RendersAllSteps()
        {
            // Arrange
            var steps = new[]
            {
                new StepperStep { StepNumber = 1, Label = "Intro" },
                new StepperStep { StepNumber = 2, Label = "Info" },
                new StepperStep { StepNumber = 3, Label = "Review" }
            };

            var tagHelper = new FDCPStepperTagHelper
            {
                CurrentStep = 2,
                Steps = steps
            };

            // Act
            tagHelper.Process(_context, _output);

            // Assert
            var content = _output.Content.GetContent();
            Assert.Contains("<gcds-heading tag='h2'>Info</gcds-heading>", content);
            Assert.Contains("class='fdcp-step completed'", content);
            Assert.Contains("class='fdcp-step active'", content);
            Assert.Contains("class='fdcp-step incomplete'", content);
            Assert.Contains("aria-live='polite'", content);
            Assert.Contains("data-stepper-live-region='true'", content);
            Assert.Contains("data-stepper-announcement='Step 2 of 3: Info.'", content);
            Assert.Contains("<li class='fdcp-step active' aria-label='Step 2 of 3: Info (Current step)' aria-current='step'>", content);
            Assert.Contains("<span class='visibility-sr-only'>Step 1 of 3: Intro (Completed)</span>", content);
            Assert.Contains("<span class='visibility-sr-only'>Step 2 of 3: Info (Current step)</span>", content);
            Assert.Contains("<span class='visibility-sr-only'>Step 3 of 3: Review (Upcoming)</span>", content);
            Assert.Contains("<span class='fdcp-step-label' aria-hidden='true'>Intro</span>", content);
            Assert.Contains("<span class='fdcp-step-label' aria-hidden='true'>Info</span>", content);
            Assert.Contains("<span class='fdcp-step-label' aria-hidden='true'>Review</span>", content);
        }

        [Fact]
        public void Process_WithNullOutput_ThrowsArgumentNullException()
        {
            // Arrange
            var tagHelper = new FDCPStepperTagHelper();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => tagHelper.Process(_context, null!));
        }

        [Theory]
        [InlineData(1, "active", "incomplete", "incomplete")]
        [InlineData(2, "completed", "active", "incomplete")]
        [InlineData(3, "completed", "completed", "active")]
        public void Process_CorrectlyAssignsStepClasses(int currentStep, string step1Class, string step2Class, string step3Class)
        {
            // Arrange
            var steps = new[]
            {
                new StepperStep { StepNumber = 1, Label = "Step 1" },
                new StepperStep { StepNumber = 2, Label = "Step 2" },
                new StepperStep { StepNumber = 3, Label = "Step 3" }
            };

            var tagHelper = new FDCPStepperTagHelper
            {
                CurrentStep = currentStep,
                Steps = steps
            };

            // Act
            tagHelper.Process(_context, _output);

            // Assert
            var content = _output.Content.GetContent();
            Assert.Contains($"class='fdcp-step {step1Class}'", content);
            Assert.Contains($"class='fdcp-step {step2Class}'", content);
            Assert.Contains($"class='fdcp-step {step3Class}'", content);
        }

        [Fact]
        public void Process_WithHiddenStep_DoesNotRenderHiddenStep()
        {
            // Arrange
            var steps = new[]
            {
                new StepperStep { StepNumber = 1, Label = "Step 1" },
                new StepperStep { StepNumber = 2, Label = "Step 2", IsHidden = true },
                new StepperStep { StepNumber = 3, Label = "Step 3" }
            };

            var tagHelper = new FDCPStepperTagHelper
            {
                CurrentStep = 1,
                Steps = steps
            };

            // Act
            tagHelper.Process(_context, _output);

            // Assert
            var content = _output.Content.GetContent();
            Assert.Contains("Step 1", content);
            Assert.DoesNotContain("Step 2", content);
            Assert.Contains("Step 3", content);
        }

        [Fact]
        public void Process_WithStepLink_RendersLinkCorrectly()
        {
            // Arrange
            var steps = new[]
            {
                new StepperStep { 
                    StepNumber = 1, 
                    Label = "Step 1",
                    IsLink = true,
                    LinkUrl = "/step1"
                },
                new StepperStep
                {
                    StepNumber = 2,
                    Label = "Step 2"
                }
            };

            var tagHelper = new FDCPStepperTagHelper
            {
                CurrentStep = 2,
                Steps = steps
            };

            // Act
            tagHelper.Process(_context, _output);

            // Assert
            var content = _output.Content.GetContent();
            Assert.Contains("<a class='fdcp-step__link' href='/step1' aria-label='Step 1 of 2: Step 1 (Completed)' data-stepper-focus-trigger='true'>", content);
        }

        [Fact]
        public void Process_WithLinksTabbableFalse_RendersStepLinksAsNonTabbable()
        {
            // Arrange
            var steps = new[]
            {
                new StepperStep { StepNumber = 1, Label = "Step 1", IsLink = true, LinkUrl = "/step1" },
                new StepperStep { StepNumber = 2, Label = "Step 2", IsLink = true, LinkUrl = "/step2" },
                new StepperStep { StepNumber = 3, Label = "Step 3", IsLink = true, LinkUrl = "/step3" }
            };

            var tagHelper = new FDCPStepperTagHelper
            {
                CurrentStep = 2,
                LinksTabbable = false,
                Steps = steps
            };

            // Act
            tagHelper.Process(_context, _output);

            // Assert
            // Non-active step links must be removed from the natural tab order so that Tab from the active step
            // moves directly to the first form input that follows the stepper.
            var content = _output.Content.GetContent();
            Assert.Contains("<a class='fdcp-step__link' href='/step1' aria-label='Step 1 of 3: Step 1 (Completed)' data-stepper-focus-trigger='true' tabindex='-1'>", content);
            Assert.Contains("<a class='fdcp-step__link' href='/step3' aria-label='Step 3 of 3: Step 3 (Upcoming)' data-stepper-focus-trigger='true' tabindex='-1'>", content);
            // The active step remains focusable programmatically with a valid region name.
            Assert.Contains("<div class='fdcp-step__content' tabindex='0' data-stepper-active-step='true' role='region' aria-label='Step 2 of 3: Step 2 (Current step)'>", content);
        }

        [Fact]
        public void Process_WithLinksTabbableTrue_RendersStepLinksWithoutTabindex()
        {
            // Arrange
            var steps = new[]
            {
                new StepperStep { StepNumber = 1, Label = "Step 1", IsLink = true, LinkUrl = "/step1" },
                new StepperStep { StepNumber = 2, Label = "Step 2" }
            };

            var tagHelper = new FDCPStepperTagHelper
            {
                CurrentStep = 2,
                Steps = steps
            };

            // Act
            tagHelper.Process(_context, _output);

            // Assert
            // Default behavior preserved: step links do not carry tabindex when LinksTabbable is true.
            var content = _output.Content.GetContent();
            Assert.DoesNotContain("data-stepper-focus-trigger='true' tabindex=", content);
        }

        [Fact]
        public void Process_ActiveStep_IsProgrammaticallyFocusable()
        {
            // Arrange
            var steps = new[]
            {
                new StepperStep { StepNumber = 1, Label = "Step 1" },
                new StepperStep { StepNumber = 2, Label = "Step 2" },
                new StepperStep { StepNumber = 3, Label = "Step 3" }
            };

            var tagHelper = new FDCPStepperTagHelper
            {
                CurrentStep = 2,
                Steps = steps
            };

            // Act
            tagHelper.Process(_context, _output);

            // Assert
            var content = _output.Content.GetContent();
            // Active step's content wrapper must be focusable so callers (e.g. Next/Previous buttons) can move focus to it,
            // and carry a screen-reader-friendly accessible name so NVDA announces "Step X of Y: Label (Current step)".
            Assert.Contains("<div class='fdcp-step__content' tabindex='0' data-stepper-active-step='true' role='region' aria-label='Step 2 of 3: Step 2 (Current step)'>", content);
            // Non-active, non-link steps should remain plain wrappers.
            Assert.Contains("<div class='fdcp-step__content'>", content);
        }

        [Fact]
        public void Process_WithCustomHeading_RendersHeadingCorrectly()
        {
            // Arrange
            var steps = new[]
            {
                new StepperStep {
                    StepNumber = 1,
                    Label = "Step 1",
                    IsLink = true,
                    LinkUrl = "/step1"
                }
            };

            var tagHelper = new FDCPStepperTagHelper
            {
                CurrentStep = 1,
                HeadingTag = GCFoundation.Components.Enums.HeadingTag.h3,
                HeadingId = "stepper-heading",
                HeadingTitle = "Custom title",
                Steps = steps
            };

            var context = new TagHelperContext(
                new TagHelperAttributeList { { "heading-title", "Custom title" } },
                new Dictionary<object, object>(),
                "test-id");

            // Act
            tagHelper.Process(context, _output);

            // Assert
            var content = _output.Content.GetContent();
            Assert.Contains("<gcds-heading id='stepper-heading' tabindex='-1' tag='h3'>Custom title</gcds-heading>", content);
        }

        [Fact]
        public void Process_WithNullStatusBadgeLabel_DoesNotRenderBadge()
        {
            // Arrange
            var steps = new[]
            {
                new StepperStep
                {
                    StepNumber = 1,
                    Label = "Step 1",
                    StatusBadgeLabel = null
                }
            };

            var tagHelper = new FDCPStepperTagHelper
            {
                CurrentStep = 1,
                Steps = steps
            };

            // Act
            tagHelper.Process(_context, _output);

            // Assert
            var content = _output.Content.GetContent();
            Assert.DoesNotContain("fdcp-badge", content);
        }

        [Fact]
        public void Process_WithEmptyStatusBadgeLabel_DoesNotRenderBadge()
        {
            // Arrange
            var steps = new[]
            {
                new StepperStep
                {
                    StepNumber = 1,
                    Label = "Step 1",
                    StatusBadgeLabel = ""
                }
            };

            var tagHelper = new FDCPStepperTagHelper
            {
                CurrentStep = 1,
                Steps = steps
            };

            // Act
            tagHelper.Process(_context, _output);

            // Assert
            var content = _output.Content.GetContent();
            Assert.DoesNotContain("fdcp-badge", content);
        }

        [Fact]
        public void Process_WithStatusBadgeLabel_RendersBadgeWithDefaultStyle()
        {
            // Arrange
            var steps = new[]
            {
                new StepperStep
                {
                    StepNumber = 1,
                    Label = "Step 1",
                    StatusBadgeLabel = "New"
                }
            };

            var tagHelper = new FDCPStepperTagHelper
            {
                CurrentStep = 1,
                Steps = steps
            };

            // Act
            tagHelper.Process(_context, _output);

            // Assert
            var content = _output.Content.GetContent();
            Assert.Contains("fdcp-badge", content);
            Assert.Contains("fdcp-badge-primary", content);
            Assert.Contains("<span class='fdcp-badge-content'>New</span>", content);
        }

        [Fact]
        public void Process_WithStatusBadgeLabelHtml_RendersHtmlInBadgeContent()
        {
            // Arrange
            var steps = new[]
            {
                new StepperStep
                {
                    StepNumber = 1,
                    Label = "Step 1",
                    StatusBadgeLabel = "<strong>New</strong> <em>today</em>"
                }
            };

            var tagHelper = new FDCPStepperTagHelper
            {
                CurrentStep = 1,
                Steps = steps
            };

            // Act
            tagHelper.Process(_context, _output);

            // Assert
            var content = _output.Content.GetContent();
            // HTML is intentionally preserved so callers can embed SR-only helper text inside badge content.
            Assert.Contains("<span class='fdcp-badge-content'><strong>New</strong> <em>today</em></span>", content);
            Assert.DoesNotContain("&lt;strong&gt;", content);
            Assert.DoesNotContain("&lt;em&gt;", content);
        }

        [Fact]
        public void Process_WithStatusBadgeStyle_RendersBadgeWithSpecifiedStyle()
        {
            // Arrange
            var steps = new[]
            {
                new StepperStep
                {
                    StepNumber = 1,
                    Label = "Step 1",
                    StatusBadgeLabel = "Complete",
                    StatusBadgeStyle = BadgeStyle.success
                }
            };

            var tagHelper = new FDCPStepperTagHelper
            {
                CurrentStep = 1,
                Steps = steps
            };

            // Act
            tagHelper.Process(_context, _output);

            // Assert
            var content = _output.Content.GetContent();
            Assert.Contains("fdcp-badge-success", content);
            Assert.Contains("<span class='fdcp-badge-content'>Complete</span>", content);
        }

        [Fact]
        public void Process_WithStatusBadgeStyleInvertedTrue_RendersInvertedBadge()
        {
            // Arrange
            var steps = new[]
            {
                new StepperStep
                {
                    StepNumber = 1,
                    Label = "Step 1",
                    StatusBadgeLabel = "In review",
                    StatusBadgeStyle = BadgeStyle.primary,
                    StatusBadgeStyleInverted = true
                }
            };

            var tagHelper = new FDCPStepperTagHelper
            {
                CurrentStep = 1,
                Steps = steps
            };

            // Act
            tagHelper.Process(_context, _output);

            // Assert
            var content = _output.Content.GetContent();
            Assert.Contains("fdcp-badge-primary", content);
            Assert.Contains("inverted", content);
            Assert.Contains("<span class='fdcp-badge-content'>In review</span>", content);
        }
    }
}