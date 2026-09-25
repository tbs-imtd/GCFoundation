using GCFoundation.Components.Enums;
using GCFoundation.Components.TagHelpers.GCDS;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel.DataAnnotations;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.GCDS
{
    public class DateInputTagHelperTests
    {
        [Fact]
        public void Process_EmitsDateInputAndCommonFormAttributes()
        {
            var helper = new DateInputTagHelper
            {
                Format = DateInputFormatType.full,
                Legend = "Date of birth",
                Name = "DateOfBirth",
                Hint = "Use the format shown.",
                Required = true,
                Value = "2026-06-01"
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal("full", output.Attributes["format"].Value?.ToString());
            Assert.Equal("Date of birth", output.Attributes["legend"].Value?.ToString());
            Assert.Equal("DateOfBirth", output.Attributes["name"].Value?.ToString());
            Assert.Equal("Use the format shown.", output.Attributes["hint"].Value?.ToString());
            Assert.Equal("2026-06-01", output.Attributes["value"].Value?.ToString());
            Assert.True(output.Attributes.ContainsName("required"));
        }

        [Fact]
        public void Process_WithRangeDataAnnotation_EmitsMinAndMaxAttributes()
        {
            var helper = new DateInputTagHelper
            {
                For = CreateModelExpression(nameof(TestModel.EventDate), new TestModel()),
                ViewContext = new ViewContext(),
                Format = DateInputFormatType.full,
                Legend = "Event date"
            };

            var output = CreateOutput();
            helper.Process(CreateContext(), output);

            Assert.Equal("2000-01-01", output.Attributes["min"].Value?.ToString());
            Assert.Equal("2030-12-31", output.Attributes["max"].Value?.ToString());
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
            new("gcds-date-input", new TagHelperAttributeList(),
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        private sealed class TestModel
        {
            [Range(typeof(DateTime), "2000-01-01", "2030-12-31")]
            public DateTime EventDate { get; set; }
        }
    }
}
