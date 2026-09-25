using GCFoundation.Components.TagHelpers.GCDS;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel;
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
        public void Process_WithDataAnnotations_EmitsConstraintAttributes()
        {
            var helper = new InputTagHelper
            {
                For = CreateModelExpression(nameof(TestModel.Username), new TestModel()),
                ViewContext = new ViewContext()
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal("25", output.Attributes["maxlength"].Value?.ToString());
            Assert.Equal("3", output.Attributes["minlength"].Value?.ToString());
            Assert.Equal(@"^[A-Za-z]+$", output.Attributes["pattern"].Value?.ToString());

            var rangeHelper = new InputTagHelper
            {
                For = CreateModelExpression(nameof(TestModel.Quantity), new TestModel()),
                ViewContext = new ViewContext()
            };

            var rangeOutput = CreateOutput();
            rangeHelper.Process(CreateContext(), rangeOutput);

            Assert.Equal("1", rangeOutput.Attributes["min"].Value?.ToString());
            Assert.Equal("10", rangeOutput.Attributes["max"].Value?.ToString());
        }

        [Fact]
        public void Process_WithDataType_EmitsMatchingInputType()
        {
            AssertInputType(nameof(TestModel.EmailAddress), "email");
            AssertInputType(nameof(TestModel.Password), "password");
            AssertInputType(nameof(TestModel.Phone), "tel");
            AssertInputType(nameof(TestModel.Website), "url");
        }

        [Fact]
        public void Process_WithExplicitType_DoesNotOverwriteWithDataType()
        {
            var helper = new InputTagHelper
            {
                For = CreateModelExpression(nameof(TestModel.EmailAddress), new TestModel()),
                ViewContext = new ViewContext()
            };

            var output = CreateOutput();
            output.Attributes.SetAttribute("type", "text");
            helper.Process(CreateContext(), output);

            Assert.Equal("text", output.Attributes["type"].Value?.ToString());
        }

        [Fact]
        public void Process_WithReadOnlyDataAnnotation_EmitsReadonlyAttribute()
        {
            var helper = new InputTagHelper
            {
                For = CreateModelExpression(nameof(TestModel.LockedCode), new TestModel()),
                ViewContext = new ViewContext()
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.True(output.Attributes.ContainsName("readonly"));
        }

        private void AssertInputType(string propertyName, string expectedType)
        {
            var helper = new InputTagHelper
            {
                For = CreateModelExpression(propertyName, new TestModel()),
                ViewContext = new ViewContext()
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal(expectedType, output.Attributes["type"].Value?.ToString());
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

            [MaxLength(25)]
            [MinLength(3)]
            [RegularExpression(@"^[A-Za-z]+$")]
            public string Username { get; set; } = string.Empty;

            [Range(1, 10)]
            public int Quantity { get; set; }

            [DataType(DataType.EmailAddress)]
            public string EmailAddress { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.PhoneNumber)]
            public string Phone { get; set; } = string.Empty;

            [DataType(DataType.Url)]
            public string Website { get; set; } = string.Empty;

            [ReadOnly(true)]
            public string LockedCode { get; set; } = string.Empty;
        }
    }
}
