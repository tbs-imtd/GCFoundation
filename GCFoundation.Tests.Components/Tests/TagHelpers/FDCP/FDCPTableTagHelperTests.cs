using GCFoundation.Components.Enums;
using GCFoundation.Components.TagHelpers.FDCP;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using Moq;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.FDCP
{
    public class FDCPTableTagHelperTests
    {
        [Fact]
        public async Task ProcessAsync_DefaultCaptionTag_RendersH5Heading()
        {
            var helper = CreateHelper();
            var output = CreateOutput();

            await helper.ProcessAsync(CreateContext(), output);

            Assert.Contains("<gcds-heading tag=\"h5\">Employee list</gcds-heading>", output.PreContent.GetContent());
        }

        [Fact]
        public async Task ProcessAsync_CustomCaptionTag_RendersSelectedHeading()
        {
            var helper = CreateHelper();
            helper.CaptionTag = HeadingTag.h3;
            var output = CreateOutput();

            await helper.ProcessAsync(CreateContext(), output);

            Assert.Contains("<gcds-heading tag=\"h3\">Employee list</gcds-heading>", output.PreContent.GetContent());
        }

        private static FDCPTableTagHelper CreateHelper()
        {
            return new FDCPTableTagHelper(new Mock<IStringLocalizerFactory>().Object)
            {
                Caption = "Employee list",
                Rows = Array.Empty<object>()
            };
        }

        private static TagHelperContext CreateContext()
        {
            return new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "table-test");
        }

        private static TagHelperOutput CreateOutput()
        {
            return new TagHelperOutput(
                "fdcp-table",
                new TagHelperAttributeList(),
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
        }
    }
}
