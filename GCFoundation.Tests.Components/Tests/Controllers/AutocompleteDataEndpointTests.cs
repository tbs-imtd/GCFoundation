using GCFoundation.Components.Models;
using GCFoundation.Tests.Components.Tests.TestData;
using GCFoundation.Web.Controllers;
using GCFoundation.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace GCFoundation.Tests.Components.Tests.Controllers
{
    public class AutocompleteDataEndpointTests
    {
        private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web);

        [Fact]
        public void AutocompleteData_PassesQueryToDirectoryAndReturnsItsResult()
        {
            var expected = new FDCPAutocompleteResult(1342, [new FDCPAutocompleteItem("E000001", "Marie Tremblay", "Analyst, Policy")]);
            var directory = new Mock<IAutocompleteSampleDirectory>(MockBehavior.Strict);
            directory.Setup(d => d.Search("trem", 50, 25, "fr")).Returns(expected);

            using var controller = CreateController();
            var result = Assert.IsType<JsonResult>(controller.AutocompleteData(directory.Object, "trem", 50, 25, "fr"));

            Assert.Same(expected, result.Value);
            directory.Verify(d => d.Search("trem", 50, 25, "fr"), Times.Once);
        }

        [Fact]
        public void AutocompleteData_SerializesToTheContractTheComponentReads()
        {
            var directory = new Mock<IAutocompleteSampleDirectory>();
            directory
                .Setup(d => d.Search(It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .Returns(new FDCPAutocompleteResult(2, [new FDCPAutocompleteItem("E1", "Zoé Côté", "Director, Finance"), new FDCPAutocompleteItem("E2", "Liam Roy")]));

            using var controller = CreateController();
            var result = Assert.IsType<JsonResult>(controller.AutocompleteData(directory.Object, "zo"));

            using var json = JsonDocument.Parse(JsonSerializer.Serialize(result.Value, WebJson));
            var root = json.RootElement;

            Assert.Equal(2, root.GetProperty("total").GetInt32());
            Assert.Equal("E1", root.GetProperty("items")[0].GetProperty("value").GetString());
            Assert.Equal("Zoé Côté", root.GetProperty("items")[0].GetProperty("text").GetString());
            Assert.Equal("Director, Finance", root.GetProperty("items")[0].GetProperty("description").GetString());
            Assert.Equal(JsonValueKind.Null, root.GetProperty("items")[1].GetProperty("description").ValueKind);
        }

        [Fact]
        public void AutocompleteData_FiveThousandGeneratedRecords_SurviveSerializationUnchanged()
        {
            // Faker names include accents, apostrophes and hyphens; none may be lost or altered on the wire.
            var items = AutocompleteFakes.Items(5000);
            var directory = new Mock<IAutocompleteSampleDirectory>();
            directory
                .Setup(d => d.Search(It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .Returns(new FDCPAutocompleteResult(items.Count, items));

            using var controller = CreateController();
            var result = Assert.IsType<JsonResult>(controller.AutocompleteData(directory.Object, "an"));
            var roundTripped = JsonSerializer.Deserialize<FDCPAutocompleteResult>(JsonSerializer.Serialize(result.Value, WebJson), WebJson);

            Assert.NotNull(roundTripped);
            Assert.Equal(5000, roundTripped.Total);
            Assert.Equal(items, roundTripped.Items);
            Assert.Equal(5000, roundTripped.Items.Select(item => item.Value).Distinct(StringComparer.Ordinal).Count());
            Assert.Contains(roundTripped.Items, item => item.Description == null);
            Assert.Contains(roundTripped.Items, item => item.Text.Any(character => character > 127));
        }

        [Fact]
        public void AutocompleteData_GeneratedQueries_ArePassedThroughUntouched()
        {
            var faker = AutocompleteFakes.Create();
            var directory = new Mock<IAutocompleteSampleDirectory>();
            directory
                .Setup(d => d.Search(It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .Returns(new FDCPAutocompleteResult(0, []));

            using var controller = CreateController();

            for (int sample = 0; sample < 200; sample++)
            {
                string term = faker.Random.Bool(0.2f) ? faker.PickRandom(AutocompleteFakes.Payloads) : AutocompleteFakes.SearchTerm(faker);
                int skip = faker.Random.Int(-10, 10000);
                int take = faker.Random.Int(-10, 1000);
                string lang = faker.PickRandom("en", "fr");

                controller.AutocompleteData(directory.Object, term, skip, take, lang);

                // The controller adds no logic of its own: limits are the directory's job, and are tested there.
                directory.Verify(d => d.Search(term, skip, take, lang), Times.AtLeastOnce);
            }
        }

        [Fact]
        public void AutocompleteData_UsesDefaultPaging()
        {
            var directory = new Mock<IAutocompleteSampleDirectory>();
            directory
                .Setup(d => d.Search(It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .Returns(new FDCPAutocompleteResult(0, []));

            using var controller = CreateController();
            controller.AutocompleteData(directory.Object, null);

            directory.Verify(d => d.Search(null, 0, 50, null), Times.Once);
        }

        [Fact]
        public void AutocompleteData_NullDirectory_Throws()
        {
            using var controller = CreateController();

            Assert.Throws<ArgumentNullException>(() => controller.AutocompleteData(null!, "trem"));
        }

        private static ComponentsController CreateController()
        {
            return new ComponentsController(Mock.Of<ILogger<ComponentsController>>(), Mock.Of<ITagHelperCatalogService>());
        }
    }
}
