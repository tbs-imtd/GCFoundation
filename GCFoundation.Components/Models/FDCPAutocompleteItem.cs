namespace GCFoundation.Components.Models
{
    /// <summary>
    /// A single suggestion returned by an <c>fdcp-autocomplete</c> data source.
    /// </summary>
    /// <param name="Value">The value submitted with the form when the item is selected.</param>
    /// <param name="Text">The text shown in the list and in the input once selected.</param>
    /// <param name="Description">Optional secondary line used to tell similar items apart.</param>
    public sealed record FDCPAutocompleteItem(string Value, string Text, string? Description = null);
}
