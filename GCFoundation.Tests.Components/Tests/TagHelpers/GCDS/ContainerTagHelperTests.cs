using GCFoundation.Components.Enums;
using GCFoundation.Components.TagHelpers.GCDS;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.GCDS
{
    public class ContainerTagHelperTests
    {
        [Theory]
        [InlineData(HorizontalAlignment.start, "start")]
        [InlineData(HorizontalAlignment.center, "center")]
        [InlineData(HorizontalAlignment.end, "end")]
        public void Process_EmitsAlignment(HorizontalAlignment alignment, string expected)
        {
            var helper = new ContainerTagHelper { Alignment = alignment };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal(expected, output.Attributes["alignment"].Value?.ToString());
        }

        [Theory]
        [InlineData(ContainerLayout.full, "full")]
        [InlineData(ContainerLayout.page, "page")]
        public void Process_EmitsLayout(ContainerLayout layout, string expected)
        {
            var helper = new ContainerTagHelper { Layout = layout };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal(expected, output.Attributes["layout"].Value?.ToString());
        }

        [Fact]
        public void Process_PageLayoutWithCenter_EmitsAlignmentAndLayout()
        {
            var helper = new ContainerTagHelper
            {
                Alignment = HorizontalAlignment.center,
                Layout = ContainerLayout.page,
                Tag = "main",
                Size = SizeTypeEmum.lg
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal("center", output.Attributes["alignment"].Value?.ToString());
            Assert.Equal("page", output.Attributes["layout"].Value?.ToString());
            Assert.Equal("main", output.Attributes["tag"].Value?.ToString());
            Assert.Equal("lg", output.Attributes["size"].Value?.ToString());
        }

        [Fact]
        public void Process_WithoutAlignmentOrLayout_DoesNotEmitThem()
        {
            var helper = new ContainerTagHelper();

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.False(output.Attributes.ContainsName("alignment"));
            Assert.False(output.Attributes.ContainsName("layout"));
        }

        [Fact]
        public void Process_LegacyAliases_MapToAlignmentAndLayout()
        {
            var helper = new ContainerTagHelper();
#pragma warning disable CS0618 // Type or member is obsolete
            helper.Centered = true;
            helper.MainContainer = true;
#pragma warning restore CS0618 // Type or member is obsolete

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal("center", output.Attributes["alignment"].Value?.ToString());
            Assert.Equal("page", output.Attributes["layout"].Value?.ToString());
            Assert.False(output.Attributes.ContainsName("centered"));
            Assert.False(output.Attributes.ContainsName("main-container"));
        }

        private static TagHelperContext CreateContext() =>
            new(new TagHelperAttributeList(), new Dictionary<object, object>(), "test-id");

        private static TagHelperOutput CreateOutput() =>
            new("gcds-container", new TagHelperAttributeList(),
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
    }
}
