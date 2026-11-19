using GCFoundation.Components.Enums;
using GCFoundation.Components.TagHelpers.GCDS;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.GCDS
{
    public class NoticeTagHelperTests
    {
        [Theory]
        [InlineData(AlertType.info, "info")]
        [InlineData(AlertType.warning, "warning")]
        [InlineData(AlertType.danger, "danger")]
        [InlineData(AlertType.success, "success")]
        public void Process_SetsNoticeRoleFromAlertType(AlertType role, string expectedRole)
        {
            var helper = new NoticeTagHelper
            {
                NoticeTitle = "Test title",
                NoticeRole = role,
                NoticeTitleTag = HeadingTag.h3
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal("gcds-notice", output.TagName);
            Assert.Equal("Test title", output.Attributes["notice-title"].Value?.ToString());
            Assert.Equal("h3", output.Attributes["notice-title-tag"].Value?.ToString());
            Assert.Equal(expectedRole, output.Attributes["notice-role"].Value?.ToString());
        }

        [Fact]
        public void Process_DoesNotEmitLegacyAttributes()
        {
            var helper = new NoticeTagHelper
            {
                NoticeTitle = "T",
                NoticeRole = AlertType.warning
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.False(output.Attributes.ContainsName("type"));
            Assert.False(output.Attributes.ContainsName("title"));
            Assert.False(output.Attributes.ContainsName("title-tag"));
        }

        [Fact]
        public void Process_LegacyAliases_StillMapToGcdsAttributes()
        {
            var helper = new NoticeTagHelper { NoticeTitle = string.Empty };
#pragma warning disable CS0618 // Type or member is obsolete
            helper.Title = "Legacy";
            helper.TitleTag = HeadingTag.h4;
            helper.Type = AlertType.danger;
#pragma warning restore CS0618 // Type or member is obsolete

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal("Legacy", output.Attributes["notice-title"].Value?.ToString());
            Assert.Equal("h4", output.Attributes["notice-title-tag"].Value?.ToString());
            Assert.Equal("danger", output.Attributes["notice-role"].Value?.ToString());
        }

        private static TagHelperContext CreateContext() =>
            new(new TagHelperAttributeList(), new Dictionary<object, object>(), "test-id");

        private static TagHelperOutput CreateOutput() =>
            new("gcds-notice", new TagHelperAttributeList(),
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
    }
}
