using GCFoundation.Components.Models;

namespace GCFoundation.Web.Services.Interfaces
{
    /// <summary>
    /// One fictional person in the sample directory, shaped like a database row.
    /// </summary>
    /// <param name="Id">The unique identifier.</param>
    /// <param name="Name">The display name, "First Last".</param>
    /// <param name="SearchName">
    /// The name prepared for searching: lower case, accents removed, in both "first last" and "last, first" order.
    /// Storing this in its own indexed column keeps matching identical on every database, whatever its collation.
    /// </param>
    /// <param name="DescriptionEn">Title and branch in English.</param>
    /// <param name="DescriptionFr">Title and branch in French.</param>
    public sealed record AutocompleteSamplePerson(string Id, string Name, string SearchName, string DescriptionEn, string DescriptionFr);

    /// <summary>
    /// A searchable directory of fictional people used by the <c>fdcp-autocomplete</c> documentation samples.
    /// </summary>
    public interface IAutocompleteSampleDirectory
    {
        /// <summary>
        /// Gets the number of records in the directory.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Returns one page of records whose name contains <paramref name="term"/>, ignoring case and accents.
        /// </summary>
        /// <param name="term">The text typed by the user.</param>
        /// <param name="skip">The number of matches to skip.</param>
        /// <param name="take">The number of matches to return.</param>
        /// <param name="lang">The language of the descriptions, <c>en</c> or <c>fr</c>.</param>
        /// <returns>The requested page and the total number of matches.</returns>
        FDCPAutocompleteResult Search(string? term, int skip, int take, string? lang);

        /// <summary>
        /// Finds a single record by its value, so a posted value can be validated or displayed.
        /// </summary>
        /// <param name="value">The record value.</param>
        /// <param name="lang">The language of the description, <c>en</c> or <c>fr</c>.</param>
        /// <returns>The record, or <c>null</c> when the value is unknown.</returns>
        FDCPAutocompleteItem? Find(string? value, string? lang);

        /// <summary>
        /// Exposes the records as a queryable set, the way an Entity Framework <c>DbSet</c> would be,
        /// for samples that build their own query.
        /// </summary>
        /// <returns>The records, ordered by name.</returns>
        IQueryable<AutocompleteSamplePerson> Query();
    }
}
