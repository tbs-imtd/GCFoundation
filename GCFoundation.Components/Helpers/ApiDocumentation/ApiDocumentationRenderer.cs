using System.Globalization;
using System.Resources;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using static GCFoundation.Components.Helpers.ApiDocumentation.OpenApiDocumentationReader;

namespace GCFoundation.Components.Helpers.ApiDocumentation;

internal sealed class ApiDocumentationRenderer(string id, OpenApiDocumentationReader? document)
{
    private static readonly ResourceManager Resources = new("GCFoundation.Components.Resources.ApiDocumentation", typeof(ApiDocumentationRenderer).Assembly);
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly StringBuilder content = new();
    private readonly List<(string Id, string Title, bool Endpoint)> navigation = [];
    private readonly HashSet<string> usedIds = new(StringComparer.Ordinal);

    private static string H(string? value) => HtmlEncoder.Default.Encode(value ?? string.Empty);
    private static string L(string key) => Resources.GetString(key, CultureInfo.CurrentUICulture) ?? key;

    internal string Render(string? title, string? downloadUrl, List<ApiDocumentationSection> sections)
    {
        var root = document?.Root ?? default;
        var info = Get(root, "info");
        title = string.IsNullOrWhiteSpace(title) ? Text(info, "title") : title;
        if (string.IsNullOrWhiteSpace(title)) { title = L("Documentation"); }
        if (document != null)
        {
            StartSection("overview", L("Overview"));
            Paragraph(Text(info, "description"));
            if (Get(root, "servers").ValueKind == JsonValueKind.Array)
            {
                content.Append(CultureInfo.InvariantCulture, $"<h3>{H(L("Servers"))}</h3>");
                Servers(Get(root, "servers"));
            }
            if (!string.IsNullOrWhiteSpace(downloadUrl))
            {
                ValidateDownloadUrl(downloadUrl);
                content.Append(CultureInfo.InvariantCulture, $"<p class='api-docs__download bg-light mt-300 p-250'><a href='{H(downloadUrl)}' download>{H(L("Download"))} <span class='api-docs__file-type'>(JSON)</span></a></p>");
            }
            EndSection();
        }
        foreach (var section in sections.Where(s => !s.AfterEndpoints)) { CustomSection(section); }
        if (document != null)
        {
            var reference = new OpenApiReferenceRenderer(id, document).Render();
            content.Append(reference.Html);
            navigation.AddRange(reference.Navigation);
        }
        foreach (var section in sections.Where(s => s.AfterEndpoints)) { CustomSection(section); }

        var nav = new StringBuilder($"<nav class='api-docs__nav' aria-labelledby='{H(id)}-nav-title'><h2 id='{H(id)}-nav-title'>{H(title)}</h2>");
        if (Text(info, "version") is { Length: > 0 } version) { nav.Append(CultureInfo.InvariantCulture, $"<p class='api-docs__version'>{H(version)}</p>"); }
        nav.Append("<ul>");
        foreach (var entry in navigation)
        {
            nav.Append(CultureInfo.InvariantCulture, $"<li{(entry.Endpoint ? " class='api-docs__endpoint-link'" : "")}><a href='#{H(entry.Id)}'>{H(entry.Title)}</a></li>");
        }
        nav.Append("</ul></nav><div class='api-docs__content'>");
        nav.Append(content);
        nav.Append("</div>");
        return nav.ToString();
    }

    private void Servers(JsonElement servers)
    {
        foreach (var server in Items(servers))
        {
            content.Append(CultureInfo.InvariantCulture, $"<p><code>{H(Text(server, "url"))}</code> {H(Text(server, "description"))}</p>");
            if (Get(server, "variables").ValueKind != JsonValueKind.Undefined) { Details(Get(server, "variables"), L("Variables")); }
        }
    }

    private void CustomSection(ApiDocumentationSection section)
    {
        StartSection("section-" + (section.Id ?? section.Title), section.Title);
        // This content comes from the application's Razor pipeline, never from the OpenAPI document.
        content.Append(section.Html);
        EndSection();
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
    private static void ValidateDownloadUrl(string url)
    {
        if (url.Any(char.IsControl) || url.Contains('\\', StringComparison.Ordinal) || url.StartsWith("//", StringComparison.Ordinal)
            || !Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var parsed)
            || (parsed.IsAbsoluteUri && parsed.Scheme is not ("http" or "https"))
            || (!parsed.IsAbsoluteUri && url.Split('/')[0].Contains(':', StringComparison.Ordinal)))
        {
            throw new ArgumentException("The download URL must be an HTTP(S) or application-relative URL.", nameof(url));
        }
    }
}
