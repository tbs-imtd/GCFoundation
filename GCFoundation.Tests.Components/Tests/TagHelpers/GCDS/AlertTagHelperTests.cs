using GCFoundation.Components.Enums;
using GCFoundation.Components.TagHelpers.GCDS;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.GCDS
{
    public class AlertTagHelperTests
    {
        [Fact]
        public void Process_WithConfiguredProperties_EmitsAlertAttributes()
        {
            var helper = new AlertTagHelper
            {
                AlertRole = AlertType.warning,
                Container = ContainerSize.md,
                Heading = "Service interruption",
                HideCloseBtn = true,
                HideRoleIcon = true,
                IsFixed = true
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal("gcds-alert", output.TagName);
            Assert.Equal("warning", output.Attributes["alert-role"].Value?.ToString());
            Assert.Equal("md", output.Attributes["container"].Value?.ToString());
            Assert.Equal("Service interruption", output.Attributes["heading"].Value?.ToString());
            Assert.True(output.Attributes.ContainsName("hide-close-btn"));
            Assert.True(output.Attributes.ContainsName("hide-role-icon"));
            Assert.True(output.Attributes.ContainsName("is-fixed"));
        }

        [Fact]
        public void Process_WithDefaultProperties_OmitsBooleanAttributes()
        {
            var helper = new AlertTagHelper
            {
                Heading = "Information"
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal("info", output.Attributes["alert-role"].Value?.ToString());
            Assert.Equal("full", output.Attributes["container"].Value?.ToString());
            Assert.False(output.Attributes.ContainsName("hide-close-btn"));
            Assert.False(output.Attributes.ContainsName("hide-role-icon"));
            Assert.False(output.Attributes.ContainsName("is-fixed"));
        }

        private static TagHelperContext CreateContext() =>
            new(new TagHelperAttributeList(), new Dictionary<object, object>(), "test-id");

        private static TagHelperOutput CreateOutput() =>
            new("gcds-alert", new TagHelperAttributeList(),
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
    }
}
