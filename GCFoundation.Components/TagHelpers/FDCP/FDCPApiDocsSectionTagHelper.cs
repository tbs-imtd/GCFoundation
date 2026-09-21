using GCFoundation.Components.Helpers.ApiDocumentation;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.FDCP;

/// <summary>Adds an authored Razor section to API documentation and its contents navigation.</summary>
[HtmlTargetElement("fdcp-api-docs-section", ParentTag = "fdcp-api-docs")]
public class FDCPApiDocsSectionTagHelper : TagHelper
{
    /// <summary>Gets or sets the section heading and navigation label.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets a stable anchor suffix, prefixed by the parent component ID.</summary>
    public string? Id { get; set; }

    /// <summary>Gets or sets whether the section appears after the generated reference.</summary>
    public bool AfterEndpoints { get; set; }

    /// <inheritdoc/>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentException.ThrowIfNullOrWhiteSpace(Title);
        if (!context.Items.TryGetValue(typeof(FDCPApiDocsTagHelper), out var value)
            || value is not List<ApiDocumentationSection> sections)
        {
            throw new InvalidOperationException("fdcp-api-docs-section must be a child of fdcp-api-docs.");
        }

#pragma warning disable CA2007 // Execute child Razor content in its rendering context.
        var content = await output.GetChildContentAsync();
#pragma warning restore CA2007
        sections.Add(new ApiDocumentationSection(Id, Title, content.GetContent(), AfterEndpoints));
        output.SuppressOutput();
    }
}
