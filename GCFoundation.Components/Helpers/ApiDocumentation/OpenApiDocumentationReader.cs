using System.Text.Json;

namespace GCFoundation.Components.Helpers.ApiDocumentation;

internal sealed record ApiDocumentationSection(string? Id, string Title, string Html, bool AfterEndpoints);
internal sealed record ApiDocumentationOperation(string Method, string Path, JsonElement Definition, JsonElement PathItem);

/// <summary>
/// A presentation reader, not a validator. It never fetches URLs or recursively expands schemas.
/// </summary>
internal sealed class OpenApiDocumentationReader
{
    private static readonly HashSet<string> Methods = new(StringComparer.Ordinal)
        { "get", "put", "post", "delete", "options", "head", "patch", "trace" };
    private readonly bool supportsReferenceAnnotations;

    internal JsonElement Root { get; }
    internal List<ApiDocumentationOperation> Operations { get; } = [];

    internal OpenApiDocumentationReader(JsonElement root)
    {
        Root = root;
        var version = Text(root, "openapi").Split('.');
        if (version.Length != 3 || version[0] != "3" || !IsVersionNumber(version[1]) || !IsVersionNumber(version[2]))
        {
            throw new ArgumentException("API documentation requires OpenAPI 3.x JSON with an openapi version in the form 3.minor.patch (for example, 3.2.1). Convert YAML or Swagger 2 documents before passing them to fdcp-api-docs.");
        }
        supportsReferenceAnnotations = version[1] != "0";
        var supportsAdditionalOperations = version[1] is not ("0" or "1");
        if (Get(root, "info").ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException("The OpenAPI document must contain an info object.");
        }
        foreach (var path in Properties(Get(root, "paths")))
        {
            if (!path.Name.StartsWith('/')) { continue; }
            var pathItem = Resolve(path.Value);
            foreach (var operation in Properties(pathItem).Where(p => Methods.Contains(p.Name) || (supportsAdditionalOperations && p.Name == "query")))
            {
                Operations.Add(new(operation.Name.ToUpperInvariant(), path.Name, operation.Value, pathItem));
            }
            if (supportsAdditionalOperations)
            {
                foreach (var operation in Properties(Get(pathItem, "additionalOperations")))
                {
                    // Additional HTTP methods are case-sensitive; retain their supplied spelling.
                    Operations.Add(new(operation.Name, path.Name, operation.Value, pathItem));
                }
            }
        }
    }

    private static bool IsVersionNumber(string value) =>
        value.Length > 0 && (value.Length == 1 || value[0] != '0') && value.All(char.IsAsciiDigit);

    internal JsonElement Resolve(JsonElement value)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        while (Text(value, "$ref") is { Length: > 0 } reference)
        {
            if (!visited.Add(reference))
            {
                throw new ArgumentException($"Cyclic OpenAPI reference object: {reference}");
            }
            var original = value;
            value = ResolvePointer(reference);
            // OpenAPI 3.1 and later Reference Object siblings override the referenced annotation.
            if (supportsReferenceAnnotations
                && (Get(original, "description").ValueKind != JsonValueKind.Undefined
                    || Get(original, "summary").ValueKind != JsonValueKind.Undefined))
            {
                var merged = Properties(value).ToDictionary(p => p.Name, p => p.Value, StringComparer.Ordinal);
                foreach (var key in new[] { "description", "summary" })
                {
                    if (Get(original, key).ValueKind != JsonValueKind.Undefined) { merged[key] = Get(original, key); }
                }
                value = JsonSerializer.SerializeToElement(merged);
            }
        }
        return value;
    }

    internal JsonElement ResolvePointer(string reference)
    {
        if (!reference.StartsWith("#/", StringComparison.Ordinal))
        {
            throw new ArgumentException($"Bundle external references into the OpenAPI document before rendering: {reference}");
        }
        var value = Root;
        foreach (var segment in Uri.UnescapeDataString(reference[2..]).Split('/'))
        {
            var key = segment.Replace("~1", "/", StringComparison.Ordinal).Replace("~0", "~", StringComparison.Ordinal);
            if (value.ValueKind == JsonValueKind.Object && value.TryGetProperty(key, out var property))
            {
                value = property;
            }
            else if (value.ValueKind == JsonValueKind.Array && int.TryParse(key, out var index) && index >= 0 && index < value.GetArrayLength())
            {
                value = value[index];
            }
            else { throw new ArgumentException($"Unresolved OpenAPI reference: {reference}"); }
        }
        return value;
    }

    internal List<JsonElement> Parameters(ApiDocumentationOperation operation)
    {
        var parameters = new Dictionary<(string Name, string Location), JsonElement>();
        foreach (var parameter in Items(Get(operation.PathItem, "parameters")).Concat(Items(Get(operation.Definition, "parameters"))))
        {
            var resolved = Resolve(parameter);
            parameters[(Text(resolved, "name"), Text(resolved, "in"))] = resolved;
        }
        return parameters.Values.ToList();
    }

    internal static JsonElement Get(JsonElement value, string name) =>
        value.ValueKind == JsonValueKind.Object && value.TryGetProperty(name, out var result) ? result : default;

    internal static string Text(JsonElement value, string name) =>
        Get(value, name) is var result && result.ValueKind == JsonValueKind.String ? result.GetString()! : string.Empty;

    internal static IEnumerable<JsonProperty> Properties(JsonElement value) =>
        value.ValueKind == JsonValueKind.Object ? value.EnumerateObject() : [];

    internal static IEnumerable<JsonElement> Items(JsonElement value) =>
        value.ValueKind == JsonValueKind.Array ? value.EnumerateArray() : [];
}
