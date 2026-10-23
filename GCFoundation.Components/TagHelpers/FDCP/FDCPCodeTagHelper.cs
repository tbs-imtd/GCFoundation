using GCFoundation.Components.Enums;
using GCFoundation.Components.Helpers;
using GCFoundation.Components.Models.Code;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Net;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// Renders code with syntax highlighting, an optional title and a copy button. Supply one snippet with
    /// <c>code</c> or child content, or several languages with <c>examples</c>. The language selector is shown
    /// only when there is more than one language. Set <c>theme</c> to <c>dark</c> for a dark background that
    /// Dark Reader will not invert.
    /// </summary>
    /// <example>
    /// <code>
    /// &lt;fdcp-code id=&quot;response-example&quot; title=&quot;Example response&quot;
    ///            language=&quot;json&quot; code=&quot;@Model.ResponseJson&quot; /&gt;
    ///
    /// &lt;fdcp-code id=&quot;create-request-example&quot; title=&quot;Create a request&quot;
    ///            examples=&quot;@Model.RequestExamples&quot; selected-language=&quot;csharp&quot;
    ///            sync-group=&quot;request-examples&quot; theme=&quot;dark&quot; /&gt;
    /// </code>
    /// </example>
    [HtmlTargetElement("fdcp-code")]
    public class FDCPCodeTagHelper : TagHelper
    {
        /// <summary>
        /// Source text for a single snippet. When <c>null</c> and <see cref="Examples"/> is omitted, the
        /// HTML-decoded child content is used, without its first and last blank lines and common indentation.
        /// Ignored when <see cref="Examples"/> is set.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Whether the copy button is rendered. Defaults to <c>true</c>.
        /// </summary>
        public bool CopyEnabled { get; set; } = true;

        /// <summary>
        /// Language variants of one example. Language identifiers must be unique. When this is set, the
        /// language selector is rendered only if there is more than one variant.
        /// </summary>
        public IEnumerable<CodeSample>? Examples { get; set; }

        /// <summary>
        /// Identifier of the rendered component. Generated when omitted.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Language identifier for a single snippet, such as <c>csharp</c>, <c>javascript</c>, <c>python</c>
        /// or <c>json</c>. Unknown or empty languages are displayed as plain text. Ignored when
        /// <see cref="Examples"/> is set.
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// Display name of the single snippet language. Derived from <see cref="Language"/> when omitted.
        /// </summary>
        public string? LanguageLabel { get; set; }

        /// <summary>
        /// Language shown first when several variants are supplied. Defaults to the first variant.
        /// </summary>
        public string? SelectedLanguage { get; set; }

        /// <summary>
        /// Optional group name. Components with the same group on a page switch language together; a component
        /// without the chosen language keeps its current selection.
        /// </summary>
        public string? SyncGroup { get; set; }

        /// <summary>
        /// Colour theme. <see cref="FDCPCodeTheme.dark"/> uses a dark background and <c>color-scheme: dark</c>.
        /// Defaults to <see cref="FDCPCodeTheme.light"/>.
        /// </summary>
        public FDCPCodeTheme Theme { get; set; } = FDCPCodeTheme.light;

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

            List<CodeSample> samples;
            if (Examples is not null)
            {
                samples = Examples.ToList();
            }
            else
            {
                var code = Code;
                if (code is null)
                {
                    var childContent = await output.GetChildContentAsync().ConfigureAwait(false);
                    code = Dedent(WebUtility.HtmlDecode(childContent.GetContent()));
                }

                samples = [new CodeSample { Language = Language ?? string.Empty, Label = LanguageLabel, Code = code }];
            }

            var html = FDCPCodeRenderer.Render(new CodeBlockOptions
            {
                CopyEnabled = CopyEnabled,
                CssClass = output.Attributes["class"]?.Value?.ToString(),
                Id = Id,
                Samples = samples,
                SelectedLanguage = SelectedLanguage,
                SyncGroup = SyncGroup,
                Theme = Theme,
                Title = Title,
                Wrap = Wrap
            });

            output.TagName = null;
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Clear();
            output.Content.SetHtmlContent(html);
        }

        private static string Dedent(string content)
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
