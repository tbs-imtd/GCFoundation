using GCFoundation.Components.Enums;
using GCFoundation.Components.TagHelpers.FDCP;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.FDCP
{
    public class FDCPSearchableSelectTagHelperTests
    {
        private readonly TagHelperContext _context;

        public FDCPSearchableSelectTagHelperTests()
        {
            _context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test");
        }

        [Fact]
        public async Task ProcessAsync_WithSingleMode_RendersButtonOptionsAndHiddenInput()
        {
            var output = CreateOutput();
            var tagHelper = new FDCPSearchableSelectTagHelper
            {
                Name = "country",
                Label = "Country",
                Hint = "Start typing to filter countries",
                OneResultText = "1 localized result",
                MultipleResultsText = "{0} localized results",
                Items = new List<SelectListItem>
                {
                    new() { Value = "CA", Text = "Canada" },
                    new() { Value = "US", Text = "United States" }
                }
            };

            await tagHelper.ProcessAsync(_context, output);
            string content = output.Content.GetContent();

            Assert.Equal("div", output.TagName);
            Assert.Contains("fdcp-searchable-select--single", output.Attributes["class"].Value?.ToString());
            Assert.Contains("gcds-select-wrapper", output.Attributes["class"].Value?.ToString());
            Assert.Contains("class=\"fdcp-searchable-select__label gcds-label\"", content);
            Assert.Contains("type=\"hidden\"", content);
            Assert.Contains("data-fdcp-searchable-select-single-input", content);
            Assert.DoesNotContain("id=\"country\"", content);
            Assert.Contains("fdcp-searchable-select__sizer", content);
            Assert.Contains("aria-hidden=\"true\"", content);
            Assert.Contains("fdcp-searchable-select__option-item", content);
            Assert.Contains("role=\"option\"", content);
            Assert.Contains("tabindex=\"-1\"", content);
            Assert.DoesNotContain("tabindex=\"0\"", content);
            Assert.Contains("<gcds-hint", content);
            Assert.Contains("Start typing to filter countries", content);
            Assert.Contains("aria-haspopup=\"listbox\"", content);
            Assert.Contains("role=\"combobox\"", content);
            Assert.Contains("aria-autocomplete=\"list\"", content);
            Assert.Contains("aria-expanded=\"false\"", content);
            Assert.Contains("aria-controls=\"country_options\"", content);
            Assert.Contains("aria-labelledby=\"country_label country_search_label\"", content);
            Assert.Contains("id=\"country_search_label\"", content);
            Assert.Contains("role=\"listbox\"", content);
            Assert.Contains("id=\"country_options\"", content);
            Assert.Contains("id=\"country_status\"", content);
            Assert.Contains("aria-live=\"polite\"", content);
            Assert.Contains("data-fdcp-searchable-select-status", content);
            Assert.Equal("1 localized result", output.Attributes["data-one-result-text"].Value?.ToString());
            Assert.Equal("{0} localized results", output.Attributes["data-multiple-results-text"].Value?.ToString());
            Assert.Contains("<span class=\"fdcp-searchable-select__trigger-icon\" aria-hidden=\"true\"></span>", content);
            Assert.DoesNotContain("<svg", content);
            Assert.DoesNotContain("type=\"radio\"", content);
            Assert.DoesNotContain("type=\"checkbox\"", content);
            Assert.Contains("data-fdcp-searchable-select-search", content);
            Assert.Contains("Country", content);
        }

        [Fact]
        public async Task ProcessAsync_WithMultipleMode_RendersCheckboxOptionsAndGroups()
        {
            var output = CreateOutput();
            var economicsGroup = new SelectListGroup { Name = "Economics and Social Science Services (EC)" };
            var tagHelper = new FDCPSearchableSelectTagHelper
            {
                Name = "classificationLevels",
                Label = "Classification group & level",
                SelectionMode = FDCPSearchableSelectSelectionMode.Multiple,
                Items = new List<SelectListItem>
                {
                    new() { Value = "EC-02", Text = "EC-02", Group = economicsGroup },
                    new() { Value = "EC-03", Text = "EC-03", Group = economicsGroup }
                }
            };

            await tagHelper.ProcessAsync(_context, output);
            string content = output.Content.GetContent();

            Assert.Contains("fdcp-searchable-select--multiple", output.Attributes["class"].Value?.ToString());
            Assert.Contains("gcds-select-wrapper", output.Attributes["class"].Value?.ToString());
            Assert.Contains("type=\"checkbox\"", content);
            Assert.Contains("Economics and Social Science Services (EC)", content);
            Assert.Contains("role=\"group\"", content);
            Assert.DoesNotContain("aria-multiselectable", content);
            Assert.DoesNotContain("aria-haspopup=\"listbox\"", content);
            Assert.DoesNotContain("role=\"combobox\"", content);
        }

        [Fact]
        public async Task ProcessAsync_WithSelectedValues_RendersCheckedInputsAndSelectedSummary()
        {
            var output = CreateOutput();
            var tagHelper = new FDCPSearchableSelectTagHelper
            {
                Name = "classificationLevels",
                Label = "Classification group & level",
                Value = "EC-02,EC-04",
                MultipleSelectedText = "option(s) selected",
                SelectionMode = FDCPSearchableSelectSelectionMode.Multiple,
                Items = new List<SelectListItem>
                {
                    new() { Value = "EC-02", Text = "EC-02" },
                    new() { Value = "EC-03", Text = "EC-03" },
                    new() { Value = "EC-04", Text = "EC-04" }
                }
            };

            await tagHelper.ProcessAsync(_context, output);
            string content = output.Content.GetContent();

            Assert.Contains("value=\"EC-02\"", content);
            Assert.Contains("value=\"EC-04\"", content);
            Assert.Equal(2, CountOccurrences(content, " checked"));
            Assert.Contains("2 option(s) selected", content);
            Assert.Equal("option(s) selected", output.Attributes["data-multiple-selected-text"].Value?.ToString());
        }

        [Fact]
        public async Task ProcessAsync_WithForBinding_UsesModelSelectedValues()
        {
            var output = CreateOutput();
            var tagHelper = new FDCPSearchableSelectTagHelper
            {
                For = MockModelExpression("SelectedLevels", new List<string> { "EC-03" }),
                SelectionMode = FDCPSearchableSelectSelectionMode.Multiple,
                Items = new List<SelectListItem>
                {
                    new() { Value = "EC-02", Text = "EC-02" },
                    new() { Value = "EC-03", Text = "EC-03" }
                }
            };

            await tagHelper.ProcessAsync(_context, output);
            string content = output.Content.GetContent();

            Assert.Contains("name=\"SelectedLevels\"", content);
            Assert.Contains("value=\"EC-03\"", content);
            Assert.Contains(" checked", content);
        }

        [Fact]
        public async Task ProcessAsync_WithSelectedOptionsSlot_RendersFooter()
        {
            var output = CreateOutput("<div slot=\"selected-options\"><span data-fdcp-searchable-select-selected-count>0</span> selected</div>");
            var tagHelper = new FDCPSearchableSelectTagHelper
            {
                Name = "country",
                Label = "Country",
                Items = new List<SelectListItem>
                {
                    new() { Value = "CA", Text = "Canada" }
                }
            };

            await tagHelper.ProcessAsync(_context, output);
            string content = output.Content.GetContent();

            Assert.Contains("fdcp-searchable-select__footer", content);
            Assert.Contains("aria-live=\"polite\"", content);
            Assert.Contains("data-fdcp-searchable-select-selected-count", content);
        }

        [Fact]
        public async Task ProcessAsync_WithoutSlot_DoesNotRenderFooter()
        {
            var output = CreateOutput();
            var tagHelper = new FDCPSearchableSelectTagHelper
            {
                Name = "country",
                Label = "Country",
                Items = new List<SelectListItem>
                {
                    new() { Value = "CA", Text = "Canada" }
                }
            };

            await tagHelper.ProcessAsync(_context, output);

            Assert.DoesNotContain("fdcp-searchable-select__footer", output.Content.GetContent());
        }

        [Fact]
        public async Task ProcessAsync_WithRequiredSingleMode_RendersRequiredValidationAttributes()
        {
            var output = CreateOutput();
            var tagHelper = new FDCPSearchableSelectTagHelper
            {
                Name = "country",
                Label = "Country",
                Required = true,
                Items = new List<SelectListItem>
                {
                    new() { Value = "CA", Text = "Canada" }
                }
            };

            await tagHelper.ProcessAsync(_context, output);
            string content = output.Content.GetContent();

            Assert.Equal("true", output.Attributes["data-required"].Value?.ToString());
            Assert.Equal("This field is required.", output.Attributes["data-required-message"].Value?.ToString());
            Assert.Equal("Country: This field is required.", output.Attributes["data-required-summary-message"].Value?.ToString());
            Assert.Contains("label--required", content);
            Assert.Contains("aria-required=\"true\"", content);
            Assert.Contains("aria-describedby=\"country_error\"", content);
            Assert.Contains("<gcds-error-message id=\"country_error\" message-id=\"country_error\"", content);
            Assert.Contains("data-fdcp-searchable-select-error hidden", content);
            Assert.Contains("This field is required.", content);
            Assert.True(
                content.IndexOf("<gcds-error-message", StringComparison.Ordinal) <
                content.IndexOf("<button", StringComparison.Ordinal));
        }

        [Fact]
        public async Task ProcessAsync_WithRequiredMultipleMode_DoesNotRequireEveryCheckbox()
        {
            var output = CreateOutput();
            var tagHelper = new FDCPSearchableSelectTagHelper
            {
                Name = "classificationLevels",
                Label = "Classification group & level",
                Required = true,
                SelectionMode = FDCPSearchableSelectSelectionMode.Multiple,
                Items = new List<SelectListItem>
                {
                    new() { Value = "EC-02", Text = "EC-02" },
                    new() { Value = "EC-03", Text = "EC-03" }
                }
            };

            await tagHelper.ProcessAsync(_context, output);
            string content = output.Content.GetContent();

            Assert.Equal("true", output.Attributes["data-required"].Value?.ToString());
            Assert.Contains("aria-required=\"true\"", content);
            Assert.Contains("type=\"checkbox\"", content);
            Assert.DoesNotContain("required />", content);
            Assert.Contains("<gcds-error-message id=\"classificationLevels_error\" message-id=\"classificationLevels_error\"", content);
            Assert.Contains("data-fdcp-searchable-select-error hidden", content);
        }

        private static TagHelperOutput CreateOutput(string childContent = "")
        {
            return new TagHelperOutput(
                "fdcp-searchable-select",
                new TagHelperAttributeList(),
                (useCachedResult, encoder) =>
                {
                    var content = new DefaultTagHelperContent();
                    content.SetHtmlContent(childContent);
                    return Task.FromResult<TagHelperContent>(content);
                });
        }

        private static ModelExpression MockModelExpression(string name, object value)
        {
            var metadataProvider = new EmptyModelMetadataProvider();
            var metadata = metadataProvider.GetMetadataForType(value.GetType());
            return new ModelExpression(name, new ModelExplorer(metadataProvider, metadata, value));
        }

        private static int CountOccurrences(string value, string search)
        {
            int count = 0;
            int index = 0;

            while ((index = value.IndexOf(search, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += search.Length;
            }

            return count;
        }
    }
}
