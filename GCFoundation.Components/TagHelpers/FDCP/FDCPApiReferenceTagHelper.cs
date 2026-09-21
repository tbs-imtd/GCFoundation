using GCFoundation.Components.Helpers.ApiDocumentation;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Json;

namespace GCFoundation.Components.TagHelpers.FDCP;

/// <summary>Renders an OpenAPI reference without page navigation, downloads or authored sections.</summary>
[HtmlTargetElement("fdcp-api-reference")]
public class FDCPApiReferenceTagHelper : TagHelper
{
    /// <summary>Gets or sets the required OpenAPI 3.x JSON contents.</summary>
    public string Document { get; set; } = string.Empty;

    /// <summary>Gets or sets a unique prefix for generated reference anchors.</summary>
    public string? Id { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentException.ThrowIfNullOrWhiteSpace(Document);
        var referenceId = string.IsNullOrWhiteSpace(Id) ? $"fdcp-api-reference-{Guid.NewGuid():N}" : Id;
        if (referenceId.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '-' or '_')))
        {
            throw new InvalidOperationException("The API reference ID may contain only ASCII letters, digits, hyphens and underscores.");
        }
        using var json = JsonDocument.Parse(Document);
        var reader = new OpenApiDocumentationReader(json.RootElement);
        var reference = new OpenApiReferenceRenderer(referenceId, reader).Render();
        var existingClass = output.Attributes["class"]?.Value?.ToString();
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("id", referenceId);
        output.Attributes.SetAttribute("class", $"api-docs api-docs--reference {existingClass}".TrimEnd());
        output.Content.SetHtmlContent(reference.Html);
    }
}
