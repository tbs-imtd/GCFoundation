using GCFoundation.Components.Models;
using GCFoundation.Tests.Components.Tests.TestData;
using GCFoundation.Web.Controllers;
using GCFoundation.Web.Models.Components;
using GCFoundation.Web.Services;
using GCFoundation.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace GCFoundation.Tests.Components.Tests.Controllers
{
    /// <summary>
    /// The four server-side data source samples (Web API, database, GraphQL, existing API) read the same
    /// 5,000 records. These tests prove each one works and that all four agree with each other.
    /// </summary>
    public class AutocompleteDataSourceEndpointTests
    {
        private const string GraphQLQuery = "query Employees($term: String!, $skip: Int!, $take: Int!, $lang: String!) { employees(term: $term, skip: $skip, take: $take, lang: $lang) { total items { value text description } } }";

        private static readonly AutocompleteSampleDirectory Directory = new();
        private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web);

        /* ---------- All sources agree ---------- */

        [Fact]
        public void AllSources_GeneratedSearches_ReturnIdenticalResults()
        {
            var faker = AutocompleteFakes.Create();
            using var controller = CreateController();
            int searchesWithResults = 0;

            for (int sample = 0; sample < 300; sample++)
            {
                string term = AutocompleteFakes.SearchTerm(faker);
                int pageSize = faker.PickRandom(10, 25, 50, 100);
                int page = faker.Random.Int(1, 4);
                int skip = (page - 1) * pageSize;
                string lang = faker.PickRandom("en", "fr");

                var webApi = WebApi(controller, term, skip, pageSize, lang);
                var database = Database(controller, term, skip, pageSize, lang);
                var graphQL = GraphQL(controller, term, skip, pageSize, lang);
                var legacy = Legacy(controller, term, page, pageSize, lang);

                Assert.Equal(webApi.Total, database.Total);
                Assert.Equal(webApi.Total, graphQL.Total);
                Assert.Equal(webApi.Total, legacy.Total);
                Assert.Equal(webApi.Items, database.Items);
                Assert.Equal(webApi.Items, graphQL.Items);
                Assert.Equal(webApi.Items, legacy.Items);

                if (webApi.Items.Count > 0)
                {
                    searchesWithResults++;
                }
            }

            Assert.True(searchesWithResults > 20, $"only {searchesWithResults} generated searches returned results");
        }

        [Theory]
        [InlineData("tremblay", "en")]
        [InlineData("COTE", "fr")]
        [InlineData("roy, je", "en")]
        [InlineData("an", "fr")]
        public void AllSources_PagedToTheEnd_ReturnEveryMatchOnce(string term, string lang)
        {
            using var controller = CreateController();
            int total = WebApi(controller, term, 0, 1, lang).Total;

            Assert.True(total > 0);

            foreach (var source in new Func<int, FDCPAutocompleteResult>[]
            {
                skip => WebApi(controller, term, skip, 50, lang),
                skip => Database(controller, term, skip, 50, lang),
                skip => GraphQL(controller, term, skip, 50, lang),
                skip => Legacy(controller, term, (skip / 50) + 1, 50, lang)
            })
            {
                var values = new List<string>();

                for (int skip = 0; skip < total; skip += 50)
                {
                    values.AddRange(source(skip).Items.Select(item => item.Value));
                }

                Assert.Equal(total, values.Count);
                Assert.Equal(total, values.Distinct(StringComparer.Ordinal).Count());
            }
        }

        /* ---------- Database (LINQ over IQueryable) ---------- */

        [Fact]
        public void Database_RunsItsQueryThroughTheQueryableSet()
        {
            // A mocked "DbSet": the action must use only Query(), never the directory's own Search().
            var people = new[]
            {
                new AutocompleteSamplePerson("E3", "Zoé Côté", "zoe cote cote, zoe", "Director, Finance", "Directeur, Finances"),
                new AutocompleteSamplePerson("E1", "Anne Côté", "anne cote cote, anne", "Analyst, Policy", "Analyste, Politiques"),
                new AutocompleteSamplePerson("E2", "Liam Roy", "liam roy roy, liam", "Developer, Digital Services", "Développeur, Services numériques")
            };
            var directory = new Mock<IAutocompleteSampleDirectory>(MockBehavior.Strict);
            directory.Setup(d => d.Query()).Returns(people.AsQueryable());

            using var controller = CreateController();
            var result = Read<FDCPAutocompleteResult>(controller.AutocompleteDatabase(directory.Object, "Côté", 0, 50, "fr"));

            Assert.Equal(2, result.Total);
            Assert.Equal(["Anne Côté", "Zoé Côté"], result.Items.Select(item => item.Text));
            Assert.Equal(["Analyste, Politiques", "Directeur, Finances"], result.Items.Select(item => item.Description));
            directory.Verify(d => d.Query(), Times.Once);
        }

        [Theory]
        [InlineData("loeuillet")]
        [InlineData("LŒUILLET")]
        [InlineData("lœuil")]
        [InlineData("soren")]
        [InlineData("Søren")]
        [InlineData("strasse")]
        [InlineData("Straße")]
        [InlineData("lukasz")]
        [InlineData("Łukasz")]
        [InlineData("noel")]
        [InlineData("Noël")]
        public void Database_SpecialLettersAndAccents_MatchEitherWay(string term)
        {
            var rows = new[] { "Zoé Lœuillet", "Søren Ødegård", "Anna Straße", "Łukasz Nowak", "Noël Côté", "Liam Roy" }
                .Select((name, index) =>
                {
                    string[] parts = name.Split(' ');
                    string searchName = AutocompleteSampleDirectory.NormalizeTerm($"{name} {parts[1]}, {parts[0]}");
                    return new AutocompleteSamplePerson($"E{index + 1}", name, searchName, "en", "fr");
                })
                .ToList();
            var directory = new Mock<IAutocompleteSampleDirectory>();
            directory.Setup(d => d.Query()).Returns(rows.AsQueryable());

            using var controller = CreateController();
            var result = Read<FDCPAutocompleteResult>(controller.AutocompleteDatabase(directory.Object, term));

            var match = Assert.Single(result.Items);
            Assert.DoesNotContain("Roy", match.Text, StringComparison.Ordinal);
            Assert.Contains(AutocompleteSampleDirectory.NormalizeTerm(term), AutocompleteSampleDirectory.NormalizeTerm(match.Text), StringComparison.Ordinal);
        }

        [Fact]
        public void Database_GeneratedRows_AreFilteredSortedAndPaged()
        {
            // 5,000 Faker rows stand in for a table.
            var rows = AutocompleteFakes.Items(5000)
                .Select(item => new AutocompleteSamplePerson(item.Value, item.Text, AutocompleteSampleDirectory.NormalizeTerm(item.Text), "en", "fr"))
                .ToList();
            var directory = new Mock<IAutocompleteSampleDirectory>();
            directory.Setup(d => d.Query()).Returns(() => rows.AsQueryable());
            var faker = AutocompleteFakes.Create(7);
            using var controller = CreateController();

            for (int sample = 0; sample < 100; sample++)
            {
                string term = AutocompleteSampleDirectory.NormalizeTerm(AutocompleteFakes.SearchTerm(faker));
                int skip = faker.Random.Int(0, 40);
                int take = faker.Random.Int(1, 60);
                bool longEnough = term.Length >= AutocompleteSampleDirectory.MinTermLength;
                var expected = rows
                    .Where(row => longEnough && row.SearchName.Contains(term, StringComparison.Ordinal))
                    .OrderBy(row => row.SearchName, StringComparer.Ordinal).ThenBy(row => row.Id, StringComparer.Ordinal)
                    .ToList();

                var result = Read<FDCPAutocompleteResult>(controller.AutocompleteDatabase(directory.Object, term, skip, take, "en"));

                Assert.Equal(expected.Count, result.Total);
                Assert.Equal(expected.Skip(skip).Take(take).Select(row => row.Id), result.Items.Select(item => item.Value));
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("a")]
        [InlineData("   ")]
        public void Database_TermTooShort_NeverTouchesTheDatabase(string? term)
        {
            var directory = new Mock<IAutocompleteSampleDirectory>(MockBehavior.Strict);

            using var controller = CreateController();
            var result = Read<FDCPAutocompleteResult>(controller.AutocompleteDatabase(directory.Object, term));

            Assert.Equal(0, result.Total);
            directory.Verify(d => d.Query(), Times.Never);
        }

        [Fact]
        public void Database_ClampsPageSizeAndDepth()
        {
            using var controller = CreateController();

            Assert.Equal(AutocompleteSampleDirectory.MaxPageSize, Database(controller, "an", 0, 100000, "en").Items.Count);
            Assert.Single(Database(controller, "an", 0, -5, "en").Items);
            Assert.Equal(Database(controller, "an", 0, 10, "en").Items, Database(controller, "an", -99, 10, "en").Items);
            Assert.Empty(Database(controller, "an", int.MaxValue, 10, "en").Items);
        }

        /* ---------- GraphQL (mock) ---------- */

        [Fact]
        public void GraphQL_ReturnsDataEnvelope()
        {
            using var controller = CreateController();
            var response = controller.AutocompleteGraphQL(Directory, Request("tremblay", 0, 5, "fr"));

            using var json = ToJson(response);
            var employees = json.RootElement.GetProperty("data").GetProperty("employees");

            Assert.False(json.RootElement.TryGetProperty("errors", out _));
            Assert.True(employees.GetProperty("total").GetInt32() > 5);
            Assert.Equal(5, employees.GetProperty("items").GetArrayLength());
            Assert.Contains("Tremblay", employees.GetProperty("items")[0].GetProperty("text").GetString(), StringComparison.Ordinal);
            Assert.Matches("^E[0-9]{6}$", employees.GetProperty("items")[0].GetProperty("value").GetString());
        }

        [Fact]
        public void GraphQL_ReadsVariablesAndPassesThemToTheResolver()
        {
            var directory = new Mock<IAutocompleteSampleDirectory>(MockBehavior.Strict);
            directory.Setup(d => d.Search("roy", 50, 25, "fr")).Returns(new FDCPAutocompleteResult(0, []));

            using var controller = CreateController();
            controller.AutocompleteGraphQL(directory.Object, Request("roy", 50, 25, "fr"));

            directory.Verify(d => d.Search("roy", 50, 25, "fr"), Times.Once);
        }

        [Fact]
        public void GraphQL_MissingOrWrongTypedVariables_FallBackToDefaults()
        {
            var directory = new Mock<IAutocompleteSampleDirectory>(MockBehavior.Strict);
            directory.Setup(d => d.Search(null, 0, 50, null)).Returns(new FDCPAutocompleteResult(0, []));

            using var controller = CreateController();
            controller.AutocompleteGraphQL(directory.Object, new AutocompleteGraphQLRequest { Query = GraphQLQuery });
            controller.AutocompleteGraphQL(directory.Object, new AutocompleteGraphQLRequest
            {
                Query = GraphQLQuery,
                Variables = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>("{\"term\":42,\"skip\":\"ten\",\"take\":1.5,\"lang\":[\"fr\"]}")
            });

            directory.Verify(d => d.Search(null, 0, 50, null), Times.Exactly(2));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("{ __schema { types { name } } }")]
        [InlineData("mutation { deleteEverything }")]
        public void GraphQL_UnknownDocument_ReturnsErrorsAndRunsNothing(string? query)
        {
            var directory = new Mock<IAutocompleteSampleDirectory>(MockBehavior.Strict);

            using var controller = CreateController();
            using var json = ToJson(controller.AutocompleteGraphQL(directory.Object, query == null ? null : new AutocompleteGraphQLRequest { Query = query }));

            Assert.False(json.RootElement.TryGetProperty("data", out _));
            Assert.False(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("errors")[0].GetProperty("message").GetString()));
        }

        /* ---------- Existing API with a different shape ---------- */

        [Fact]
        public void Legacy_ReturnsItsOwnShapeWithBothLanguages()
        {
            using var controller = CreateController();
            using var json = ToJson(controller.AutocompleteLegacy(Directory, "tremblay", 2, 10));
            var root = json.RootElement;
            var first = root.GetProperty("results")[0];

            Assert.True(root.GetProperty("count").GetInt32() > 20);
            Assert.Equal(2, root.GetProperty("page").GetInt32());
            Assert.Equal(10, root.GetProperty("pageSize").GetInt32());
            Assert.Equal(10, root.GetProperty("results").GetArrayLength());
            Assert.Matches("^E[0-9]{6}$", first.GetProperty("id").GetString());
            Assert.Contains("Tremblay", first.GetProperty("displayName").GetString(), StringComparison.Ordinal);
            Assert.NotEqual(first.GetProperty("titleEn").GetString(), first.GetProperty("titleFr").GetString());
            Assert.False(root.TryGetProperty("items", out _));
            Assert.False(root.TryGetProperty("total", out _));
        }

        [Fact]
        public void Legacy_ClampsPageAndPageSize()
        {
            using var controller = CreateController();

            using var huge = ToJson(controller.AutocompleteLegacy(Directory, "an", -3, 100000));
            Assert.Equal(1, huge.RootElement.GetProperty("page").GetInt32());
            Assert.Equal(AutocompleteSampleDirectory.MaxPageSize, huge.RootElement.GetProperty("pageSize").GetInt32());

            using var deep = ToJson(controller.AutocompleteLegacy(Directory, "an", int.MaxValue, 50));
            Assert.Equal((AutocompleteSampleDirectory.MaxSkip / 50) + 1, deep.RootElement.GetProperty("page").GetInt32());
        }

        /* ---------- Hostile input, every source ---------- */

        [Fact]
        public void AllSources_GeneratedGarbage_NeverThrow()
        {
            var faker = AutocompleteFakes.Create();
            using var controller = CreateController();

            for (int sample = 0; sample < 200; sample++)
            {
                string term = faker.PickRandom(
                    faker.Random.String(faker.Random.Int(0, 200)),
                    faker.PickRandom(AutocompleteFakes.Payloads),
                    $"{faker.Name.LastName()}'; DROP TABLE employees;--",
                    faker.Lorem.Paragraph());
                int skip = faker.Random.Int();
                int take = faker.Random.Int();

                Assert.True(WebApi(controller, term, skip, take, "en").Items.Count <= AutocompleteSampleDirectory.MaxPageSize);
                Assert.True(Database(controller, term, skip, take, "en").Items.Count <= AutocompleteSampleDirectory.MaxPageSize);
                Assert.True(GraphQL(controller, term, skip, take, "en").Items.Count <= AutocompleteSampleDirectory.MaxPageSize);
                Assert.True(Legacy(controller, term, skip, take, "en").Items.Count <= AutocompleteSampleDirectory.MaxPageSize);
            }
        }

        /* ---------- Helpers: call each source and normalize to the component's contract ---------- */

        private static FDCPAutocompleteResult WebApi(ComponentsController controller, string? term, int skip, int take, string lang)
        {
            return Read<FDCPAutocompleteResult>(controller.AutocompleteData(Directory, term, skip, take, lang));
        }

        private static FDCPAutocompleteResult Database(ComponentsController controller, string? term, int skip, int take, string lang)
        {
            return Read<FDCPAutocompleteResult>(controller.AutocompleteDatabase(Directory, term, skip, take, lang));
        }

        private static FDCPAutocompleteResult GraphQL(ComponentsController controller, string? term, int skip, int take, string lang)
        {
            using var json = ToJson(controller.AutocompleteGraphQL(Directory, Request(term, skip, take, lang)));

            return json.RootElement.GetProperty("data").GetProperty("employees").Deserialize<FDCPAutocompleteResult>(WebJson)!;
        }

        /// <summary>Applies the same mapping as the page script of the "existing API" sample.</summary>
        private static FDCPAutocompleteResult Legacy(ComponentsController controller, string? term, int page, int pageSize, string lang)
        {
            using var json = ToJson(controller.AutocompleteLegacy(Directory, term, page, pageSize));
            var root = json.RootElement;

            return new FDCPAutocompleteResult(
                root.GetProperty("count").GetInt32(),
                [.. root.GetProperty("results").EnumerateArray().Select(person => new FDCPAutocompleteItem(
                    person.GetProperty("id").GetString()!,
                    person.GetProperty("displayName").GetString()!,
                    person.GetProperty(lang == "fr" ? "titleFr" : "titleEn").GetString()))]);
        }

        private static AutocompleteGraphQLRequest Request(string? term, int skip, int take, string lang)
        {
            // Built from JSON, as the model binder would from the request body.
            string body = JsonSerializer.Serialize(new { query = GraphQLQuery, variables = new { term, skip, take, lang } });

            return JsonSerializer.Deserialize<AutocompleteGraphQLRequest>(body, WebJson)!;
        }

        private static T Read<T>(IActionResult response)
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(Assert.IsType<JsonResult>(response).Value, WebJson), WebJson)!;
        }

        private static JsonDocument ToJson(IActionResult response)
        {
            return JsonDocument.Parse(JsonSerializer.Serialize(Assert.IsType<JsonResult>(response).Value, WebJson));
        }

        private static ComponentsController CreateController()
        {
            return new ComponentsController(Mock.Of<ILogger<ComponentsController>>(), Mock.Of<ITagHelperCatalogService>());
        }
    }
}
