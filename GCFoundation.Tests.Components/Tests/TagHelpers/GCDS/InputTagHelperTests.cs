using GCFoundation.Components.TagHelpers.GCDS;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel.DataAnnotations;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.GCDS
{
    public class InputTagHelperTests
    {
        [Fact]
        public void Process_WithFor_ResolvesMetadataAndCommonAttributes()
        {
            var helper = new InputTagHelper
            {
                For = CreateModelExpression(nameof(TestModel.Email), new TestModel { Email = "test@example.com" }),
                ViewContext = new ViewContext()
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal("Email address", output.Attributes["label"].Value?.ToString());
            Assert.Equal(nameof(TestModel.Email), output.Attributes["input-id"].Value?.ToString());
            Assert.Equal(nameof(TestModel.Email), output.Attributes["name"].Value?.ToString());
            Assert.Equal("Used for notifications.", output.Attributes["hint"].Value?.ToString());
            Assert.Equal("test@example.com", output.Attributes["value"].Value?.ToString());
            Assert.True(output.Attributes.ContainsName("required"));
        }

        [Fact]
        public void Process_WithExplicitOverrides_UsesOverridesInsteadOfMetadata()
        {
            var helper = new InputTagHelper
            {
                For = CreateModelExpression(nameof(TestModel.Email), new TestModel { Email = "test@example.com" }),
                ViewContext = new ViewContext(),
                InputId = "custom-email",
                Label = "Custom email",
                Name = "CustomName",
                Hint = "Custom hint",
                Value = "override@example.com",
                Required = false
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal("Custom email", output.Attributes["label"].Value?.ToString());
            Assert.Equal("custom-email", output.Attributes["input-id"].Value?.ToString());
            Assert.Equal("CustomName", output.Attributes["name"].Value?.ToString());
            Assert.Equal("Custom hint", output.Attributes["hint"].Value?.ToString());
            Assert.Equal("override@example.com", output.Attributes["value"].Value?.ToString());
            Assert.False(output.Attributes.ContainsName("required"));
        }

        [Fact]
        public void Process_WithMinMaxAndLengthAttributes_EmitsThem()
        {
            var helper = new InputTagHelper
            {
                For = CreateModelExpression(nameof(TestModel.Email), new TestModel()),
                ViewContext = new ViewContext(),
                Min = "1",
                Max = "10",
                MinLength = 2,
                MaxLength = 50
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal("1", output.Attributes["min"].Value?.ToString());
            Assert.Equal("10", output.Attributes["max"].Value?.ToString());
            Assert.Equal("2", output.Attributes["minlength"].Value?.ToString());
            Assert.Equal("50", output.Attributes["maxlength"].Value?.ToString());
        }

        [Fact]
        public void Process_WithoutMinMaxAndLengthAttributes_OmitsThem()
        {
            var helper = new InputTagHelper
            {
                For = CreateModelExpression(nameof(TestModel.Email), new TestModel()),
                ViewContext = new ViewContext()
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.False(output.Attributes.ContainsName("min"));
            Assert.False(output.Attributes.ContainsName("max"));
            Assert.False(output.Attributes.ContainsName("minlength"));
            Assert.False(output.Attributes.ContainsName("maxlength"));
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
            new("gcds-input", new TagHelperAttributeList(),
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        private sealed class TestModel
        {
            [Required]
            [Display(Name = "Email address", Description = "Used for notifications.")]
            public string Email { get; set; } = string.Empty;
        }
    }
}
