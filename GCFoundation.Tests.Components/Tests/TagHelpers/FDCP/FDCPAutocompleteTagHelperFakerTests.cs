using GCFoundation.Components.TagHelpers.FDCP;
using GCFoundation.Tests.Components.Tests.TestData;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Net;
using System.Text.RegularExpressions;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.FDCP
{
    /// <summary>
    /// Runs the tag helper against hundreds of Faker-generated inputs and checks properties that must hold for all of them.
    /// </summary>
    public partial class FDCPAutocompleteTagHelperFakerTests
    {
        private const int Samples = 300;

        private readonly TagHelperContext _context = new(
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            "test");

        [Fact]
        public async Task ProcessAsync_GeneratedText_RoundTripsThroughEncoding()
        {
            var faker = AutocompleteFakes.Create();

            for (int sample = 0; sample < Samples; sample++)
            {
                string label = faker.Name.FullName();
                string hint = faker.Lorem.Sentence();
                string selectedText = faker.Name.FullName();
                string value = faker.Random.AlphaNumeric(8);
                var output = CreateOutput();

                await new FDCPAutocompleteTagHelper
                {
                    Name = $"field{sample}",
                    Label = label,
                    Hint = hint,
                    Value = value,
                    SelectedText = selectedText,
                    Placeholder = faker.Company.CatchPhrase(),
                    Src = "/api/employees"
                }.ProcessAsync(_context, output);

                string content = output.Content.GetContent();

                // Decoding what was rendered must give back exactly what went in: nothing lost, nothing injected.
                Assert.Equal(label, WebUtility.HtmlDecode(LabelText().Match(content).Groups[1].Value));
                Assert.Equal(hint, WebUtility.HtmlDecode(HintText().Match(content).Groups[1].Value));
                Assert.Equal(selectedText, WebUtility.HtmlDecode(InputValue().Match(content).Groups[1].Value));
                Assert.Equal(value, WebUtility.HtmlDecode(HiddenValue().Match(content).Groups[1].Value));
            }
        }

        [Fact]
        public async Task ProcessAsync_GeneratedTextWithPayloads_NeverProducesMarkup()
        {
            var faker = AutocompleteFakes.Create();

            for (int sample = 0; sample < Samples; sample++)
            {
                string Hostile() => $"{faker.Name.FullName()} {faker.PickRandom(AutocompleteFakes.Payloads)} {faker.Lorem.Word()}";

                string label = Hostile();
                var output = CreateOutput();

                await new FDCPAutocompleteTagHelper
                {
                    Name = $"field{sample}",
                    Label = label,
                    Hint = Hostile(),
                    Value = Hostile(),
                    SelectedText = Hostile(),
                    Placeholder = Hostile(),
                    ClearText = Hostile(),
                    InstructionsText = Hostile(),
                    Src = "/api/employees"
                }.ProcessAsync(_context, output);

                string content = output.Content.GetContent();

                // Only the elements the component itself renders may appear.
                var tags = Tags().Matches(content).Select(match => match.Groups[1].Value).Distinct().Order().ToList();
                Assert.Equal(["button", "div", "gcds-error-message", "gcds-hint", "input", "label", "span"], tags);
                Assert.Equal(label, WebUtility.HtmlDecode(LabelText().Match(content).Groups[1].Value));
            }
        }

        [Fact]
        public async Task ProcessAsync_GeneratedFieldNames_ProduceValidUniqueIds()
        {
            var faker = AutocompleteFakes.Create();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            for (int sample = 0; sample < Samples; sample++)
            {
                // Model-binding style names: prefixes, indexers, dots, and the odd hostile fragment.
                string name = faker.Random.Bool(0.1f)
                    ? $"{faker.Hacker.Noun()}{faker.PickRandom(AutocompleteFakes.Payloads)}"
                    : $"{faker.Hacker.Noun()}[{sample}].{faker.Database.Column()}";
                var output = CreateOutput();

                await new FDCPAutocompleteTagHelper { Name = name, Label = faker.Name.JobTitle(), Src = "/api" }.ProcessAsync(_context, output);

                string content = output.Content.GetContent();
                string rootId = output.Attributes["id"].Value?.ToString() ?? string.Empty;
                var ids = Ids().Matches(content).Select(match => WebUtility.HtmlDecode(match.Groups[1].Value)).Append(rootId).ToList();

                Assert.All(ids, id => Assert.Matches("^[\\p{L}\\p{Nd}_:-]+$", id));
                Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
                Assert.All(ids, id => Assert.StartsWith(rootId, id, StringComparison.Ordinal));

                // Every ARIA reference must point at an ID that was rendered.
                foreach (Match reference in IdReferences().Matches(content))
                {
                    Assert.All(WebUtility.HtmlDecode(reference.Groups[1].Value).Split(' ', StringSplitOptions.RemoveEmptyEntries), id => Assert.Contains(id, ids));
                }

                // The posted field keeps the caller's name exactly.
                Assert.Equal(name, WebUtility.HtmlDecode(HiddenName().Match(content).Groups[1].Value));
                seen.Add(rootId);
            }

            Assert.True(seen.Count > Samples / 2);
        }

        [Fact]
        public async Task ProcessAsync_GeneratedSettings_AreAlwaysClampedToUsableValues()
        {
            var faker = AutocompleteFakes.Create();

            for (int sample = 0; sample < Samples; sample++)
            {
                var output = CreateOutput();

                await new FDCPAutocompleteTagHelper
                {
                    Name = "employee",
                    Label = "Employee",
                    Src = "/api",
                    MinLength = faker.Random.Int(-10, 10),
                    Debounce = faker.Random.Int(-1000, 2000),
                    PageSize = faker.Random.Int(-100, 500),
                    MaxResults = faker.Random.Int(-100, 10000),
                    CacheTtl = faker.Random.Int(-100, 3600),
                    CacheSize = faker.Random.Int(-10, 200)
                }.ProcessAsync(_context, output);

                int Read(string attribute) => int.Parse(output.Attributes[attribute].Value!.ToString()!, System.Globalization.CultureInfo.InvariantCulture);

                Assert.True(Read("data-min-length") >= 1);
                Assert.True(Read("data-debounce") >= 0);
                Assert.True(Read("data-page-size") >= 1);
                Assert.True(Read("data-max-results") >= Read("data-page-size"));
                Assert.True(Read("data-cache-ttl") >= 0);
                Assert.True(Read("data-cache-size") >= 1);
            }
        }

        [Fact]
        public async Task ProcessAsync_GeneratedUrls_AcceptHttpAndRelativeAndRejectOtherSchemes()
        {
            var faker = AutocompleteFakes.Create();

            for (int sample = 0; sample < Samples; sample++)
            {
                string path = faker.Internet.UrlRootedPath();
                string[] allowed = [path, path.TrimStart('/'), $"{path}?next={faker.Internet.Url()}", faker.Internet.UrlWithPath("https"), faker.Internet.UrlWithPath("http")];
                string[] rejected = [$"javascript:{faker.Lorem.Word()}()", $"data:text/html,{faker.Lorem.Word()}", $"{faker.PickRandom("ftp", "file", "vbscript", "ws", "blob")}://{faker.Internet.DomainName()}{path}"];

                foreach (string src in allowed)
                {
                    var output = CreateOutput();
                    await new FDCPAutocompleteTagHelper { Name = "employee", Label = "Employee", Src = src }.ProcessAsync(_context, output);
                    Assert.Equal(src, output.Attributes["data-src"].Value?.ToString());
                }

                foreach (string src in rejected)
                {
                    await Assert.ThrowsAsync<InvalidOperationException>(() =>
                        new FDCPAutocompleteTagHelper { Name = "employee", Label = "Employee", Src = src }.ProcessAsync(_context, CreateOutput()));
                }
            }
        }

        private static TagHelperOutput CreateOutput()
        {
            return new TagHelperOutput(
                "fdcp-autocomplete",
                new TagHelperAttributeList(),
                (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
        }

        [GeneratedRegex("<label [^>]*>(.*?)</label>", RegexOptions.Singleline)]
        private static partial Regex LabelText();

        [GeneratedRegex("<gcds-hint [^>]*>(.*?)</gcds-hint>", RegexOptions.Singleline)]
        private static partial Regex HintText();

        [GeneratedRegex("spellcheck=\"false\"\\s+value=\"([^\"]*)\"")]
        private static partial Regex InputValue();

        [GeneratedRegex("<input type=\"hidden\"\\s+name=\"[^\"]*\"\\s+value=\"([^\"]*)\"")]
        private static partial Regex HiddenValue();

        [GeneratedRegex("<input type=\"hidden\"\\s+name=\"([^\"]*)\"")]
        private static partial Regex HiddenName();

        [GeneratedRegex("<([a-zA-Z][a-zA-Z0-9-]*)")]
        private static partial Regex Tags();

        [GeneratedRegex("\\sid=\"([^\"]*)\"")]
        private static partial Regex Ids();

        [GeneratedRegex("(?:for|aria-controls|aria-labelledby|aria-describedby)=\"([^\"]*)\"")]
        private static partial Regex IdReferences();
    }
}
