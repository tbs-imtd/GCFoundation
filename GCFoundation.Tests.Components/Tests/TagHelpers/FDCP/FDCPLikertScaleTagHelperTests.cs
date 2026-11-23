using GCFoundation.Components.TagHelpers.FDCP;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.FDCP
{
    public class FDCPLikertScaleTagHelperTests
    {
        private readonly FDCPLikertScaleTagHelper _tagHelper = new();
        private readonly TagHelperContext _context;
        private readonly TagHelperOutput _output;

        public FDCPLikertScaleTagHelperTests()
        {
            _context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "likert-scale-test");
            _output = new TagHelperOutput(
                "fdcp-likert-scale",
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
            Assert.True(_output.Attributes.ContainsName("data-fdcp-likert-scale"));
            Assert.Contains("Required choice", content, StringComparison.Ordinal);
            Assert.Contains("name=\"RequiredChoice\"", content, StringComparison.Ordinal);
            string guidanceInput = GetInputTag(content, "guidance");
            Assert.True(HasAttribute(guidanceInput, "required"));
            Assert.True(HasAttribute(guidanceInput, "checked"));
            Assert.Contains("<div class=\"fdcp-likert-scale__grid\">", content, StringComparison.Ordinal);
            Assert.Contains("fdcp-likert-scale__card", content, StringComparison.Ordinal);
            Assert.Contains("Choose an option to continue.", content, StringComparison.Ordinal);
            Assert.Equal(
                "Choose an option to continue.",
                _output.Attributes["data-required-message"].Value);
            Assert.Equal(
                "fdcp-likert-scale",
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
                "<gcds-container class=\"fdcp-likert-scale__outer mt-400\"",
                _output.PreElement.GetContent(),
                StringComparison.Ordinal);
            Assert.DoesNotContain(" border", _output.PreElement.GetContent(), StringComparison.Ordinal);
            Assert.Contains("padding=\"300\"", _output.PreElement.GetContent(), StringComparison.Ordinal);
            Assert.Equal("</gcds-container>", _output.PostElement.GetContent());
            Assert.Contains("fdcp-likert-scale__card", content, StringComparison.Ordinal);
            Assert.DoesNotContain("fdcp-card ", content, StringComparison.Ordinal);
            Assert.DoesNotContain("fdcp-card-body", content, StringComparison.Ordinal);
            Assert.Contains(
                "<span class=\"fdcp-likert-scale__label-text\">Learning</span>",
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
            string learningInput = GetInputTag(content, "learning");
            Assert.True(HasAttribute(learningInput, "required"));
            Assert.True(HasAttribute(learningInput, "checked"));
            string guidanceInput = GetInputTag(content, "guidance");
            Assert.True(HasAttribute(guidanceInput, "required"));
            Assert.True(HasAttribute(guidanceInput, "disabled"));
            Assert.False(HasAttribute(guidanceInput, "checked"));
            Assert.Equal(
                "skill-level_legend skill-level_hint",
                _output.Attributes["aria-labelledby"].Value);
        }

        [Fact]
        public void Process_WithModelStateError_RendersVisibleErrorOnFieldsetAndInputs()
        {
            var viewContext = new ViewContext();
            viewContext.ModelState.AddModelError("skill", "Select your skill level.");
            _tagHelper.ViewContext = viewContext;
            _tagHelper.Name = "skill";
            _tagHelper.Legend = "Skill level";
            _tagHelper.Items =
            [
                new() { Text = "Learning", Value = "learning" },
                new() { Text = "Guidance", Value = "guidance" }
            ];

            _tagHelper.Process(_context, _output);

            string content = _output.Content.GetContent();
            Assert.Equal(
                "fdcp-likert-scale fdcp-likert-scale--error",
                _output.Attributes["class"].Value.ToString());
            string errorTag = Regex.Match(content, "<gcds-error-message[^>]*>").Value;
            Assert.False(string.IsNullOrEmpty(errorTag));
            Assert.False(HasAttribute(errorTag, "hidden"));
            Assert.Contains("Select your skill level.", content, StringComparison.Ordinal);
            Assert.False(_output.Attributes.ContainsName("aria-describedby"));

            foreach (string value in new[] { "learning", "guidance" })
            {
                string input = GetInputTag(content, value);
                Assert.Contains("aria-invalid=\"true\"", input, StringComparison.Ordinal);
                Assert.Contains("aria-description=\"Select your skill level.\"", input, StringComparison.Ordinal);
                Assert.False(HasAttribute(input, "title"));
            }
        }

        [Fact]
        public void Process_WithoutModelStateError_DoesNotMarkInputsInvalid()
        {
            _tagHelper.Name = "skill";
            _tagHelper.Legend = "Skill level";
            _tagHelper.Required = true;
            _tagHelper.Items = [new() { Text = "Learning", Value = "learning" }];

            _tagHelper.Process(_context, _output);

            string input = GetInputTag(_output.Content.GetContent(), "learning");
            Assert.False(HasAttribute(input, "aria-invalid"));
            Assert.False(HasAttribute(input, "aria-description"));
            Assert.Equal("skill_legend", _output.Attributes["aria-labelledby"].Value);
        }

        [Fact]
        public void Process_WithPostedModelStateValue_OverridesValue()
        {
            var viewContext = new ViewContext();
            viewContext.ModelState.SetModelValue("skill", "guidance", "guidance");
            _tagHelper.ViewContext = viewContext;
            _tagHelper.Name = "skill";
            _tagHelper.Legend = "Skill level";
            _tagHelper.Value = "learning";
            _tagHelper.Items =
            [
                new() { Text = "Learning", Value = "learning" },
                new() { Text = "Guidance", Value = "guidance" }
            ];

            _tagHelper.Process(_context, _output);

            string content = _output.Content.GetContent();
            Assert.True(HasAttribute(GetInputTag(content, "guidance"), "checked"));
            Assert.False(HasAttribute(GetInputTag(content, "learning"), "checked"));
        }

        [Fact]
        public void Process_WithIndex_RendersEncodedIndexAboveLegend()
        {
            _tagHelper.Name = "skill";
            _tagHelper.Legend = "Skill level";
            _tagHelper.Index = "Question <1> of 7";
            _tagHelper.Items = [new() { Text = "Learning", Value = "learning" }];

            _tagHelper.Process(_context, _output);

            string content = _output.Content.GetContent();
            Assert.Contains(
                "<span class=\"fdcp-likert-scale__index\">Question &lt;1&gt; of 7</span>",
                content,
                StringComparison.Ordinal);
            Assert.True(
                content.IndexOf("fdcp-likert-scale__index", StringComparison.Ordinal) <
                content.IndexOf("Skill level", StringComparison.Ordinal));
        }

        [Fact]
        public void Process_WithoutIndex_DoesNotRenderIndex()
        {
            _tagHelper.Name = "skill";
            _tagHelper.Legend = "Skill level";
            _tagHelper.Items = [new() { Text = "Learning", Value = "learning" }];

            _tagHelper.Process(_context, _output);

            Assert.DoesNotContain(
                "fdcp-likert-scale__index",
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
            Assert.DoesNotContain("fdcp-likert-scale__required", content, StringComparison.Ordinal);
            Assert.DoesNotContain("fdcp-likert-scale__error", content, StringComparison.Ordinal);
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

        private static string GetInputTag(string content, string value)
        {
            Match match = Regex.Match(
                content,
                $"<input[^>]*value=\"{Regex.Escape(value)}\"[^>]*>",
                RegexOptions.Singleline);
            Assert.True(match.Success, $"No input found for value '{value}'.");
            return match.Value;
        }

        private static bool HasAttribute(string tag, string attributeName)
        {
            return Regex.IsMatch(tag, $@"\s{Regex.Escape(attributeName)}(?=[\s=>])");
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
