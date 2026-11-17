using GCFoundation.Web.Services;

namespace GCFoundation.Tests.Components.Tests.Services
{
    public class AutocompleteSampleDirectoryTests
    {
        // Generating 5,000 records is cheap, but one shared instance keeps the suite fast.
        private static readonly AutocompleteSampleDirectory Directory = new();

        [Fact]
        public void Constructor_Default_GeneratesFiveThousandRecords()
        {
            Assert.Equal(5000, Directory.Count);
        }

        [Fact]
        public void Constructor_SameSeed_GeneratesIdenticalRecords()
        {
            var first = new AutocompleteSampleDirectory(2000, 7).Search("an", 0, 100, "en");
            var second = new AutocompleteSampleDirectory(2000, 7).Search("an", 0, 100, "en");

            Assert.Equal(first.Total, second.Total);
            Assert.Equal(first.Items, second.Items);
        }

        [Fact]
        public void Constructor_DifferentSeed_GeneratesDifferentRecords()
        {
            var first = new AutocompleteSampleDirectory(2000, 7).Search("an", 0, 100, "en");
            var second = new AutocompleteSampleDirectory(2000, 8).Search("an", 0, 100, "en");

            Assert.NotEqual(first.Items, second.Items);
        }

        [Fact]
        public void Records_HaveUniqueValues()
        {
            var values = new List<string>();
            int total = Directory.Search("an", 0, 1, "en").Total;

            for (int skip = 0; skip < total; skip += AutocompleteSampleDirectory.MaxPageSize)
            {
                values.AddRange(Directory.Search("an", skip, AutocompleteSampleDirectory.MaxPageSize, "en").Items.Select(item => item.Value));
            }

            Assert.Equal(total, values.Count);
            Assert.Equal(values.Count, values.Distinct(StringComparer.Ordinal).Count());
            Assert.All(values, value => Assert.Matches("^E[0-9]{6}$", value));
        }

        [Theory]
        [InlineData("cote")]
        [InlineData("CÔTÉ")]
        [InlineData("côté")]
        [InlineData("  Cote  ")]
        public void Search_IgnoresCaseAccentsAndOuterWhitespace(string term)
        {
            var expected = Directory.Search("Côté", 0, 100, "en");
            var actual = Directory.Search(term, 0, 100, "en");

            Assert.True(expected.Total > 0);
            Assert.Equal(expected.Total, actual.Total);
            Assert.All(actual.Items, item => Assert.Contains("Côté", item.Text, StringComparison.Ordinal));
        }

        [Theory]
        [InlineData("Côté", "cote")]
        [InlineData("  CÔTÉ   Lévesque ", "cote levesque")]
        [InlineData("Cœur", "coeur")]
        [InlineData("ŒUVRE", "oeuvre")]
        [InlineData("Lætitia", "laetitia")]
        [InlineData("Æsir", "aesir")]
        [InlineData("Søren Ødegård", "soren odegard")]
        [InlineData("Straße", "strasse")]
        [InlineData("\u1E9E", "ss")]
        [InlineData("Łukasz Wałęsa", "lukasz walesa")]
        [InlineData("Noël Çelik Ñúñez", "noel celik nunez")]
        public void NormalizeTerm_FoldsCaseAccentsAndSpecialLetters(string input, string expected)
        {
            Assert.Equal(expected, AutocompleteSampleDirectory.NormalizeTerm(input));
        }

        [Fact]
        public void Search_MatchesFirstLastAndLastFirstOrder()
        {
            var firstLast = Directory.Search("marie tremblay", 0, 100, "en");
            var lastFirst = Directory.Search("tremblay, marie", 0, 100, "en");

            Assert.True(firstLast.Total > 0);
            Assert.Equal(firstLast.Total, lastFirst.Total);
            Assert.All(lastFirst.Items, item => Assert.Equal("Marie Tremblay", item.Text));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("a")]
        [InlineData("é")]
        public void Search_BelowMinimumLength_ReturnsNothing(string? term)
        {
            var result = Directory.Search(term, 0, 50, "en");

            Assert.Equal(0, result.Total);
            Assert.Empty(result.Items);
        }

        [Theory]
        [InlineData("ab�")]
        [InlineData("�cd")]
        public void Search_MalformedUnicode_ReturnsNothingInsteadOfThrowing(string term)
        {
            var result = Directory.Search(term, 0, 50, "en");

            Assert.Equal(0, result.Total);
            Assert.Empty(result.Items);
        }

        [Fact]
        public void Search_OverlongTerm_ReturnsNothing()
        {
            var result = Directory.Search(new string('a', AutocompleteSampleDirectory.MaxTermLength + 1), 0, 50, "en");

            Assert.Equal(0, result.Total);
        }

        [Theory]
        [InlineData(int.MaxValue)]
        [InlineData(101)]
        public void Search_ClampsPageSizeToMaximum(int take)
        {
            var result = Directory.Search("an", 0, take, "en");

            Assert.True(result.Total > AutocompleteSampleDirectory.MaxPageSize);
            Assert.Equal(AutocompleteSampleDirectory.MaxPageSize, result.Items.Count);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void Search_NonPositivePageSize_ReturnsOneItem(int take)
        {
            Assert.Single(Directory.Search("an", 0, take, "en").Items);
        }

        [Fact]
        public void Search_Pages_AreContiguousAndDoNotOverlap()
        {
            var all = Directory.Search("an", 0, 100, "en");
            var firstHalf = Directory.Search("an", 0, 50, "en");
            var secondHalf = Directory.Search("an", 50, 50, "en");

            Assert.Equal(all.Total, firstHalf.Total);
            Assert.Equal(all.Items, firstHalf.Items.Concat(secondHalf.Items));
        }

        [Fact]
        public void Search_NegativeSkip_IsTreatedAsZero()
        {
            Assert.Equal(Directory.Search("an", 0, 10, "en").Items, Directory.Search("an", -50, 10, "en").Items);
        }

        [Fact]
        public void Search_SkipPastEnd_ReturnsTotalButNoItems()
        {
            var result = Directory.Search("an", int.MaxValue, 50, "en");

            Assert.True(result.Total > 0);
            Assert.Empty(result.Items);
        }

        [Fact]
        public void Search_Results_AreSortedByName()
        {
            var keys = Directory.Search("an", 0, 100, "en").Items.Select(item => AutocompleteSampleDirectory.NormalizeTerm(item.Text)).ToList();

            Assert.Equal(keys.OrderBy(key => key, StringComparer.Ordinal), keys);
        }

        [Fact]
        public void Search_Language_SelectsDescriptionLanguageOnly()
        {
            var english = Directory.Search("tremblay", 0, 100, "en");
            var french = Directory.Search("tremblay", 0, 100, "fr-CA");

            Assert.Equal(english.Items.Select(item => item.Value), french.Items.Select(item => item.Value));
            Assert.Equal(english.Items.Select(item => item.Text), french.Items.Select(item => item.Text));
            Assert.NotEqual(english.Items.Select(item => item.Description), french.Items.Select(item => item.Description));
        }

        [Fact]
        public void Find_KnownValue_ReturnsRecord()
        {
            var match = Directory.Search("tremblay", 0, 1, "en").Items.Single();

            Assert.Equal(match, Directory.Find(match.Value, "en"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("E999999")]
        [InlineData("e000001")]
        [InlineData("' OR 1=1 --")]
        public void Find_UnknownValue_ReturnsNull(string? value)
        {
            Assert.Null(Directory.Find(value, "en"));
        }

        [Fact]
        public void Constructor_NegativeCount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AutocompleteSampleDirectory(-1, 1));
        }

        [Fact]
        public void Search_EmptyDirectory_ReturnsNothing()
        {
            Assert.Equal(0, new AutocompleteSampleDirectory(0, 1).Search("an", 0, 10, "en").Total);
        }
    }
}
