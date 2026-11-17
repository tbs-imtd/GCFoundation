namespace GCFoundation.Components.Models
{
    /// <summary>
    /// One page of suggestions returned by an <c>fdcp-autocomplete</c> data source.
    /// </summary>
    /// <param name="Total">The total number of matches for the search term, across all pages.</param>
    /// <param name="Items">The matches for the requested page.</param>
    public sealed record FDCPAutocompleteResult(int Total, IReadOnlyList<FDCPAutocompleteItem> Items);
}
