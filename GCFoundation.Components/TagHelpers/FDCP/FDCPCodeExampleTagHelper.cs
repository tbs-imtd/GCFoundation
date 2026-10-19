using GCFoundation.Components.Helpers;
using GCFoundation.Components.Models.Code;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// Renders the same example in several languages with a labelled language selector, syntax highlighting
    /// and a copy button. Examples sharing a <see cref="SyncGroup"/> switch language together.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fdcp-code-example id=&quot;create-request-example&quot; title=&quot;Create a request&quot;
    ///                    examples=&quot;@Model.RequestExamples&quot; selected-language=&quot;csharp&quot;
    ///                    sync-group=&quot;request-examples&quot; /&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("fdcp-code-example", Attributes = "examples")]
    public class FDCPCodeExampleTagHelper : TagHelper
    {
        /// <summary>
        /// Whether the copy button is rendered. Defaults to <c>true</c>.
        /// </summary>
        public bool CopyEnabled { get; set; } = true;

        /// <summary>
        /// Language variants of the example. Language identifiers must be unique.
        /// </summary>
        public IEnumerable<CodeSample> Examples { get; set; } = [];

        /// <summary>
        /// Identifier of the rendered component. Generated when omitted.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Language shown first. Defaults to the first example.
        /// </summary>
        public string? SelectedLanguage { get; set; }

        /// <summary>
        /// Optional group name. Examples with the same group on a page switch language together; an example
        /// without the chosen language keeps its current selection.
        /// </summary>
        public string? SyncGroup { get; set; }

        /// <summary>
        /// Optional visible title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Whether long lines wrap instead of scrolling horizontally. Defaults to <c>false</c>.
        /// </summary>
        public bool Wrap { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output);

            var html = FDCPCodeRenderer.Render(new CodeBlockOptions
            {
                CopyEnabled = CopyEnabled,
                CssClass = output.Attributes["class"]?.Value?.ToString(),
                Id = Id,
                Samples = (Examples ?? []).ToList(),
                SelectedLanguage = SelectedLanguage,
                SyncGroup = SyncGroup,
                Title = Title,
                Wrap = Wrap
            });

            output.TagName = null;
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Clear();
            output.Content.SetHtmlContent(html);
        }
    }
}
