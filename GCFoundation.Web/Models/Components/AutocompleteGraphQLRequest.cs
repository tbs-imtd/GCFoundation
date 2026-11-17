using System.Text.Json;

namespace GCFoundation.Web.Models.Components
{
    /// <summary>
    /// The standard GraphQL-over-HTTP request body, as posted by the Autocomplete GraphQL sample.
    /// </summary>
    public sealed class AutocompleteGraphQLRequest
    {
        /// <summary>
        /// Gets or sets the GraphQL document.
        /// </summary>
        public string? Query { get; set; }

        /// <summary>
        /// Gets or sets the variables referenced by the document.
        /// </summary>
#pragma warning disable CA2227 // Set by the JSON model binder.
        public Dictionary<string, JsonElement>? Variables { get; set; }
#pragma warning restore CA2227
    }
}
