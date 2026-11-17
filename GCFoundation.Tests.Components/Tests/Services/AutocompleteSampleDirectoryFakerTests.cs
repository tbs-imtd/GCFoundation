using GCFoundation.Tests.Components.Tests.TestData;
using GCFoundation.Web.Services;
using System.Globalization;
using System.Text;

namespace GCFoundation.Tests.Components.Tests.Services
{
    /// <summary>
    /// Throws Faker-generated terms and paging values at the sample directory and checks rules that must hold for any input.
    /// </summary>
    public class AutocompleteSampleDirectoryFakerTests
    {
        private const int Samples = 500;

        private static readonly AutocompleteSampleDirectory Directory = new();

        [Fact]
        public void Search_GeneratedTermsAndPaging_AlwaysRespectLimits()
        {
            var faker = AutocompleteFakes.Create();

            for (int sample = 0; sample < Samples; sample++)
            {
                string term = AutocompleteFakes.SearchTerm(faker);
                int skip = faker.Random.Int(-100, 6000);
                int take = faker.Random.Int(-100, 1000);

                var result = Directory.Search(term, skip, take, faker.PickRandom("en", "fr", "fr-CA", "xx", null));

                Assert.InRange(result.Total, 0, Directory.Count);
                Assert.InRange(result.Items.Count, 0, AutocompleteSampleDirectory.MaxPageSize);
                Assert.True(result.Items.Count <= result.Total);
                Assert.All(result.Items, item =>
                {
                    Assert.Matches("^E[0-9]{6}$", item.Value);
                    Assert.False(string.IsNullOrWhiteSpace(item.Text));
                    Assert.False(string.IsNullOrWhiteSpace(item.Description));
                });
            }
        }

        [Fact]
        public void Search_GeneratedTerms_EveryResultContainsTheTerm()
        {
            var faker = AutocompleteFakes.Create();
            int termsWithMatches = 0;

            for (int sample = 0; sample < Samples; sample++)
            {
                string term = AutocompleteFakes.SearchTerm(faker);
                string folded = Fold(term);
                var result = Directory.Search(term, 0, 100, "en");

                if (result.Total > 0)
                {
                    termsWithMatches++;
                }

                Assert.All(result.Items, item =>
                {
                    string[] parts = item.Text.Split(' ', 2);
                    string searchable = Fold($"{item.Text} {parts[1]}, {parts[0]}");
                    Assert.Contains(folded, searchable, StringComparison.Ordinal);
                });
            }

            // The generated terms must actually exercise matching, not just the empty path.
            Assert.True(termsWithMatches > 20, $"only {termsWithMatches} generated terms matched");
        }

        [Fact]
        public void Search_GeneratedTerms_PagingIsStableAndComplete()
        {
            var faker = AutocompleteFakes.Create();

            for (int sample = 0; sample < 100; sample++)
            {
                string term = AutocompleteFakes.SearchTerm(faker);
                int pageSize = faker.Random.Int(1, AutocompleteSampleDirectory.MaxPageSize);
                var values = new List<string>();
                int total = Directory.Search(term, 0, pageSize, "en").Total;

                for (int skip = 0; skip < total; skip += pageSize)
                {
                    var page = Directory.Search(term, skip, pageSize, "en");
                    Assert.Equal(total, page.Total);
                    values.AddRange(page.Items.Select(item => item.Value));
                }

                Assert.Equal(total, values.Count);
                Assert.Equal(values.Count, values.Distinct(StringComparer.Ordinal).Count());
            }
        }

        [Fact]
        public void Search_GeneratedTerms_CaseAccentAndPaddingDoNotChangeResults()
        {
            var faker = AutocompleteFakes.Create();

            for (int sample = 0; sample < Samples; sample++)
            {
                string term = AutocompleteFakes.SearchTerm(faker).Trim();
                var expected = Directory.Search(term, 0, 50, "en");

                Assert.Equal(expected, Directory.Search(term.ToUpperInvariant(), 0, 50, "en"), ResultComparer.Instance);
                Assert.Equal(expected, Directory.Search($"\t {term}  ", 0, 50, "en"), ResultComparer.Instance);
                Assert.Equal(expected, Directory.Search(Fold(term), 0, 50, "en"), ResultComparer.Instance);
            }
        }

        [Fact]
        public void Search_GeneratedGarbage_NeverThrows()
        {
            var faker = AutocompleteFakes.Create();

            for (int sample = 0; sample < Samples; sample++)
            {
                string term = faker.PickRandom(
                    faker.Random.String(faker.Random.Int(0, 300)),
                    faker.Random.Utf16String(0, 40),
                    faker.PickRandom(AutocompleteFakes.Payloads),
                    faker.Lorem.Paragraph(),
                    $"{faker.Name.LastName()}'; DROP TABLE employees;--");

                var result = Directory.Search(term, faker.Random.Int(), faker.Random.Int(), faker.Random.String(3));

                Assert.NotNull(result.Items);
                Assert.True(result.Items.Count <= AutocompleteSampleDirectory.MaxPageSize);
            }
        }

        [Fact]
        public void Find_GeneratedValues_OnlyKnownIdsResolve()
        {
            var faker = AutocompleteFakes.Create();

            for (int sample = 0; sample < Samples; sample++)
            {
                int number = faker.Random.Int(1, Directory.Count);
                string known = $"E{number:D6}";

                Assert.Equal(known, Directory.Find(known, "en")?.Value);
                Assert.Null(Directory.Find($"E{Directory.Count + number:D6}", "en"));
                Assert.Null(Directory.Find(faker.Random.AlphaNumeric(7), "en"));
                Assert.Null(Directory.Find(known.ToLowerInvariant(), "en"));
            }
        }

        private static string Fold(string value)
        {
            var sb = new StringBuilder();

            foreach (char character in string.Join(' ', value.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                {
                    char lower = char.ToLowerInvariant(character);
                    sb.Append(lower switch
                    {
                        'œ' => "oe",
                        'æ' => "ae",
                        'ø' => "o",
                        'ß' => "ss",
                        'ł' => "l",
                        _ => lower.ToString()
                    });
                }
            }

            return sb.ToString();
        }

        private sealed class ResultComparer : IEqualityComparer<GCFoundation.Components.Models.FDCPAutocompleteResult>
        {
            public static readonly ResultComparer Instance = new();

            public bool Equals(GCFoundation.Components.Models.FDCPAutocompleteResult? x, GCFoundation.Components.Models.FDCPAutocompleteResult? y)
            {
                return x != null && y != null && x.Total == y.Total && x.Items.SequenceEqual(y.Items);
            }

            public int GetHashCode(GCFoundation.Components.Models.FDCPAutocompleteResult obj)
            {
                return obj.Total;
            }
        }
    }
}
