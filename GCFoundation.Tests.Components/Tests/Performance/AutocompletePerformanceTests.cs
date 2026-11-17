using GCFoundation.Components.Models;
using GCFoundation.Components.TagHelpers.FDCP;
using GCFoundation.Tests.Components.Tests.TestData;
using GCFoundation.Web.Services;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Diagnostics;
using System.Text.Json;

namespace GCFoundation.Tests.Components.Tests.Performance
{
    /// <summary>
    /// Runs alone so other test classes do not compete for CPU while timings are taken.
    /// </summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class PerformanceCollection
    {
        public const string Name = "Performance";
    }

    /// <summary>
    /// Performance budgets for the server side of fdcp-autocomplete.
    /// Budgets are several times what a developer laptop needs, so they catch a change in algorithmic cost
    /// (for example a search that becomes quadratic) without failing on a slow build agent.
    /// Run only these with: dotnet test --filter "Category=Performance"
    /// </summary>
    [Collection(PerformanceCollection.Name)]
    [Trait("Category", "Performance")]
    public class AutocompletePerformanceTests(ITestOutputHelper output)
    {
        private static readonly AutocompleteSampleDirectory Directory = new();
        private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web);

        [Fact]
        public void Directory_FiveThousandRecords_BuildsWithinBudget()
        {
            var elapsed = Measure(() => _ = new AutocompleteSampleDirectory(), runs: 5);

            Report("build 5,000 records", elapsed, TimeSpan.FromMilliseconds(250));
        }

        [Fact]
        public void Directory_FiftyThousandRecords_BuildsWithinBudget()
        {
            var elapsed = Measure(() => _ = new AutocompleteSampleDirectory(50_000, 1), runs: 3);

            Report("build 50,000 records", elapsed, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Search_GeneratedTermsOverFiveThousandRecords_AveragesUnderBudget()
        {
            var faker = AutocompleteFakes.Create();
            var terms = Enumerable.Range(0, 1000).Select(_ => AutocompleteFakes.SearchTerm(faker)).ToArray();

            var elapsed = Measure(() =>
            {
                foreach (string term in terms)
                {
                    Directory.Search(term, 0, 50, "en");
                }
            });

            Report("search, 5,000 records (per call)", elapsed / terms.Length, TimeSpan.FromMilliseconds(2));
        }

        [Fact]
        public void Search_BroadTermOverFiveThousandRecords_StaysUnderBudget()
        {
            // "a" + "n" appears in a large share of names: the worst case for building a page.
            Assert.True(Directory.Search("an", 0, 1, "en").Total > 500);

            var elapsed = Measure(() => Directory.Search("an", 0, 100, "en"), runs: 200);

            Report("broad search, first page", elapsed, TimeSpan.FromMilliseconds(5));
        }

        [Fact]
        public void Search_DeepPage_CostsAboutTheSameAsTheFirstPage()
        {
            int total = Directory.Search("an", 0, 1, "en").Total;
            int lastPage = Math.Max(0, total - 100);

            var first = Measure(() => Directory.Search("an", 0, 100, "en"), runs: 300);
            var deep = Measure(() => Directory.Search("an", lastPage, 100, "en"), runs: 300);

            Report("broad search, last page", deep, TimeSpan.FromMilliseconds(5));

            // Paging must not get slower the deeper the caller goes (allowing for timer noise on tiny durations).
            Assert.True(deep < first * 5 + TimeSpan.FromMilliseconds(1), $"first page {first.TotalMilliseconds:F3} ms, last page {deep.TotalMilliseconds:F3} ms");
        }

        [Fact]
        public void Search_FiftyThousandRecords_StaysUnderBudget()
        {
            var large = new AutocompleteSampleDirectory(50_000, 1);
            var faker = AutocompleteFakes.Create();
            var terms = Enumerable.Range(0, 200).Select(_ => AutocompleteFakes.SearchTerm(faker)).ToArray();

            var elapsed = Measure(() =>
            {
                foreach (string term in terms)
                {
                    large.Search(term, 0, 50, "en");
                }
            });

            Report("search, 50,000 records (per call)", elapsed / terms.Length, TimeSpan.FromMilliseconds(15));
        }

        [Fact]
        public void Search_CostGrowsLinearlyWithDirectorySize()
        {
            var small = new AutocompleteSampleDirectory(5_000, 1);
            var large = new AutocompleteSampleDirectory(50_000, 1);

            // A term nothing matches forces a full scan in both: a clean measure of per-record cost.
            var smallScan = Measure(() => small.Search("zzzz", 0, 50, "en"), runs: 300);
            var largeScan = Measure(() => large.Search("zzzz", 0, 50, "en"), runs: 300);
            double ratio = largeScan.TotalMilliseconds / Math.Max(0.0001, smallScan.TotalMilliseconds);

            output.WriteLine($"full scan: 5,000 = {smallScan.TotalMilliseconds:F4} ms, 50,000 = {largeScan.TotalMilliseconds:F4} ms, ratio {ratio:F1}x for 10x the data");

            // 10x the records should cost about 10x. Well beyond that means the scan is no longer linear.
            Assert.True(ratio < 40, $"50,000 records cost {ratio:F1}x the time of 5,000");
        }

        [Fact]
        public void Find_ByValue_IsConstantTime()
        {
            var faker = AutocompleteFakes.Create();
            var ids = Enumerable.Range(0, 10_000).Select(_ => $"E{faker.Random.Int(1, 5000):D6}").ToArray();

            var elapsed = Measure(() =>
            {
                foreach (string id in ids)
                {
                    Directory.Find(id, "fr");
                }
            });

            Report("find by value (per call)", elapsed / ids.Length, TimeSpan.FromMilliseconds(0.01));
        }

        [Fact]
        public async Task TagHelper_Render_AveragesUnderBudget()
        {
            var faker = AutocompleteFakes.Create();
            var context = new TagHelperContext(new TagHelperAttributeList(), new Dictionary<object, object>(), "perf");
            var helpers = Enumerable.Range(0, 2000).Select(index => new FDCPAutocompleteTagHelper
            {
                Name = $"field{index}",
                Label = faker.Name.JobTitle(),
                Hint = faker.Lorem.Sentence(),
                Value = faker.Random.AlphaNumeric(8),
                SelectedText = faker.Name.FullName(),
                Required = true,
                Src = "/api/employees"
            }).ToArray();

            async Task RenderAll()
            {
                foreach (var helper in helpers)
                {
                    await helper.ProcessAsync(context, CreateOutput());
                }
            }

            await RenderAll();

            var stopwatch = Stopwatch.StartNew();
            await RenderAll();
            stopwatch.Stop();

            Report("tag helper render (per instance)", stopwatch.Elapsed / helpers.Length, TimeSpan.FromMilliseconds(0.5));
        }

        [Fact]
        public void Serialization_HundredItemPage_AveragesUnderBudget()
        {
            var page = new FDCPAutocompleteResult(5000, AutocompleteFakes.Items(100));

            var elapsed = Measure(() => JsonSerializer.Serialize(page, WebJson), runs: 500);

            Report("serialize 100-item page", elapsed, TimeSpan.FromMilliseconds(2));
            Assert.True(JsonSerializer.SerializeToUtf8Bytes(page, WebJson).Length < 32 * 1024, "a 100-item page should stay well under 32 KB");
        }

        /// <summary>Warms up once, then returns the fastest of several runs: the least noisy estimate of the real cost.</summary>
        private static TimeSpan Measure(Action action, int runs = 5)
        {
            action();

            var best = TimeSpan.MaxValue;

            for (int run = 0; run < runs; run++)
            {
                var stopwatch = Stopwatch.StartNew();
                action();
                stopwatch.Stop();

                if (stopwatch.Elapsed < best)
                {
                    best = stopwatch.Elapsed;
                }
            }

            return best;
        }

        private void Report(string name, TimeSpan actual, TimeSpan budget)
        {
            output.WriteLine($"{name}: {actual.TotalMilliseconds:F4} ms (budget {budget.TotalMilliseconds:F2} ms)");
            Assert.True(actual <= budget, $"{name} took {actual.TotalMilliseconds:F4} ms, budget is {budget.TotalMilliseconds:F2} ms");
        }

        private static TagHelperOutput CreateOutput()
        {
            return new TagHelperOutput(
                "fdcp-autocomplete",
                new TagHelperAttributeList(),
                (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
        }
    }
}
