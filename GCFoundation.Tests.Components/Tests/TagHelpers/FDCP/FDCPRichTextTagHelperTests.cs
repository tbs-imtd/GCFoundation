using GCFoundation.Components.TagHelpers.FDCP;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel.DataAnnotations;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.FDCP
{
    public class FDCPRichTextTagHelperTests
    {
        [Fact]
        public void Process_WithDisplayPrompt_EmitsPlaceholderWhenPropertyIsEmpty()
        {
            var helper = new FDCPRichTextTagHelper
            {
                For = CreateModelExpression(nameof(TestModel.Bio), new TestModel()),
                ViewContext = new ViewContext()
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Contains("data-placeholder=\"Enter a biography\"", output.Content.GetContent(), StringComparison.Ordinal);
        }

        [Fact]
        public void Process_WithExplicitPlaceholder_DoesNotUseDisplayPrompt()
        {
            var helper = new FDCPRichTextTagHelper
            {
                For = CreateModelExpression(nameof(TestModel.Bio), new TestModel()),
                ViewContext = new ViewContext(),
                Placeholder = "Custom prompt"
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            string content = output.Content.GetContent();
            Assert.Contains("data-placeholder=\"Custom prompt\"", content, StringComparison.Ordinal);
            Assert.DoesNotContain("Enter a biography", content, StringComparison.Ordinal);
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
            new("fdcp-rich-text", new TagHelperAttributeList(),
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        private sealed class TestModel
        {
            [Display(Prompt = "Enter a biography")]
            public string Bio { get; set; } = string.Empty;
        }
    }
}
