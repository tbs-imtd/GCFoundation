using GCFoundation.Components.TagHelpers.GCDS;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.GCDS
{
    public class TextareaTagHelperTests
    {
        [Fact]
        public void Process_WithMaxLength_EmitsMaxlengthAttribute()
        {
            var helper = new TextareaTagHelper
            {
                Name = "comments",
                Label = "Comments",
                TextareaId = "comments",
                MaxLength = 400
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal("400", output.Attributes["maxlength"].Value?.ToString());
        }

        [Fact]
        public void Process_WithMaxLengthZero_OmitsMaxlengthAttribute()
        {
            var helper = new TextareaTagHelper
            {
                Name = "c",
                Label = "L",
                TextareaId = "c",
                MaxLength = 0
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.False(output.Attributes.ContainsName("maxlength"));
        }

        [Fact]
        public void Process_WithHideLimit_EmitsHideLimitAttribute()
        {
            var helper = new TextareaTagHelper
            {
                Name = "c",
                Label = "L",
                TextareaId = "c",
                MaxLength = 100,
                HideLimit = true
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal("100", output.Attributes["maxlength"].Value?.ToString());
            Assert.Equal("true", output.Attributes["hide-limit"].Value?.ToString());
        }

        [Fact]
        public void Process_WithMinLength_EmitsMinlengthAttribute()
        {
            var helper = new TextareaTagHelper
            {
                Name = "comments",
                Label = "Comments",
                TextareaId = "comments",
                MinLength = 10
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal("10", output.Attributes["minlength"].Value?.ToString());
        }

        [Fact]
        public void Process_WithMinLengthZero_OmitsMinlengthAttribute()
        {
            var helper = new TextareaTagHelper
            {
                Name = "c",
                Label = "L",
                TextareaId = "c",
                MinLength = 0
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.False(output.Attributes.ContainsName("minlength"));
        }

        private static TagHelperContext CreateContext() =>
            new(new TagHelperAttributeList(), new Dictionary<object, object>(), "test-id");

        private static TagHelperOutput CreateOutput() =>
            new("gcds-textarea", new TagHelperAttributeList(),
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
    }
}
