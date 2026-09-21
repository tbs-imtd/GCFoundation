using GCFoundation.Components.Helpers.ApiDocumentation;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Json;

namespace GCFoundation.Components.TagHelpers.FDCP;

/// <summary>Composes the API documentation page body with navigation, authored sections and the shared OpenAPI reference renderer.</summary>
[HtmlTargetElement("fdcp-api-docs")]
public class FDCPApiDocsTagHelper : TagHelper
{
    /// <summary>Gets or sets OpenAPI 3.x JSON. Omit to render only authored sections.</summary>
    public string? Document { get; set; }

    /// <summary>Gets or sets a unique component ID, used as a prefix for all generated anchors.</summary>
    public string? Id { get; set; }

    /// <summary>Gets or sets a title overriding the document's info.title.</summary>
    public string? Title { get; set; }

    /// <summary>Gets or sets an optional HTTP(S) or application-relative specification download URL.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "Razor URL strings are validated before rendering; preserve application-relative URLs from Url.Content.")]
    public string? DownloadUrl { get; set; }

    /// <inheritdoc/>
    public override void Init(TagHelperContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Items[typeof(FDCPApiDocsTagHelper)] = new List<ApiDocumentationSection>();
    }

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);
        if (!context.Items.TryGetValue(typeof(FDCPApiDocsTagHelper), out var sections))
        {
            throw new InvalidOperationException("Initialize fdcp-api-docs before rendering its children.");
        }

#pragma warning disable CA2007 // Execute child Razor content in its rendering context.
        await output.GetChildContentAsync();
#pragma warning restore CA2007

        using var json = Document is null ? null : JsonDocument.Parse(Document);
        var reader = json is null ? null : new OpenApiDocumentationReader(json.RootElement);
        var componentId = string.IsNullOrWhiteSpace(Id) ? $"fdcp-api-docs-{Guid.NewGuid():N}" : Id;
        if (componentId.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '-' or '_')))
        {
            throw new InvalidOperationException("The API documentation ID may contain only ASCII letters, digits, hyphens and underscores.");
        }

        var renderer = new ApiDocumentationRenderer(componentId, reader);
        var html = renderer.Render(Title, DownloadUrl, (List<ApiDocumentationSection>)sections);
        var existingClass = output.Attributes["class"]?.Value?.ToString();
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("id", componentId);
        output.Attributes.SetAttribute("class", $"api-docs {existingClass}".TrimEnd());
        output.Attributes.SetAttribute("data-fdcp-api-docs", "true");
        output.Content.SetHtmlContent(html);
    }
}
