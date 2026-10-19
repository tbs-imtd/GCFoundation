using GCFoundation.Components.Models.Code;
using GCFoundation.Components.Resources;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace GCFoundation.Components.Helpers
{
    /// <summary>
    /// Renders the FDCP code component. The <c>&lt;fdcp-code-block&gt;</c> and <c>&lt;fdcp-code-example&gt;</c>
    /// Tag Helpers use this renderer, and C# code that generates HTML can call it directly instead of
    /// reproducing the component markup.
    /// </summary>
    /// <example>
    /// <code>
    /// var html = FDCPCodeRenderer.Render(new CodeBlockOptions
    /// {
    ///     Title = "Example response",
    ///     Samples = [new CodeSample { Language = "json", Code = responseJson }]
    /// });
    /// </code>
    /// </example>
    public static class FDCPCodeRenderer
    {
        /// <summary>The language identifier used for unknown, empty and plain text languages.</summary>
        public const string PlainTextLanguage = "plaintext";

        private static readonly HashSet<string> PlainTextAliases = new(StringComparer.Ordinal) { "plaintext", "plain", "text", "txt", "none" };

        private static readonly Dictionary<string, string> LanguageLabels = new(StringComparer.Ordinal)
        {
            ["aspnet"] = "ASP.NET",
            ["bash"] = "Bash",
            ["cs"] = "C#",
            ["csharp"] = "C#",
            ["cshtml"] = "Razor",
            ["css"] = "CSS",
            ["dotnet"] = "C#",
            ["html"] = "HTML",
            ["javascript"] = "JavaScript",
            ["js"] = "JavaScript",
            ["json"] = "JSON",
            ["markup"] = "HTML",
            ["py"] = "Python",
            ["python"] = "Python",
            ["razor"] = "Razor",
            ["sh"] = "Shell",
            ["shell"] = "Shell",
            ["svg"] = "SVG",
            ["xml"] = "XML",
        };

        /// <summary>
        /// Renders a code block, or a multi-language code example when several samples are supplied.
        /// </summary>
        /// <param name="options">The code and display options.</param>
        /// <returns>Encoded HTML for the component.</returns>
        /// <exception cref="ArgumentException">No samples are supplied or two samples share a language.</exception>
        public static IHtmlContent Render(CodeBlockOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            if (options.Samples is not { Count: > 0 })
            {
                throw new ArgumentException("At least one code sample is required.", nameof(options));
            }

            var id = SanitizeId(options.Id);
            var samples = options.Samples
                .Select(s => (Language: NormalizeLanguage(s?.Language), Sample: s ?? new CodeSample()))
                .Select(s => (s.Language, Label: string.IsNullOrWhiteSpace(s.Sample.Label) ? LabelFor(s.Language, s.Sample.Language) : s.Sample.Label.Trim(), Code: NormalizeNewLines(s.Sample.Code)))
                .ToList();

            var duplicate = samples.GroupBy(s => s.Language, StringComparer.Ordinal).FirstOrDefault(g => g.Count() > 1);
            if (duplicate is not null)
            {
                throw new ArgumentException($"The code language '{duplicate.Key}' is used more than once in the same component.", nameof(options));
            }

            var selectedLanguage = NormalizeLanguage(options.SelectedLanguage);
            var activeIndex = Math.Max(0, samples.FindIndex(s => s.Language == selectedLanguage));
            var hasTitle = !string.IsNullOrWhiteSpace(options.Title);
            var titleId = $"{id}-title";
            var isMultiLanguage = samples.Count > 1;

            var root = new TagBuilder("div");
            root.Attributes["id"] = id;
            root.AddCssClass("fdcp-code");
            if (isMultiLanguage) root.AddCssClass("fdcp-code--multiple");
            if (options.Wrap) root.AddCssClass("fdcp-code--wrap");
            if (!string.IsNullOrWhiteSpace(options.CssClass)) root.AddCssClass(options.CssClass.Trim());
            root.Attributes["data-fdcp-code"] = "true";
            if (!string.IsNullOrWhiteSpace(options.SyncGroup)) root.Attributes["data-fdcp-code-sync-group"] = options.SyncGroup.Trim();
            if (options.CopyEnabled)
            {
                root.Attributes["data-fdcp-code-copied"] = Localization.Code_Copied;
                root.Attributes["data-fdcp-code-copy-failed"] = Localization.Code_CopyFailed;
            }

            var hasHeader = hasTitle || options.CopyEnabled || isMultiLanguage;
            if (hasHeader)
            {
                var header = new TagBuilder("div");
                header.AddCssClass("fdcp-code__header");

                var heading = new TagBuilder("div");
                heading.AddCssClass("fdcp-code__heading");
                if (hasTitle)
                {
                    var title = new TagBuilder("p");
                    title.Attributes["id"] = titleId;
                    title.AddCssClass("fdcp-code__title");
                    title.InnerHtml.Append(options.Title!.Trim());
                    heading.InnerHtml.AppendHtml(title);
                }
                if (!isMultiLanguage) heading.InnerHtml.AppendHtml(LanguageLabel(samples[0].Label));
                header.InnerHtml.AppendHtml(heading);

                // Controls depend on JavaScript, so they stay hidden until the component is initialized.
                var controls = new TagBuilder("div");
                controls.AddCssClass("fdcp-code__controls");
                controls.Attributes["data-fdcp-code-controls"] = "true";
                controls.Attributes["hidden"] = "hidden";

                if (isMultiLanguage)
                {
                    var selectId = $"{id}-language";
                    var picker = new TagBuilder("div");
                    picker.AddCssClass("fdcp-code__picker");

                    var label = new TagBuilder("label");
                    label.AddCssClass("fdcp-code__select-label");
                    label.Attributes["for"] = selectId;
                    label.InnerHtml.Append(Localization.Code_LanguageSelect);
                    picker.InnerHtml.AppendHtml(label);

                    var select = new TagBuilder("select");
                    select.AddCssClass("fdcp-code__select");
                    select.Attributes["id"] = selectId;
                    select.Attributes["data-fdcp-code-select"] = "true";
                    if (hasTitle) select.Attributes["aria-describedby"] = titleId;
                    for (var i = 0; i < samples.Count; i++)
                    {
                        var option = new TagBuilder("option");
                        option.Attributes["value"] = samples[i].Language;
                        if (i == activeIndex) option.Attributes["selected"] = "selected";
                        option.InnerHtml.Append(samples[i].Label);
                        select.InnerHtml.AppendHtml(option);
                    }
                    picker.InnerHtml.AppendHtml(select);
                    controls.InnerHtml.AppendHtml(picker);
                }

                if (options.CopyEnabled)
                {
                    var copy = new TagBuilder("button");
                    copy.AddCssClass("fdcp-code__copy");
                    copy.Attributes["type"] = "button";
                    copy.Attributes["data-fdcp-code-copy"] = "true";
                    if (hasTitle) copy.Attributes["aria-describedby"] = titleId;
                    copy.InnerHtml.Append(Localization.Code_Copy);
                    controls.InnerHtml.AppendHtml(copy);

                    var status = new TagBuilder("span");
                    status.AddCssClass("fdcp-code__status");
                    status.Attributes["role"] = "status";
                    status.Attributes["data-fdcp-code-status"] = "true";
                    controls.InnerHtml.AppendHtml(status);
                }

                header.InnerHtml.AppendHtml(controls);
                root.InnerHtml.AppendHtml(header);
            }

            for (var i = 0; i < samples.Count; i++)
            {
                var (language, label, code) = samples[i];
                var panelId = $"{id}-panel-{(i + 1).ToString(CultureInfo.InvariantCulture)}";

                // Every variant is rendered visible so the content stays readable without JavaScript and in print.
                var panel = new TagBuilder("div");
                panel.AddCssClass("fdcp-code__panel");
                panel.Attributes["id"] = panelId;
                panel.Attributes["data-fdcp-code-panel"] = "true";
                panel.Attributes["data-fdcp-code-language"] = language;
                if (i == activeIndex) panel.Attributes["data-fdcp-code-active"] = "true";

                if (isMultiLanguage || !hasHeader) panel.InnerHtml.AppendHtml(LanguageLabel(label));

                var pre = new TagBuilder("pre");
                pre.AddCssClass("fdcp-code__pre");
                pre.Attributes["tabindex"] = "0";
                pre.Attributes["role"] = "group";
                var name = hasTitle ? options.Title!.Trim() : options.AccessibleLabel?.Trim();
                pre.Attributes["aria-label"] = string.IsNullOrEmpty(name)
                    ? string.Format(CultureInfo.CurrentCulture, Localization.Code_RegionLabel, label)
                    : $"{name} ({label})";
                pre.Attributes["dir"] = "ltr";
                pre.Attributes["translate"] = "no";

                var codeElement = new TagBuilder("code");
                codeElement.AddCssClass("fdcp-code__code");
                codeElement.Attributes["data-fdcp-code-source"] = "true";
                codeElement.InnerHtml.Append(code);
                pre.InnerHtml.AppendHtml(codeElement);

                panel.InnerHtml.AppendHtml(pre);
                root.InnerHtml.AppendHtml(panel);
            }

            return root;
        }

        /// <summary>
        /// Normalizes a language identifier. Empty, plain text and malformed identifiers return
        /// <see cref="PlainTextLanguage"/>.
        /// </summary>
        /// <param name="language">The language identifier supplied by the consumer.</param>
        /// <returns>The lower-case identifier used by the component.</returns>
        public static string NormalizeLanguage(string? language)
        {
            var value = language?.Trim().ToLowerInvariant() ?? string.Empty;
            if (value.Length == 0 || PlainTextAliases.Contains(value)) return PlainTextLanguage;
            return value.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_') ? value : PlainTextLanguage;
        }

        private static TagBuilder LanguageLabel(string label)
        {
            var element = new TagBuilder("p");
            element.AddCssClass("fdcp-code__language");
            element.InnerHtml.Append(label);
            return element;
        }

        private static string LabelFor(string language, string? original)
        {
            if (language == PlainTextLanguage)
            {
                return string.IsNullOrWhiteSpace(original) || PlainTextAliases.Contains(original.Trim().ToLowerInvariant())
                    ? Localization.Code_PlainText
                    : original.Trim();
            }
            return LanguageLabels.TryGetValue(language, out var label) ? label : language;
        }

        // Browsers normalize CRLF to LF when parsing HTML, so the component does the same up front for deterministic copies.
        private static string NormalizeNewLines(string? code) =>
            (code ?? string.Empty).Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');

        private static string SanitizeId(string? id)
        {
            if (string.IsNullOrWhiteSpace(id)) return $"fdcp-code-{Guid.NewGuid():N}";
            return new string(id.Trim().Select(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' ? c : '-').ToArray());
        }
    }
}
