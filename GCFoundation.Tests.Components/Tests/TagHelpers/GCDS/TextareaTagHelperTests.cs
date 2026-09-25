using GCFoundation.Components.TagHelpers.GCDS;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel.DataAnnotations;

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
        public void Process_WithMaxLengthDataAnnotation_EmitsMaxlengthWhenPropertyIsZero()
        {
            var helper = new TextareaTagHelper
            {
                For = CreateModelExpression(nameof(TestModel.Bio), new TestModel()),
                ViewContext = new ViewContext(),
                Label = "Biography",
                TextareaId = "bio"
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal("2000", output.Attributes["maxlength"].Value?.ToString());
            Assert.Equal("10", output.Attributes["minlength"].Value?.ToString());
        }

        [Fact]
        public void Process_WithExplicitMaxLength_DoesNotOverwriteWithDataAnnotation()
        {
            var helper = new TextareaTagHelper
            {
                For = CreateModelExpression(nameof(TestModel.Bio), new TestModel()),
                ViewContext = new ViewContext(),
                Label = "Biography",
                TextareaId = "bio",
                MaxLength = 400
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal("400", output.Attributes["maxlength"].Value?.ToString());
            Assert.Equal("10", output.Attributes["minlength"].Value?.ToString());
        }

        private static ModelExpression CreateModelExpression(string propertyName, TestModel model)
        {
            var metadataProvider = new EmptyModelMetadataProvider();
            var modelExplorer = metadataProvider.GetModelExplorerForType(typeof(TestModel), model);
            return new ModelExpression(propertyName, modelExplorer.GetExplorerForProperty(propertyName));
        }

        private static TagHelperContext CreateContext() =>
            new(new TagHelperAttributeList(), new Dictionary<object, object>(), "test-id");

        private static TagHelperOutput CreateOutput() =>
            new("gcds-textarea", new TagHelperAttributeList(),
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        private sealed class TestModel
        {
            [MaxLength(2000)]
            [MinLength(10)]
            public string Bio { get; set; } = string.Empty;
        }
    }
}
