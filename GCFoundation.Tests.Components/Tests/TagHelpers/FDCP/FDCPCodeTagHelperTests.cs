using GCFoundation.Components.Helpers;
using GCFoundation.Components.Models.Code;
using GCFoundation.Components.TagHelpers.FDCP;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;
using System.Net;
using System.Text.Encodings.Web;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.FDCP;

public class FDCPCodeTagHelperTests
{
    private const string AoasExample = """"
        var payload = """
            { "title": "Demande d'accès", "tags": ["<b>", "</script>"] }
            """;
        var request = new HttpRequestMessage(HttpMethod.Post, "v1/requests") { Content = new StringContent(payload) };
        """";

    [Fact]
    public async Task CodeBlockRendersSemanticMarkupAndHidesControlsUntilInitialized()
    {
        var doc = Parse(await RenderBlock(new() { Id = "response", Title = "Example response", Language = "json", Code = "{ \"id\": 1 }" }));

        var root = Single(doc, "//div[@data-fdcp-code='true']");
        Assert.Equal("response", root.Id);
        Assert.Contains("fdcp-code", root.GetClasses());
        Assert.DoesNotContain("fdcp-code--multiple", root.GetClasses());
        Assert.Equal("Example response", Single(doc, "//p[@id='response-title']").InnerText);

        var controls = Single(doc, "//*[@data-fdcp-code-controls]");
        Assert.True(controls.Attributes.Contains("hidden"));
        Assert.Null(doc.DocumentNode.SelectSingleNode("//select"));

        var button = Single(doc, "//button[@data-fdcp-code-copy]");
        Assert.Equal("button", button.GetAttributeValue("type", ""));
        Assert.Equal("response-title", button.GetAttributeValue("aria-describedby", ""));
        Assert.Equal("Copy code", button.InnerText);
        Assert.Equal("status", Single(doc, "//*[@data-fdcp-code-status]").GetAttributeValue("role", ""));
        Assert.Equal("Code copied to the clipboard.", Decode(root.GetAttributeValue("data-fdcp-code-copied", "")));

        var panel = Single(doc, "//div[@data-fdcp-code-panel]");
        Assert.Equal("json", panel.GetAttributeValue("data-fdcp-code-language", ""));
        Assert.False(panel.Attributes.Contains("hidden"));
        Assert.Equal("JSON", Single(doc, "//div[contains(@class,'fdcp-code__heading')]/p[contains(@class,'fdcp-code__language')]").InnerText);

        var pre = Single(doc, "//pre");
        Assert.Equal("0", pre.GetAttributeValue("tabindex", ""));
        Assert.Equal("group", pre.GetAttributeValue("role", ""));
        Assert.Equal("Example response (JSON)", Decode(pre.GetAttributeValue("aria-label", "")));
        Assert.Equal("no", pre.GetAttributeValue("translate", ""));
        Assert.Equal("{ \"id\": 1 }", CodeText(doc));
    }

    [Theory]
    [InlineData("</script><script>alert(1)</script>")]
    [InlineData("<b onclick=\"x()\">bold</b> & 'single' \"double\"")]
    [InlineData("Accès à l'information — 日本語 😀")]
    [InlineData("    indented\n\ttabbed\n\nblank line above")]
    [InlineData(AoasExample)]
    public void SourceIsEncodedOnceAndPreservedExactly(string source)
    {
        var html = Html(new CodeBlockOptions { Samples = [new CodeSample { Language = "csharp", Code = source }] });

        Assert.DoesNotContain("<script", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<b ", html, StringComparison.Ordinal);
        Assert.DoesNotContain("&amp;lt;", html, StringComparison.Ordinal);
        Assert.Equal(source.ReplaceLineEndings("\n"), CodeText(Parse(html)));
    }

    [Fact]
    public void LineEndingsAreNormalizedToLineFeeds()
    {
        var html = Html(new CodeBlockOptions { Samples = [new CodeSample { Code = "a\r\nb\rc\n" }] });
        Assert.Equal("a\nb\nc\n", CodeText(Parse(html)));
    }

    [Theory]
    [InlineData("csharp", "csharp", "C#")]
    [InlineData("JavaScript", "javascript", "JavaScript")]
    [InlineData("python", "python", "Python")]
    [InlineData("json", "json", "JSON")]
    [InlineData("cshtml", "cshtml", "Razor")]
    [InlineData("cobol", "cobol", "cobol")]
    [InlineData("", "plaintext", "Plain text")]
    [InlineData("txt", "plaintext", "Plain text")]
    [InlineData("c#\"><x", "plaintext", "c#\"><x")]
    public void LanguagesAreNormalizedAndLabelled(string language, string expected, string label)
    {
        var doc = Parse(Html(new CodeBlockOptions { Samples = [new CodeSample { Language = language, Code = "x" }] }));
        Assert.Equal(expected, Single(doc, "//div[@data-fdcp-code-panel]").GetAttributeValue("data-fdcp-code-language", ""));
        Assert.Equal(label, Decode(Single(doc, "//p[contains(@class,'fdcp-code__language')]").InnerHtml));
    }

    [Fact]
    public void CodeExampleRendersEveryVariantReadableWithLabelledSelector()
    {
        var output = RenderExample(new()
        {
            Id = "create",
            Title = "Create a request",
            Examples = Samples("csharp", "javascript", "python"),
            SelectedLanguage = "python",
            SyncGroup = "request-examples"
        });
        var doc = Parse(output);

        var root = Single(doc, "//div[@data-fdcp-code='true']");
        Assert.Contains("fdcp-code--multiple", root.GetClasses());
        Assert.Equal("request-examples", root.GetAttributeValue("data-fdcp-code-sync-group", ""));

        var select = Single(doc, "//select[@data-fdcp-code-select]");
        Assert.Equal(select.Id, Single(doc, "//label").GetAttributeValue("for", ""));
        Assert.Equal("Code language", Single(doc, "//label").InnerText);
        Assert.Equal("create-title", select.GetAttributeValue("aria-describedby", ""));
        var options = select.SelectNodes("option");
        Assert.Equal(new[] { "csharp", "javascript", "python" }, options.Select(o => o.GetAttributeValue("value", "")));
        Assert.Equal(new[] { "C#", "JavaScript", "Python" }, options.Select(o => o.InnerText));
        Assert.Equal("python", Single(doc, "//option[@selected]").GetAttributeValue("value", ""));

        var panels = doc.DocumentNode.SelectNodes("//div[@data-fdcp-code-panel]");
        Assert.Equal(3, panels.Count);
        Assert.All(panels, p => Assert.False(p.Attributes.Contains("hidden")));
        Assert.All(panels, p => Assert.NotNull(p.SelectSingleNode("p[contains(@class,'fdcp-code__language')]")));
        Assert.Null(doc.DocumentNode.SelectSingleNode("//div[contains(@class,'fdcp-code__heading')]/p[contains(@class,'fdcp-code__language')]"));
        Assert.Equal("python", Single(doc, "//div[@data-fdcp-code-active='true']").GetAttributeValue("data-fdcp-code-language", ""));
        Assert.Equal(new[] { "source csharp", "source javascript", "source python" }, doc.DocumentNode.SelectNodes("//code").Select(c => Decode(c.InnerHtml)));
    }

    [Fact]
    public void UnknownSelectedLanguageFallsBackToFirstExample()
    {
        var doc = Parse(RenderExample(new() { Examples = Samples("csharp", "python"), SelectedLanguage = "ruby" }));
        Assert.Equal("csharp", Single(doc, "//div[@data-fdcp-code-active='true']").GetAttributeValue("data-fdcp-code-language", ""));
        Assert.Equal("csharp", Single(doc, "//option[@selected]").GetAttributeValue("value", ""));
    }

    [Fact]
    public void RejectsMissingAndDuplicateLanguages()
    {
        Assert.Throws<ArgumentException>(() => RenderExample(new()));
        Assert.Throws<ArgumentException>(() => RenderExample(new() { Examples = Samples("csharp", "CSharp") }));
        Assert.Throws<ArgumentException>(() => RenderExample(new() { Examples = [new() { Code = "a" }, new() { Language = "text", Code = "b" }] }));
    }

    [Fact]
    public async Task CopyDisabledOmitsCopyControlsAndMessages()
    {
        var html = await RenderBlock(new() { Language = "csharp", Code = "x", CopyEnabled = false });
        var doc = Parse(html);
        Assert.Null(doc.DocumentNode.SelectSingleNode("//button"));
        Assert.Null(doc.DocumentNode.SelectSingleNode("//*[@data-fdcp-code-status]"));
        Assert.DoesNotContain("data-fdcp-code-copied", html, StringComparison.Ordinal);
        Assert.Null(doc.DocumentNode.SelectSingleNode("//*[contains(@class,'fdcp-code__header')]"));
        Assert.Equal("C#", Single(doc, "//div[@data-fdcp-code-panel]/p[contains(@class,'fdcp-code__language')]").InnerText);
    }

    [Fact]
    public async Task WrapAndClassAreAppliedToRoot()
    {
        var output = new TagHelperOutput("fdcp-code-block", new TagHelperAttributeList { { "class", "mb-400" } }, (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
        await new FDCPCodeBlockTagHelper { Code = "x", Wrap = true }.ProcessAsync(Context(), output);

        Assert.Null(output.TagName);
        var classes = Single(Parse(output.Content.GetContent()), "//div[@data-fdcp-code]").GetClasses().ToList();
        Assert.Contains("fdcp-code--wrap", classes);
        Assert.Contains("mb-400", classes);
    }

    [Fact]
    public async Task ChildContentIsDecodedTrimmedAndDedented()
    {
        const string child = "\n        &lt;fdcp-input for=&quot;Name&quot; /&gt;\n            @Model.Value\n\n        &lt;/div&gt;\n    ";
        var doc = Parse(await RenderBlock(new() { Language = "cshtml" }, child));
        Assert.Equal("<fdcp-input for=\"Name\" />\n    @Model.Value\n\n</div>", CodeText(doc));
    }

    [Fact]
    public void IdsAreSanitizedUniqueAndReferenced()
    {
        var first = RenderExample(new() { Id = "my example<1>", Title = "First", Examples = Samples("csharp", "python") });
        var second = RenderExample(new() { Title = "Second", Examples = Samples("csharp", "python") });
        var third = RenderExample(new() { Title = "Third", Examples = Samples("csharp", "python") });
        var doc = Parse(first + second + third);

        var ids = doc.DocumentNode.SelectNodes("//*[@id]").Select(n => n.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
        Assert.Contains("my-example-1-", ids);
        Assert.All(ids, id => Assert.Matches("^[A-Za-z0-9_-]+$", id));
        foreach (var node in doc.DocumentNode.SelectNodes("//*[@for or @aria-describedby]"))
        {
            var target = node.GetAttributeValue("for", "");
            Assert.Contains(target.Length > 0 ? target : node.GetAttributeValue("aria-describedby", ""), ids);
        }
    }

    [Fact]
    public void AccessibleLabelNamesCodeWithoutVisibleTitle()
    {
        var doc = Parse(Html(new CodeBlockOptions
        {
            AccessibleLabel = "GET /requests — Schema",
            Samples = [new CodeSample { Language = "json", Code = "{}" }]
        }));
        Assert.Null(doc.DocumentNode.SelectSingleNode("//*[contains(@class,'fdcp-code__title')]"));
        Assert.Null(Single(doc, "//button").Attributes["aria-describedby"]);
        Assert.Equal("GET /requests — Schema (JSON)", Decode(Single(doc, "//pre").GetAttributeValue("aria-label", "")));
    }

    [Fact]
    public void FrenchLabelsFollowCurrentUICulture()
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-CA");
            var doc = Parse(RenderExample(new() { Examples = [new() { Language = "csharp", Code = "a" }, new() { Language = "", Code = "b" }] }));
            Assert.Equal("Copier le code", Single(doc, "//button").InnerText);
            Assert.Equal("Langage du code", Decode(Single(doc, "//label").InnerHtml));
            Assert.Equal("Texte brut", Single(doc, "//option[@value='plaintext']").InnerText);
            Assert.Equal("Code C#", Decode(Single(doc, "(//pre)[1]").GetAttributeValue("aria-label", "")));
            Assert.StartsWith("Le code n'a pas pu", Decode(Single(doc, "//div[@data-fdcp-code]").GetAttributeValue("data-fdcp-code-copy-failed", "")), StringComparison.Ordinal);
        }
        finally { CultureInfo.CurrentUICulture = previous; }
    }

    private static List<CodeSample> Samples(params string[] languages) =>
        languages.Select(l => new CodeSample { Language = l, Code = "source " + l.ToLowerInvariant() }).ToList();

    private static async Task<string> RenderBlock(FDCPCodeBlockTagHelper helper, string childContent = "")
    {
        var output = new TagHelperOutput("fdcp-code-block", [], (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent().SetHtmlContent(childContent)));
        await helper.ProcessAsync(Context(), output);
        return output.Content.GetContent();
    }

    private static string RenderExample(FDCPCodeExampleTagHelper helper)
    {
        var output = new TagHelperOutput("fdcp-code-example", [], (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
        helper.Process(Context(), output);
        return output.Content.GetContent();
    }

    private static string Html(CodeBlockOptions options)
    {
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        FDCPCodeRenderer.Render(options).WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }

    private static TagHelperContext Context() => new([], new Dictionary<object, object>(), "test");
    private static HtmlDocument Parse(string html) { var doc = new HtmlDocument(); doc.LoadHtml(html); return doc; }
    private static HtmlNode Single(HtmlDocument doc, string xpath) => Assert.Single(doc.DocumentNode.SelectNodes(xpath) ?? Enumerable.Empty<HtmlNode>());
    private static string Decode(string value) => WebUtility.HtmlDecode(value);
    private static string CodeText(HtmlDocument doc) => Decode(Single(doc, "//code[@data-fdcp-code-source]").InnerHtml);
}
