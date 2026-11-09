using GCFoundation.Components.TagHelpers.FDCP;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel.DataAnnotations;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.FDCP
{
    public class FDCPHorizontalRadiosTagHelperTests
    {
        private readonly FDCPHorizontalRadiosTagHelper _tagHelper = new();
        private readonly TagHelperContext _context;
        private readonly TagHelperOutput _output;

        public FDCPHorizontalRadiosTagHelperTests()
        {
            _context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "horizontal-radios-test");
            _output = new TagHelperOutput(
                "fdcp-horizontal-radios",
                new TagHelperAttributeList(),
                (_, _) => Task.FromResult<TagHelperContent>(
                    new DefaultTagHelperContent()));
            _tagHelper.ViewContext = new ViewContext();
        }

        [Fact]
        public void Process_WithBoundRequiredModel_RendersSelectedRadioAndMetadata()
        {
            SetupModelExpression(
                nameof(TestModel.RequiredChoice),
                new TestModel { RequiredChoice = "guidance" });
            _tagHelper.Items =
            [
                new() { Text = "Learning", Value = "learning" },
                new() { Text = "Guidance", Value = "guidance" }
            ];

            _tagHelper.Process(_context, _output);

            string content = _output.Content.GetContent();
            Assert.Equal("fieldset", _output.TagName);
            Assert.Equal("RequiredChoice", _output.Attributes["id"].Value);
            Assert.True(_output.Attributes.ContainsName("data-fdcp-horizontal-radios"));
            Assert.Contains("Required choice", content, StringComparison.Ordinal);
            Assert.Contains("name=\"RequiredChoice\"", content, StringComparison.Ordinal);
            Assert.Contains("value=\"guidance\" required checked", content, StringComparison.Ordinal);
            Assert.Contains("fdcp-horizontal-radios__grid d-grid", content, StringComparison.Ordinal);
            Assert.Contains("fdcp-horizontal-radios__card", content, StringComparison.Ordinal);
            Assert.Equal(
                "fdcp-horizontal-radios",
                _output.Attributes["class"].Value.ToString());
        }

        [Fact]
        public void Process_RendersOuterContainerAndOptionCards()
        {
            _tagHelper.Name = "skill";
            _tagHelper.Legend = "Skill level";
            _tagHelper.Items = [new() { Text = "Learning", Value = "learning" }];
            _output.Attributes.SetAttribute("class", "mt-400");

            _tagHelper.Process(_context, _output);

            string content = _output.Content.GetContent();
            Assert.Contains(
                "<gcds-container class=\"fdcp-horizontal-radios__outer mt-400\"",
                _output.PreElement.GetContent(),
                StringComparison.Ordinal);
            Assert.DoesNotContain(" border", _output.PreElement.GetContent(), StringComparison.Ordinal);
            Assert.Contains("padding=\"300\"", _output.PreElement.GetContent(), StringComparison.Ordinal);
            Assert.Equal("</gcds-container>", _output.PostElement.GetContent());
            Assert.Contains("fdcp-card fdcp-horizontal-radios__card", content, StringComparison.Ordinal);
            Assert.Contains("fdcp-card-body", content, StringComparison.Ordinal);
            Assert.Contains(
                "<span class=\"fdcp-horizontal-radios__label-text\">Learning</span>",
                content,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                "mt-400",
                _output.Attributes["class"].Value.ToString(),
                StringComparison.Ordinal);
        }

        [Fact]
        public void Process_WithStandaloneAttributes_RendersExpectedOptions()
        {
            _tagHelper.Name = "skill";
            _tagHelper.Id = "skill-level";
            _tagHelper.Legend = "Skill level";
            _tagHelper.Hint = "Choose one";
            _tagHelper.Value = "learning";
            _tagHelper.Required = true;
            _tagHelper.Items =
            [
                new() { Text = "Learning", Value = "learning" },
                new() { Text = "Guidance", Value = "guidance", Disabled = true }
            ];

            _tagHelper.Process(_context, _output);

            string content = _output.Content.GetContent();
            Assert.Equal("skill-level", _output.Attributes["id"].Value);
            Assert.Contains("Choose one", content, StringComparison.Ordinal);
            Assert.Contains("value=\"learning\" required checked", content, StringComparison.Ordinal);
            Assert.Contains("value=\"guidance\" required disabled", content, StringComparison.Ordinal);
        }

        [Fact]
        public void Process_WithDisplayedIndex_RendersEncodedIndexAboveLegend()
        {
            _tagHelper.Name = "skill";
            _tagHelper.Legend = "Skill level";
            _tagHelper.Index = "Question <1> of 7";
            _tagHelper.DisplayIndex = true;
            _tagHelper.Items = [new() { Text = "Learning", Value = "learning" }];

            _tagHelper.Process(_context, _output);

            string content = _output.Content.GetContent();
            Assert.Contains(
                "<span class=\"fdcp-horizontal-radios__index\">Question &lt;1&gt; of 7</span>",
                content,
                StringComparison.Ordinal);
            Assert.True(
                content.IndexOf("fdcp-horizontal-radios__index", StringComparison.Ordinal) <
                content.IndexOf("Skill level", StringComparison.Ordinal));
        }

        [Fact]
        public void Process_WithHiddenIndex_DoesNotRenderIndex()
        {
            _tagHelper.Name = "skill";
            _tagHelper.Legend = "Skill level";
            _tagHelper.Index = "Question 1 of 7";
            _tagHelper.DisplayIndex = false;
            _tagHelper.Items = [new() { Text = "Learning", Value = "learning" }];

            _tagHelper.Process(_context, _output);

            Assert.DoesNotContain(
                "fdcp-horizontal-radios__index",
                _output.Content.GetContent(),
                StringComparison.Ordinal);
        }

        [Fact]
        public void Process_WithoutRequired_DoesNotRenderRequiredInputs()
        {
            _tagHelper.Name = "skill";
            _tagHelper.Legend = "Skill level";
            _tagHelper.Items = [new() { Text = "Learning", Value = "learning" }];

            _tagHelper.Process(_context, _output);

            Assert.DoesNotContain(" required", _output.Content.GetContent(), StringComparison.Ordinal);
        }

        [Fact]
        public void Process_WhenDisabled_DoesNotRenderRequiredState()
        {
            _tagHelper.Name = "skill";
            _tagHelper.Legend = "Skill level";
            _tagHelper.Required = true;
            _tagHelper.Disabled = true;
            _tagHelper.Items = [new() { Text = "Learning", Value = "learning" }];

            _tagHelper.Process(_context, _output);

            string content = _output.Content.GetContent();
            Assert.True(_output.Attributes.ContainsName("disabled"));
            Assert.DoesNotContain("fdcp-horizontal-radios__required", content, StringComparison.Ordinal);
            Assert.DoesNotContain("fdcp-horizontal-radios__error", content, StringComparison.Ordinal);
            Assert.DoesNotContain(" required", content, StringComparison.Ordinal);
        }

        [Fact]
        public void Process_EncodesCallerControlledContent()
        {
            _tagHelper.Name = "skill";
            _tagHelper.Legend = "<script>legend</script>";
            _tagHelper.Hint = "<img src=x onerror=alert(1)>";
            _tagHelper.Items =
            [
                new() { Text = "<strong>option</strong>", Value = "\"bad\"" }
            ];

            _tagHelper.Process(_context, _output);

            string content = _output.Content.GetContent();
            Assert.DoesNotContain("<script>", content, StringComparison.Ordinal);
            Assert.DoesNotContain("<img", content, StringComparison.Ordinal);
            Assert.Contains("&lt;strong&gt;option&lt;/strong&gt;", content, StringComparison.Ordinal);
            Assert.Contains("value=\"&quot;bad&quot;\"", content, StringComparison.Ordinal);
        }

        [Fact]
        public void Process_WithoutForOrName_ThrowsInvalidOperationException()
        {
            _tagHelper.Items = [];

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => _tagHelper.Process(_context, _output));

            Assert.Contains("Either 'for' or 'name' must be specified", exception.Message, StringComparison.Ordinal);
        }

        private void SetupModelExpression(string propertyName, TestModel model)
        {
            var metadataProvider = new EmptyModelMetadataProvider();
            var modelExplorer = metadataProvider.GetModelExplorerForType(
                typeof(TestModel),
                model);
            var propertyExplorer = modelExplorer.GetExplorerForProperty(propertyName);
            _tagHelper.For = new ModelExpression(propertyName, propertyExplorer);
        }

        private sealed class TestModel
        {
            [Required]
            [Display(Name = "Required choice")]
            public string RequiredChoice { get; set; } = string.Empty;
        }
    }
}
