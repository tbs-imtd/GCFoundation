using GCFoundation.Components.Enums;
using GCFoundation.Components.TagHelpers.GCDS;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.GCDS
{
    public class TextTagHelperTests
    {
        [Theory]
        [InlineData(TextDisplay.Block, "block")]
        [InlineData(TextDisplay.InlineBlock, "inline-block")]
        [InlineData(TextDisplay.InlineFlex, "inline-flex")]
        public void Process_EmitsKebabCaseDisplay(TextDisplay display, string expected)
        {
            var helper = new TextTagHelper { Display = display };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal(expected, output.Attributes["display"].Value?.ToString());
        }

        private static TagHelperContext CreateContext() =>
            new(new TagHelperAttributeList(), new Dictionary<object, object>(), "test-id");

        private static TagHelperOutput CreateOutput() =>
            new("gcds-text", new TagHelperAttributeList(),
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
    }
}
