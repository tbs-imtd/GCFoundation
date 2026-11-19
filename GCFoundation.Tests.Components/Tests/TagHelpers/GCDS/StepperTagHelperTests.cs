using GCFoundation.Components.Enums;
using GCFoundation.Components.TagHelpers.GCDS;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.GCDS
{
    public class StepperTagHelperTests
    {
        [Fact]
        public void Process_EmitsGcdsStepperAttributes()
        {
            var helper = new StepperTagHelper
            {
                CurrentStep = 2,
                TotalSteps = 5,
                Tag = StepperTag.h3
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal("2", output.Attributes["current-step"].Value?.ToString());
            Assert.Equal("5", output.Attributes["total-steps"].Value?.ToString());
            Assert.Equal("h3", output.Attributes["tag"].Value?.ToString());
            Assert.False(output.Attributes.ContainsName("total-step"));
        }

        [Fact]
        public void Process_LegacyTotalStepAlias_StillMapsToTotalSteps()
        {
            var helper = new StepperTagHelper { CurrentStep = 1 };
#pragma warning disable CS0618 // Type or member is obsolete
            helper.TotalStep = 4;
#pragma warning restore CS0618 // Type or member is obsolete

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal("4", output.Attributes["total-steps"].Value?.ToString());
        }

        private static TagHelperContext CreateContext() =>
            new(new TagHelperAttributeList(), new Dictionary<object, object>(), "test-id");

        private static TagHelperOutput CreateOutput() =>
            new("gcds-stepper", new TagHelperAttributeList(),
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
    }
}
