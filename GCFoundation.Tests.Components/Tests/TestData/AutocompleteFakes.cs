using Bogus;
using GCFoundation.Components.Models;

namespace GCFoundation.Tests.Components.Tests.TestData
{
    /// <summary>
    /// Bogus (Faker) generators for autocomplete tests. Every generator is seeded, so a failing test can be re-run with the same data.
    /// </summary>
    internal static class AutocompleteFakes
    {
        public const int Seed = 20251117;

        /// <summary>Markup and attribute-breaking strings mixed into generated text by the encoding tests.</summary>
        public static readonly string[] Payloads =
        [
            "<script>alert(1)</script>",
            "\"><img src=x onerror=alert(1)>",
            "' onfocus='alert(1)",
            "</label><svg onload=alert(1)>",
            "javascript:alert(1)",
            "&lt;already-encoded&gt;",
            "{{constructor.constructor('alert(1)')()}}"
        ];

        /// <summary>A seeded Faker. French by default so names carry accents, apostrophes and hyphens.</summary>
        public static Faker Create(int seed = Seed, string locale = "fr")
        {
            return new Faker(locale) { Random = new Randomizer(seed) };
        }

        /// <summary>Generates fictional people as autocomplete items.</summary>
        public static List<FDCPAutocompleteItem> Items(int count, int seed = Seed, string locale = "fr")
        {
            return new Faker<FDCPAutocompleteItem>(locale)
                .UseSeed(seed)
                .CustomInstantiator(f => new FDCPAutocompleteItem(
                    $"E{f.IndexFaker + 1:D6}",
                    f.Name.FullName(),
                    f.Random.Bool(0.8f) ? $"{f.Name.JobTitle()}, {f.Commerce.Department()}" : null))
                .Generate(count);
        }

        /// <summary>A plausible search term: a slice of a generated name, in random case, sometimes padded.</summary>
        public static string SearchTerm(Faker faker)
        {
            string name = faker.Random.Bool() ? faker.Name.LastName() : faker.Name.FirstName();
            int length = faker.Random.Int(2, Math.Max(2, Math.Min(6, name.Length)));
            int start = faker.Random.Int(0, Math.Max(0, name.Length - length));
            string term = name.Substring(start, Math.Min(length, name.Length - start));

            term = faker.Random.Bool() ? term.ToUpperInvariant() : term.ToLowerInvariant();
            return faker.Random.Bool(0.2f) ? $"  {term} " : term;
        }
    }
}
