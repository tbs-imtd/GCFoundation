using GCFoundation.Components.TagHelpers.FDCP;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.FDCP
{
    public class FDCPAutocompleteTagHelperTests
    {
        private readonly TagHelperContext _context;

        public FDCPAutocompleteTagHelperTests()
        {
            _context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                "test");
        }

        [Fact]
        public async Task ProcessAsync_Standalone_RendersComboboxContract()
        {
            var output = CreateOutput();
            var tagHelper = new FDCPAutocompleteTagHelper
            {
                Name = "employee",
                Label = "Employee",
                Hint = "Type a name",
                Src = "/api/employees"
            };

            await tagHelper.ProcessAsync(_context, output);

            string content = output.Content.GetContent();

            Assert.Equal("div", output.TagName);
            Assert.Equal("fdcp-autocomplete", output.Attributes["class"].Value?.ToString());
            Assert.Equal("employee", output.Attributes["id"].Value?.ToString());
            Assert.Equal("/api/employees", output.Attributes["data-src"].Value?.ToString());
            Assert.True(output.Attributes.ContainsName("data-fdcp-autocomplete"));

            Assert.Contains("<label class=\"fdcp-autocomplete__label gcds-label\" id=\"employee_label\" for=\"employee_input\">Employee</label>", content);
            Assert.Contains("<gcds-hint hint-id=\"employee_hint\" id=\"employee_hint\">Type a name</gcds-hint>", content);
            Assert.Contains("id=\"employee_input\"", content);
            Assert.Contains("role=\"combobox\"", content);
            Assert.Contains("aria-autocomplete=\"list\"", content);
            Assert.Contains("aria-expanded=\"false\"", content);
            Assert.Contains("aria-controls=\"employee_listbox\"", content);
            Assert.Contains("aria-describedby=\"employee_hint employee_error employee_instructions\"", content);
            Assert.Contains("autocomplete=\"off\"", content);
            Assert.Contains("id=\"employee_listbox\" role=\"listbox\" aria-labelledby=\"employee_label\"", content);
            Assert.Contains("id=\"employee_status\" aria-live=\"polite\" aria-atomic=\"true\"", content);
            Assert.Contains("id=\"employee_instructions\"", content);
            Assert.Contains("data-fdcp-autocomplete-panel", content);
            Assert.DoesNotContain("aria-invalid", content);
            Assert.DoesNotContain("aria-required", content);
        }

        [Fact]
        public async Task ProcessAsync_Standalone_SubmitsValueThroughHiddenInputOnly()
        {
            var output = CreateOutput();
            var tagHelper = new FDCPAutocompleteTagHelper { Name = "employee", Label = "Employee", Src = "/api" };

            await tagHelper.ProcessAsync(_context, output);

            string content = output.Content.GetContent();

            Assert.Equal(1, CountOccurrences(content, "name=\"employee\""));
            Assert.Contains("<input type=\"hidden\"", content);
            Assert.Contains("data-fdcp-autocomplete-value", content);
        }

        [Fact]
        public async Task ProcessAsync_WithForBinding_UsesModelNameAndValue()
        {
            var output = CreateOutput();
            var tagHelper = new FDCPAutocompleteTagHelper
            {
                For = MockModelExpression("EmployeeId", "E000123"),
                Label = "Employee",
                SelectedText = "Marie Tremblay",
                Src = "/api"
            };

            await tagHelper.ProcessAsync(_context, output);

            string content = output.Content.GetContent();

            Assert.Equal("EmployeeId", output.Attributes["id"].Value?.ToString());
            Assert.Contains("name=\"EmployeeId\"", content);
            Assert.Contains("value=\"E000123\"", content);
            Assert.Contains("value=\"Marie Tremblay\"", content);
        }

        [Fact]
        public async Task ProcessAsync_WithValueButNoSelectedText_ShowsValue()
        {
            var output = CreateOutput();
            var tagHelper = new FDCPAutocompleteTagHelper { Name = "employee", Label = "Employee", Value = "E1", Src = "/api" };

            await tagHelper.ProcessAsync(_context, output);

            Assert.Equal(2, CountOccurrences(output.Content.GetContent(), "value=\"E1\""));
        }

        [Fact]
        public async Task ProcessAsync_WithoutValue_IgnoresSelectedTextAndHidesClearButton()
        {
            var output = CreateOutput();
            var tagHelper = new FDCPAutocompleteTagHelper { Name = "employee", Label = "Employee", SelectedText = "Orphan", Src = "/api" };

            await tagHelper.ProcessAsync(_context, output);

            string content = output.Content.GetContent();

            Assert.DoesNotContain("Orphan", content);
            Assert.Contains("data-fdcp-autocomplete-clear hidden", content);
        }

        [Fact]
        public async Task ProcessAsync_WithValue_ShowsClearButtonWithLocalizedName()
        {
            var output = CreateOutput();
            var tagHelper = new FDCPAutocompleteTagHelper { Name = "employee", Label = "Employee", Value = "E1", SelectedText = "Marie", Src = "/api" };

            await tagHelper.ProcessAsync(_context, output);

            string content = output.Content.GetContent();

            Assert.Contains("aria-label=\"Clear selection\"", content);
            Assert.DoesNotContain("data-fdcp-autocomplete-clear hidden", content);
        }

        [Fact]
        public async Task ProcessAsync_Required_RendersRequiredStateAndMessages()
        {
            var output = CreateOutput();
            var tagHelper = new FDCPAutocompleteTagHelper { Name = "employee", Label = "Employee", Required = true, Src = "/api" };

            await tagHelper.ProcessAsync(_context, output);

            string content = output.Content.GetContent();

            Assert.Equal("true", output.Attributes["data-required"].Value?.ToString());
            Assert.Equal("This field is required.", output.Attributes["data-required-message"].Value?.ToString());
            Assert.Equal("Employee: This field is required.", output.Attributes["data-required-summary-message"].Value?.ToString());
            Assert.Equal("Select an option from the list.", output.Attributes["data-invalid-message"].Value?.ToString());
            Assert.Equal("Employee: Select an option from the list.", output.Attributes["data-invalid-summary-message"].Value?.ToString());
            Assert.Contains("aria-required=\"true\"", content);
            Assert.Contains("<span class=\"label--required\" aria-hidden=\"true\">(required)</span>", content);
            Assert.Contains("data-fdcp-autocomplete-error hidden", content);
        }

        [Fact]
        public async Task ProcessAsync_RendersErrorMessageAfterHintAndBeforeControl()
        {
            var output = CreateOutput();
            var tagHelper = new FDCPAutocompleteTagHelper { Name = "employee", Label = "Employee", Hint = "Hint", Src = "/api" };

            await tagHelper.ProcessAsync(_context, output);

            string content = output.Content.GetContent();
            int hint = content.IndexOf("<gcds-hint", StringComparison.Ordinal);
            int error = content.IndexOf("<gcds-error-message id=\"employee_error\" message-id=\"employee_error\"", StringComparison.Ordinal);
            int input = content.IndexOf("role=\"combobox\"", StringComparison.Ordinal);

            Assert.True(hint >= 0 && hint < error);
            Assert.True(error < input);
        }

        [Fact]
        public async Task ProcessAsync_Disabled_DisablesEveryControl()
        {
            var output = CreateOutput();
            var tagHelper = new FDCPAutocompleteTagHelper { Name = "employee", Label = "Employee", Disabled = true, Src = "/api" };

            await tagHelper.ProcessAsync(_context, output);

            Assert.Equal(3, CountOccurrences(output.Content.GetContent(), " disabled"));
        }

        [Fact]
        public async Task ProcessAsync_Defaults_PassLookupSettingsToScript()
        {
            var output = CreateOutput();
            var tagHelper = new FDCPAutocompleteTagHelper { Name = "employee", Label = "Employee", Src = "/api" };

            await tagHelper.ProcessAsync(_context, output);

            Assert.Equal("q", output.Attributes["data-query-param"].Value?.ToString());
            Assert.Equal("2", output.Attributes["data-min-length"].Value?.ToString());
            Assert.Equal("250", output.Attributes["data-debounce"].Value?.ToString());
            Assert.Equal("50", output.Attributes["data-page-size"].Value?.ToString());
            Assert.Equal("1000", output.Attributes["data-max-results"].Value?.ToString());
            Assert.Equal("true", output.Attributes["data-cache"].Value?.ToString());
            Assert.Equal("300", output.Attributes["data-cache-ttl"].Value?.ToString());
            Assert.Equal("50", output.Attributes["data-cache-size"].Value?.ToString());
            Assert.Equal("false", output.Attributes["data-required"].Value?.ToString());
        }

        [Fact]
        public async Task ProcessAsync_CustomSettings_AreClampedToSafeValues()
        {
            var output = CreateOutput();
            var tagHelper = new FDCPAutocompleteTagHelper
            {
                Name = "employee",
                Label = "Employee",
                Src = "/api",
                QueryParam = "term",
                MinLength = 0,
                Debounce = -5,
                PageSize = 100,
                MaxResults = 10,
                Cache = false,
                CacheTtl = -1,
                CacheSize = 0
            };

            await tagHelper.ProcessAsync(_context, output);

            Assert.Equal("term", output.Attributes["data-query-param"].Value?.ToString());
            Assert.Equal("1", output.Attributes["data-min-length"].Value?.ToString());
            Assert.Equal("0", output.Attributes["data-debounce"].Value?.ToString());
            Assert.Equal("100", output.Attributes["data-page-size"].Value?.ToString());
            Assert.Equal("100", output.Attributes["data-max-results"].Value?.ToString());
            Assert.Equal("false", output.Attributes["data-cache"].Value?.ToString());
            Assert.Equal("0", output.Attributes["data-cache-ttl"].Value?.ToString());
            Assert.Equal("1", output.Attributes["data-cache-size"].Value?.ToString());
        }

        [Fact]
        public async Task ProcessAsync_English_PassesLocalizedTextToScript()
        {
            var output = await ProcessInCultureAsync("en-CA");

            Assert.Equal("en", output.Attributes["lang"].Value?.ToString());
            Assert.Equal("Loading results...", output.Attributes["data-loading-text"].Value?.ToString());
            Assert.Equal("No results found", output.Attributes["data-no-results-text"].Value?.ToString());
            Assert.Equal("1 result available", output.Attributes["data-one-result-text"].Value?.ToString());
            Assert.Equal("{0} results available", output.Attributes["data-multiple-results-text"].Value?.ToString());
            Assert.Equal("Type 3 or more characters to search", output.Attributes["data-min-length-text"].Value?.ToString());
            Assert.Equal("Unable to load results. Please try again.", output.Attributes["data-load-error-text"].Value?.ToString());
            Assert.Equal("{0} selected", output.Attributes["data-selected-announcement-text"].Value?.ToString());
        }

        [Fact]
        public async Task ProcessAsync_French_PassesLocalizedTextToScript()
        {
            var output = await ProcessInCultureAsync("fr-CA");
            string content = output.Content.GetContent();

            Assert.Equal("fr", output.Attributes["lang"].Value?.ToString());
            Assert.Equal("Chargement des résultats...", output.Attributes["data-loading-text"].Value?.ToString());
            Assert.Equal("Aucun résultat trouvé", output.Attributes["data-no-results-text"].Value?.ToString());
            Assert.Equal("Sélection : {0}", output.Attributes["data-selected-announcement-text"].Value?.ToString());
            Assert.Equal("{0} résultats disponibles", output.Attributes["data-multiple-results-text"].Value?.ToString());
            Assert.Equal("Saisissez au moins 3 caractères pour lancer la recherche", output.Attributes["data-min-length-text"].Value?.ToString());
            Assert.Equal("Ce champ est obligatoire.", output.Attributes["data-required-message"].Value?.ToString());
            Assert.Equal("Sélectionnez une option dans la liste.", output.Attributes["data-invalid-message"].Value?.ToString());
            Assert.Equal("Employé : Sélectionnez une option dans la liste.", output.Attributes["data-invalid-summary-message"].Value?.ToString());
            Assert.Contains("Effacer la s", content, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ProcessAsync_TextOverrides_ReplaceLocalizedDefaults()
        {
            var output = CreateOutput();
            var tagHelper = new FDCPAutocompleteTagHelper
            {
                Name = "employee",
                Label = "Employee",
                Src = "/api",
                LoadingText = "Searching",
                NoResultsText = "Nobody",
                OneResultText = "One",
                MultipleResultsText = "{0} people",
                MinLengthText = "Need {0}",
                LoadErrorText = "Broken",
                SelectedAnnouncementText = "Picked {0}",
                ClearText = "Remove",
                InstructionsText = "How to"
            };

            await tagHelper.ProcessAsync(_context, output);

            string content = output.Content.GetContent();

            Assert.Equal("Searching", output.Attributes["data-loading-text"].Value?.ToString());
            Assert.Equal("Nobody", output.Attributes["data-no-results-text"].Value?.ToString());
            Assert.Equal("One", output.Attributes["data-one-result-text"].Value?.ToString());
            Assert.Equal("{0} people", output.Attributes["data-multiple-results-text"].Value?.ToString());
            Assert.Equal("Need 2", output.Attributes["data-min-length-text"].Value?.ToString());
            Assert.Equal("Broken", output.Attributes["data-load-error-text"].Value?.ToString());
            Assert.Equal("Picked {0}", output.Attributes["data-selected-announcement-text"].Value?.ToString());
            Assert.Contains("aria-label=\"Remove\"", content);
            Assert.Contains(">How to</span>", content);
        }

        [Fact]
        public async Task ProcessAsync_HostileText_IsEncoded()
        {
            const string Hostile = "\"><script>alert(1)</script>";
            var output = CreateOutput();
            var tagHelper = new FDCPAutocompleteTagHelper
            {
                Name = "employee",
                Label = Hostile,
                Hint = Hostile,
                Placeholder = Hostile,
                Value = Hostile,
                SelectedText = Hostile,
                ClearText = Hostile,
                InstructionsText = Hostile,
                Src = "/api"
            };

            await tagHelper.ProcessAsync(_context, output);

            string content = output.Content.GetContent();

            Assert.DoesNotContain("<script", content, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\"><", content, StringComparison.Ordinal);
            Assert.Contains("&lt;script&gt;", content);
        }

        [Fact]
        public async Task ProcessAsync_HostileId_IsSanitized()
        {
            var output = CreateOutput();
            var tagHelper = new FDCPAutocompleteTagHelper { Name = "people[0].id\" onfocus=\"x", Label = "Employee", Src = "/api" };

            await tagHelper.ProcessAsync(_context, output);

            string id = output.Attributes["id"].Value?.ToString() ?? string.Empty;

            Assert.All(id, character => Assert.True(char.IsLetterOrDigit(character) || character is '_' or '-' or ':'));
            Assert.DoesNotContain("onfocus=\"x\"", output.Content.GetContent(), StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("/api/employees")]
        [InlineData("api/employees?dept=1")]
        [InlineData("../employees")]
        [InlineData("/en/search?next=http://example.org")]
        [InlineData("https://api.example.gc.ca/employees")]
        [InlineData("HTTP://api.example.gc.ca/employees")]
        public async Task ProcessAsync_AllowsRelativeAndHttpSources(string src)
        {
            var output = CreateOutput();
            var tagHelper = new FDCPAutocompleteTagHelper { Name = "employee", Label = "Employee", Src = src };

            await tagHelper.ProcessAsync(_context, output);

            Assert.Equal(src, output.Attributes["data-src"].Value?.ToString());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("javascript:alert(1)")]
        [InlineData(" JavaScript:alert(1)")]
        [InlineData("data:application/json,[]")]
        [InlineData("vbscript:x")]
        [InlineData("file:///etc/passwd")]
        public async Task ProcessAsync_RejectsUnsafeSources(string src)
        {
            var output = CreateOutput();
            var tagHelper = new FDCPAutocompleteTagHelper { Name = "employee", Label = "Employee", Src = src };

            await Assert.ThrowsAsync<InvalidOperationException>(() => tagHelper.ProcessAsync(_context, output));
        }

        [Fact]
        public async Task ProcessAsync_WithoutForOrName_Throws()
        {
            var output = CreateOutput();
            var tagHelper = new FDCPAutocompleteTagHelper { Label = "Employee", Src = "/api" };

            await Assert.ThrowsAsync<InvalidOperationException>(() => tagHelper.ProcessAsync(_context, output));
        }

        [Fact]
        public async Task ProcessAsync_TwoInstances_RenderUniqueIds()
        {
            var first = CreateOutput();
            var second = CreateOutput();

            await new FDCPAutocompleteTagHelper { Name = "employee", Label = "Employee", Src = "/api" }.ProcessAsync(_context, first);
            await new FDCPAutocompleteTagHelper { Name = "manager", Label = "Manager", Src = "/api" }.ProcessAsync(_context, second);

            var ids = ExtractIds(first.Content.GetContent()).Concat(ExtractIds(second.Content.GetContent())).ToList();

            Assert.NotEmpty(ids);
            Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
        }

        private async Task<TagHelperOutput> ProcessInCultureAsync(string culture)
        {
            var originalCulture = CultureInfo.CurrentCulture;
            var originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                CultureInfo.CurrentUICulture = new CultureInfo(culture);

                var output = CreateOutput();
                var tagHelper = new FDCPAutocompleteTagHelper
                {
                    Name = "employee",
                    Label = culture.StartsWith("fr", StringComparison.Ordinal) ? "Employé" : "Employee",
                    MinLength = 3,
                    Required = true,
                    Src = "/api"
                };

                await tagHelper.ProcessAsync(_context, output);
                return output;
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        private static TagHelperOutput CreateOutput()
        {
            return new TagHelperOutput(
                "fdcp-autocomplete",
                new TagHelperAttributeList(),
                (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
        }

        private static ModelExpression MockModelExpression(string name, object value)
        {
            var metadataProvider = new EmptyModelMetadataProvider();
            var metadata = metadataProvider.GetMetadataForType(value.GetType());
            return new ModelExpression(name, new ModelExplorer(metadataProvider, metadata, value));
        }

        private static IEnumerable<string> ExtractIds(string html)
        {
            const string Marker = " id=\"";
            int index = 0;

            while ((index = html.IndexOf(Marker, index, StringComparison.Ordinal)) >= 0)
            {
                int start = index + Marker.Length;
                int end = html.IndexOf('"', start);
                yield return html[start..end];
                index = end;
            }
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
