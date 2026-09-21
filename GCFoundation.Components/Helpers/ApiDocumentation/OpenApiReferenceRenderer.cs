using System.Globalization;
using System.Resources;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using static GCFoundation.Components.Helpers.ApiDocumentation.OpenApiDocumentationReader;

namespace GCFoundation.Components.Helpers.ApiDocumentation;

internal sealed class OpenApiReferenceRenderer(string id, OpenApiDocumentationReader document)
{
    private static readonly ResourceManager Resources = new("GCFoundation.Components.Resources.ApiDocumentation", typeof(OpenApiReferenceRenderer).Assembly);
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly StringBuilder content = new();
    private readonly List<(string Id, string Title, bool Endpoint)> navigation = [];
    private readonly HashSet<string> usedIds = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> schemaIds = new(StringComparer.Ordinal);
    private readonly List<(string Id, string Title, JsonElement Schema)> schemas = [];

    private static string H(string? value) => HtmlEncoder.Default.Encode(value ?? string.Empty);
    private static string L(string key) => Resources.GetString(key, CultureInfo.CurrentUICulture) ?? key;

    internal ApiReferenceContent Render()
    {
        // Reserve schema anchors before rendering any links to reusable definitions.
        foreach (var schema in Properties(Get(Get(document.Root, "components"), "schemas")))
        {
            var schemaId = Anchor("schema-" + schema.Name);
            schemaIds[schema.Name] = schemaId;
            schemas.Add((schemaId, schema.Name, schema.Value));
        }
        var securitySchemes = Properties(Get(Get(document.Root, "components"), "securitySchemes")).ToList();
        if (securitySchemes.Count > 0)
        {
            StartSection("authentication", L("Authentication"));
            foreach (var scheme in securitySchemes)
            {
                content.Append(CultureInfo.InvariantCulture, $"<h3>{H(scheme.Name)}</h3>");
                Code(document.Resolve(scheme.Value), scheme.Name);
            }
            EndSection();
        }
        StartSection("endpoints", L("Endpoints"));
        if (document.Operations.Count == 0) { Paragraph(L("NoEndpoints")); }
        foreach (var operation in document.Operations) { Operation(operation); }
        EndSection();
        if (schemas.Count > 0)
        {
            StartSection("schemas", L("Schemas"));
            foreach (var schema in schemas)
            {
                content.Append(CultureInfo.InvariantCulture, $"<section aria-labelledby='{H(schema.Id)}'><h3 id='{H(schema.Id)}' tabindex='-1'>{H(schema.Title)}</h3>");
                Schema(schema.Schema, schema.Title);
                content.Append("</section>");
            }
            EndSection();
        }
        return new ApiReferenceContent(content.ToString(), navigation);
    }

    private void Operation(ApiDocumentationOperation operation)
    {
        var definition = operation.Definition;
        var operationId = Anchor("endpoint-" + operation.Method + "-" + operation.Path);
        var label = operation.Method + " " + operation.Path;
        navigation.Add((operationId, label, true));
        content.Append(CultureInfo.InvariantCulture, $"<section class='api-docs__endpoint' aria-labelledby='{H(operationId)}'><h3 id='{H(operationId)}' class='api-docs__endpoint-heading' tabindex='-1' data-api-section><span class='api-docs__method' data-method='{H(operation.Method)}'>{H(operation.Method)}</span><code>{H(operation.Path)}</code></h3>");
        Paragraph(Text(definition, "summary"));
        Paragraph(Text(definition, "description"));
        if (Get(definition, "deprecated").ValueKind == JsonValueKind.True) { Paragraph(L("Deprecated")); }
        if (Text(definition, "operationId") is { Length: > 0 } name) { Paragraph(L("OperationId") + ": " + name); }
        var servers = Get(definition, "servers");
        if (servers.ValueKind == JsonValueKind.Undefined) { servers = Get(operation.PathItem, "servers"); }
        if (servers.ValueKind == JsonValueKind.Array)
        {
            Subheading(L("Servers"));
            Servers(servers);
        }
        Subheading(L("Authentication"));
        var security = Get(definition, "security");
        if (security.ValueKind == JsonValueKind.Undefined) { security = Get(document!.Root, "security"); }
        if (!Items(security).Any()) { Paragraph(L("NoAuthentication")); }
        else
        {
            Paragraph(L("SecurityHelp"));
            Code(security, label + " — " + L("Authentication"));
        }

        Subheading(L("Parameters"));
        var parameters = document!.Parameters(operation);
        if (parameters.Count == 0) { Paragraph(L("None")); }
        else
        {
            StartTable(label + " — " + L("Parameters"), L("Name"), L("Type"), L("Required"), L("Description"));
            foreach (var parameter in parameters)
            {
                content.Append(CultureInfo.InvariantCulture, $"<tr><th scope='row'><code>{H(Text(parameter, "name"))}</code> ({H(Text(parameter, "in"))})</th><td>{H(Type(Get(parameter, "schema")))}</td><td>{H(L(Get(parameter, "required").ValueKind == JsonValueKind.True || Text(parameter, "in") == "path" ? "Yes" : "No"))}</td><td>{H(Text(parameter, "description"))}");
                Details(parameter, L("Definition"));
                content.Append("</td></tr>");
            }
            EndTable();
        }
        var request = Get(definition, "requestBody");
        if (request.ValueKind != JsonValueKind.Undefined)
        {
            request = document.Resolve(request);
            Subheading(L("RequestBody"));
            Paragraph(L("Required") + ": " + L(Get(request, "required").ValueKind == JsonValueKind.True ? "Yes" : "No"));
            Paragraph(Text(request, "description"));
            MediaTypes(Get(request, "content"), label + " — " + L("RequestBody"));
        }
        Subheading(L("Responses"));
        var responses = Properties(Get(definition, "responses")).Where(p => !p.Name.StartsWith("x-", StringComparison.Ordinal)).ToList();
        if (responses.Count == 0) { Paragraph(L("None")); }
        foreach (var response in responses)
        {
            var value = document.Resolve(response.Value);
            content.Append(CultureInfo.InvariantCulture, $"<h5 class='api-docs__response-heading'><code>{H(response.Name)}</code> {H(Text(value, "description"))}</h5>");
            MediaTypes(Get(value, "content"), label + " — " + response.Name);
            if (Properties(Get(value, "headers")).Any())
            {
                foreach (var header in Properties(Get(value, "headers")))
                {
                    Details(document.Resolve(header.Value), L("Headers") + ": " + header.Name);
                }
            }
        }
        content.Append("</section>");
    }

    private void MediaTypes(JsonElement mediaTypes, string label)
    {
        foreach (var media in Properties(mediaTypes))
        {
            content.Append(CultureInfo.InvariantCulture, $"<p class='api-docs__content-type'>{H(L("ContentType"))}: <code>{H(media.Name)}</code></p>");
            if (Get(media.Value, "schema").ValueKind != JsonValueKind.Undefined) { Schema(Get(media.Value, "schema"), label + " — " + media.Name); }
            var example = Get(media.Value, "example");
            if (example.ValueKind != JsonValueKind.Undefined) { Details(example, L("Example")); }
            foreach (var namedExample in Properties(Get(media.Value, "examples")))
            {
                var resolved = document!.Resolve(namedExample.Value);
                Paragraph(Text(resolved, "summary"));
                Paragraph(Text(resolved, "description"));
                var value = Get(resolved, "value");
                if (value.ValueKind != JsonValueKind.Undefined) { Details(value, L("Example") + ": " + namedExample.Name); }
                else { Details(resolved, L("Example") + ": " + namedExample.Name); }
            }
            if (Get(media.Value, "encoding").ValueKind != JsonValueKind.Undefined) { Details(Get(media.Value, "encoding"), L("Encoding")); }
        }
    }

    private void Schema(JsonElement schema, string label)
    {
        // Preserve the actual JSON Schema, including compositions, constraints and recursive refs.
        var properties = Properties(Get(schema, "properties")).ToList();
        if (properties.Count > 0)
        {
            var required = Items(Get(schema, "required")).Where(v => v.ValueKind == JsonValueKind.String).Select(v => v.GetString()).ToHashSet(StringComparer.Ordinal);
            StartTable(label + " — " + L("Schema"), L("Name"), L("Type"), L("Required"), L("Description"));
            foreach (var property in properties)
            {
                content.Append(CultureInfo.InvariantCulture, $"<tr><th scope='row'><code>{H(property.Name)}</code></th><td>{H(Type(property.Value))}</td><td>{H(L(required.Contains(property.Name) ? "Yes" : "No"))}</td><td>{H(Text(property.Value, "description"))}</td></tr>");
            }
            EndTable();
        }
        Code(schema, label + " — " + L("Schema"));
        foreach (var reference in References(schema).Distinct(StringComparer.Ordinal))
        {
            const string prefix = "#/components/schemas/";
            var pointer = Uri.UnescapeDataString(reference);
            var name = pointer.StartsWith(prefix, StringComparison.Ordinal)
                ? pointer[prefix.Length..].Replace("~1", "/", StringComparison.Ordinal).Replace("~0", "~", StringComparison.Ordinal) : string.Empty;
            if (schemaIds.TryGetValue(name, out var schemaId))
            {
                content.Append(CultureInfo.InvariantCulture, $"<p>{H(L("Schema"))}: <a href='#{H(schemaId)}'><code>{H(name)}</code></a></p>");
            }
            else
            {
                Paragraph(L("Reference") + ": " + reference);
            }
        }
    }

    private static IEnumerable<string> References(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in value.EnumerateObject())
            {
                if (property.Name == "$ref" && property.Value.ValueKind == JsonValueKind.String) { yield return property.Value.GetString()!; }
                // Examples, defaults and extensions are instance data, not schemas.
                else if (property.Name is not ("example" or "examples" or "default" or "const" or "enum") && !property.Name.StartsWith("x-", StringComparison.Ordinal))
                {
                    foreach (var reference in References(property.Value)) { yield return reference; }
                }
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in value.EnumerateArray())
            {
                foreach (var reference in References(item)) { yield return reference; }
            }
        }
    }

    private static string Type(JsonElement schema)
    {
        var type = Get(schema, "type");
        var text = type.ValueKind == JsonValueKind.Array ? string.Join(" | ", Items(type).Select(t => t.ToString())) : Text(schema, "type");
        if (schema.ValueKind is JsonValueKind.True or JsonValueKind.False) { text = schema.GetRawText(); }
        if (string.IsNullOrEmpty(text)) { text = Text(schema, "$ref"); }
        if (string.IsNullOrEmpty(text)) { text = L("SeeSchema"); }
        if (Text(schema, "format") is { Length: > 0 } format) { text += " (" + format + ")"; }
        if (Get(schema, "nullable").ValueKind == JsonValueKind.True) { text += " | null"; }
        return text;
    }

    private void Servers(JsonElement servers)
    {
        foreach (var server in Items(servers))
        {
            content.Append(CultureInfo.InvariantCulture, $"<p><code>{H(Text(server, "url"))}</code> {H(Text(server, "description"))}</p>");
            if (Get(server, "variables").ValueKind != JsonValueKind.Undefined) { Details(Get(server, "variables"), L("Variables")); }
        }
    }

    private string Anchor(string suffix)
    {
        var safe = new string(suffix.Select(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' ? c : '-').ToArray());
        var baseId = id + "-" + safe;
        var result = baseId;
        var index = 2;
        while (!usedIds.Add(result)) { result = baseId + "-" + (index++).ToString(CultureInfo.InvariantCulture); }
        return result;
    }

    private void StartSection(string suffix, string title)
    {
        var sectionId = Anchor(suffix);
        navigation.Add((sectionId, title, false));
        content.Append(CultureInfo.InvariantCulture, $"<section class='api-docs__section' aria-labelledby='{H(sectionId)}'><h2 id='{H(sectionId)}' tabindex='-1' data-api-section>{H(title)}</h2>");
    }

    private void EndSection() => content.Append("</section>");
    private void Subheading(string title) => content.Append(CultureInfo.InvariantCulture, $"<h4>{H(title)}</h4>");
    private void Paragraph(string text)
    {
        if (!string.IsNullOrWhiteSpace(text)) { content.Append(CultureInfo.InvariantCulture, $"<p class='api-docs__description'>{H(text)}</p>"); }
    }
    private void Code(JsonElement value, string label) => content.Append(CultureInfo.InvariantCulture, $"<pre tabindex='0' aria-label='{H(label)}'><code>{H(JsonSerializer.Serialize(value, JsonOptions))}</code></pre>");
    private void Details(JsonElement value, string title)
    {
        content.Append(CultureInfo.InvariantCulture, $"<details><summary>{H(title)}</summary>");
        Code(value, title);
        content.Append("</details>");
    }
    private void StartTable(string label, params string[] headers)
    {
        content.Append(CultureInfo.InvariantCulture, $"<div class='api-docs__table-wrap' role='region' tabindex='0' aria-label='{H(label)}'><table class='api-docs__table'><caption class='visibility-sr-only'>{H(label)}</caption><thead><tr>");
        foreach (var header in headers) { content.Append(CultureInfo.InvariantCulture, $"<th scope='col'>{H(header)}</th>"); }
        content.Append("</tr></thead><tbody>");
    }
    private void EndTable() => content.Append("</tbody></table></div>");

}

internal sealed record ApiReferenceContent(string Html, IReadOnlyList<(string Id, string Title, bool Endpoint)> Navigation);
