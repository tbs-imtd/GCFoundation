using GCFoundation.Components.Helpers;
using GCFoundation.Components.Models.Code;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Net;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// Renders a single code snippet with syntax highlighting, an optional title and a copy button.
    /// The code is taken from the <c>code</c> attribute or, when it is omitted, from the child content,
    /// which is treated as HTML-encoded text.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fdcp-code-block id=&quot;response-example&quot; title=&quot;Example response&quot;
    ///                  language=&quot;json&quot; code=&quot;@Model.ResponseJson&quot; /&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("fdcp-code-block")]
    public class FDCPCodeBlockTagHelper : TagHelper
    {
        /// <summary>
        /// Source text to display. When <c>null</c>, the HTML-decoded child content is used, without its first
        /// and last blank lines and common indentation.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Whether the copy button is rendered. Defaults to <c>true</c>.
        /// </summary>
        public bool CopyEnabled { get; set; } = true;

        /// <summary>
        /// Identifier of the rendered component. Generated when omitted.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Language identifier, such as <c>csharp</c>, <c>javascript</c>, <c>python</c> or <c>json</c>.
        /// Unknown or empty languages are displayed as plain text.
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// Display name of the language. Derived from <see cref="Language"/> when omitted.
        /// </summary>
        public string? LanguageLabel { get; set; }

        /// <summary>
        /// Optional visible title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Whether long lines wrap instead of scrolling horizontally. Defaults to <c>false</c>.
        /// </summary>
        public bool Wrap { get; set; }

        /// <inheritdoc/>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output);

            var code = Code;
            if (code is null)
            {
                var childContent = await output.GetChildContentAsync().ConfigureAwait(false);
                code = Dedent(WebUtility.HtmlDecode(childContent.GetContent()));
            }

            var html = FDCPCodeRenderer.Render(new CodeBlockOptions
            {
                CopyEnabled = CopyEnabled,
                CssClass = output.Attributes["class"]?.Value?.ToString(),
                Id = Id,
                Samples = [new CodeSample { Language = Language ?? string.Empty, Label = LanguageLabel, Code = code }],
                Title = Title,
                Wrap = Wrap
            });

            output.TagName = null;
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Clear();
            output.Content.SetHtmlContent(html);
        }

        internal static string Dedent(string content)
        {
            var lines = content.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n').ToList();
            while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[0])) lines.RemoveAt(0);
            while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1])) lines.RemoveAt(lines.Count - 1);

            var indent = lines
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Length - l.TrimStart(' ', '\t').Length)
                .DefaultIfEmpty(0)
                .Min();

            return string.Join('\n', lines.Select(l => l.Length >= indent ? l[indent..] : l.TrimStart(' ', '\t')));
        }
    }
}
