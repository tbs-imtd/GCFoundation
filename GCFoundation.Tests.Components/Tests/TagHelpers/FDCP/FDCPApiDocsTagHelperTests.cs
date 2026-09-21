using GCFoundation.Components.TagHelpers.FDCP;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;
using System.Net;
using System.Text.Json;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.FDCP;

public class FDCPApiDocsTagHelperTests
{
    private const string Spec = """
        {
          "openapi": "3.1.0", "info": { "title": "Requests API", "version": "1.0", "description": "<script>alert(1)</script>" },
          "security": [{ "Bearer": [] }],
          "paths": {
            "/requests/{id}": {
              "parameters": [{ "$ref": "#/components/parameters/Id" }, { "name": "page", "in": "query", "description": "Inherited page", "schema": { "type": "integer" } }],
              "get": {
                "summary": "<img src=x onerror=alert(1)>", "security": [],
                "parameters": [{ "name": "page", "in": "query", "description": "Operation page", "schema": { "type": "integer" } }],
                "responses": { "200": { "$ref": "#/components/responses/Success", "description": "Overridden response" }, "204": { "description": "Empty" }, "default": { "description": "Error" } }
              },
              "post": {
                "requestBody": { "$ref": "#/components/requestBodies/Create" },
                "responses": { "201": { "description": "Created" } }
              }
            }
          },
          "components": {
            "parameters": { "Id": { "name": "id", "in": "path", "required": true, "schema": { "type": "string", "format": "uuid" } } },
            "securitySchemes": { "Bearer": { "type": "http", "scheme": "bearer" } },
            "requestBodies": { "Create": { "required": true, "content": { "application/json": { "schema": { "$ref": "#/components/schemas/Node" } }, "application/xml": { "schema": { "type": "string" } } } } },
            "responses": { "Success": { "description": "Original response", "content": { "application/json": { "schema": { "type": "array", "items": { "$ref": "#/components/schemas/Node" } }, "examples": { "Sample": { "$ref": "#/components/examples/Sample" } } }, "text/plain": { "schema": { "type": "string" } } } } },
            "examples": { "Sample": { "value": [ { "name": "Example name" } ] } },
            "schemas": {
              "Node": { "type": "object", "required": ["name"], "properties": { "name": { "type": ["string", "null"], "minLength": 1 }, "child": { "$ref": "#/components/schemas/Node" } } },
              "a/b~c": { "allOf": [{ "$ref": "#/components/schemas/Node" }], "additionalProperties": false }
            }
          }
        }
        """;

    [Fact]
    public async Task RendersReferencesOverridesAllMediaTypesAndRecursiveSchemas()
    {
        var html = WebUtility.HtmlDecode((await Render(new() { Document = Spec, Id = "docs" })).Content.GetContent());
        Assert.Contains("Operation page", html);
        Assert.Contains("Inherited page", html); // POST still inherits the path-level value.
        var getOperation = html[..html.IndexOf("id='docs-endpoint-POST", StringComparison.Ordinal)];
        Assert.DoesNotContain("Inherited page", getOperation);
        Assert.Contains("string (uuid)", html);
        Assert.Contains("Overridden response", html);
        Assert.DoesNotContain("Original response", html);
        Assert.Contains("application/json", html);
        Assert.Contains("application/xml", html);
        Assert.Contains("text/plain", html);
        Assert.Contains("Example name", html);
        Assert.Contains("minLength", html);
        Assert.Contains("allOf", html);
        Assert.Contains("additionalProperties", html);
        Assert.Contains("href='#docs-schema-Node'", html);
        Assert.Contains("No authentication required.", getOperation);
        Assert.Contains("Bearer", html);
        Assert.Contains("<code>204</code>", html);
        Assert.Contains("<code>default</code>", html);
    }

    [Fact]
    public async Task EncodesUntrustedDescriptionsTitlesAndPaths()
    {
        var html = (await Render(new() { Document = Spec, Title = "<b>Title</b>" })).Content.GetContent();
        Assert.DoesNotContain("<script>", html);
        Assert.DoesNotContain("<img", html);
        Assert.DoesNotContain("<b>Title</b>", html);
        Assert.Contains("&lt;script&gt;", html);
        Assert.Contains("&lt;b&gt;Title&lt;/b&gt;", html);
    }

    [Fact]
    public async Task CustomSectionsKeepRazorMarkupAndMatchNavigationOrder()
    {
        var helper = new FDCPApiDocsTagHelper { Document = Spec, Id = "custom" };
        var context = Context();
        helper.Init(context);
        var output = Output(async () =>
        {
            await new FDCPApiDocsSectionTagHelper { Title = "Certificates", Id = "certificates" }
                .ProcessAsync(context, Output("<p><strong>Certificate guidance</strong></p>"));
            await new FDCPApiDocsSectionTagHelper { Title = "Support", AfterEndpoints = true }
                .ProcessAsync(context, Output("<a href='/support'>Contact support</a>"));
            return new DefaultTagHelperContent();
        });
        await helper.ProcessAsync(context, output);
        var doc = Parse(output.Content.GetContent());
        var nav = doc.DocumentNode.SelectNodes("//nav//a").Select(n => n.InnerText).ToList();
        Assert.True(nav.IndexOf("Certificates") < nav.IndexOf("Endpoints"));
        Assert.Equal("Support", nav[^1]);
        Assert.Contains("<strong>Certificate guidance</strong>", output.Content.GetContent());
        Assert.Contains("custom-section-certificates", output.Content.GetContent());
    }

    [Fact]
    public async Task GeneratesUniqueAnchorsWithMatchingAriaTargets()
    {
        var spec = Spec.Replace("\"a/b~c\":", "\"a b\": {}, \"a-b\": {}, \"a/b~c\":", StringComparison.Ordinal);
        var first = await Render(new() { Document = spec });
        var second = await Render(new() { Document = spec });
        Assert.NotEqual(first.Attributes["id"].Value, second.Attributes["id"].Value);
        var doc = Parse(first.Content.GetContent() + second.Content.GetContent());
        var ids = doc.DocumentNode.SelectNodes("//*[@id]").Select(n => n.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
        foreach (var link in doc.DocumentNode.SelectNodes("//a[starts-with(@href, '#')]"))
        {
            Assert.Contains(link.GetAttributeValue("href", "")[1..], ids);
        }
        foreach (var region in doc.DocumentNode.SelectNodes("//*[@aria-labelledby]"))
        {
            Assert.Contains(region.GetAttributeValue("aria-labelledby", ""), ids);
        }
        Assert.All(doc.DocumentNode.SelectNodes("//pre"), n => Assert.Equal("0", n.GetAttributeValue("tabindex", "")));
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html,x")]
    [InlineData("//example.com/spec.json")]
    [InlineData("\\evil.example/spec.json")]
    [InlineData(" javaScript:alert(1)")]
    public async Task RejectsUnsafeDownloadUrls(string url) =>
        await Assert.ThrowsAsync<ArgumentException>(() => Render(new() { Document = Spec, DownloadUrl = url }));

    [Theory]
    [InlineData("/openapi/v1.json")]
    [InlineData("https://example.com/openapi.json")]
    [InlineData("spec.json")]
    public async Task AcceptsSafeDownloadUrls(string url) =>
        Assert.Contains("download>", (await Render(new() { Document = Spec, DownloadUrl = url })).Content.GetContent());

    [Fact]
    public async Task SupportsAuthoredDocumentationWithoutOpenApi()
    {
        var output = await Render(new() { Title = "Integration guide" });
        Assert.Contains("Integration guide", output.Content.GetContent());
        Assert.DoesNotContain("No endpoints", output.Content.GetContent());
    }

    [Fact]
    public async Task FrenchLabelsFollowCurrentUICulture()
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("fr-CA");
            var html = WebUtility.HtmlDecode((await Render(new() { Document = Spec })).Content.GetContent());
            Assert.Contains("Points de terminaison", html);
            Assert.Contains("Paramètres", html);
            Assert.Contains("Obligatoire", html);
        }
        finally { CultureInfo.CurrentUICulture = previous; }
    }

    [Theory]
    [InlineData("{\"swagger\":\"2.0\"}")]
    [InlineData("{\"openapi\":\"3.2.0\"}")]
    [InlineData("{\"openapi\":\"3.1.0\"}")]
    public async Task RejectsMissingMetadata(string json) =>
        await Assert.ThrowsAsync<ArgumentException>(() => Render(new() { Document = json }));

    [Theory]
    [InlineData("3.0.99", false)]
    [InlineData("3.1.2", true)]
    [InlineData("3.2.0", true)]
    [InlineData("3.2.1", true)]
    [InlineData("3.2.123", true)]
    [InlineData("3.10.0", true)]
    public async Task AcceptsMajorVersion3InBothRenderers(string version, bool referenceOverrides)
    {
        var json = Spec.Replace("3.1.0", version, StringComparison.Ordinal);
        var page = (await Render(new() { Document = json })).Content.GetContent();
        var reference = Output("");
        new FDCPApiReferenceTagHelper { Document = json }.Process(Context(), reference);
        foreach (var html in new[] { page, reference.Content.GetContent() })
        {
            Assert.Equal(2, Parse(html).DocumentNode.SelectNodes("//section[@class='api-docs__endpoint']").Count);
            Assert.Contains(referenceOverrides ? "Overridden response" : "Original response", html);
            Assert.DoesNotContain(referenceOverrides ? "Original response" : "Overridden response", html);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("2.0.0")]
    [InlineData("4.0.0")]
    [InlineData("30.0.0")]
    [InlineData("3")]
    [InlineData("3.2")]
    [InlineData("3.2.preview")]
    [InlineData("3.2.1.0")]
    [InlineData("3.-1.0")]
    [InlineData("3.02.1")]
    [InlineData("3.2.01")]
    public async Task RejectsMalformedAndNon3VersionsInBothRenderers(string version)
    {
        var json = Spec.Replace("3.1.0", version, StringComparison.Ordinal);
        await Assert.ThrowsAsync<ArgumentException>(() => Render(new() { Document = json }));
        Assert.Throws<ArgumentException>(() => new FDCPApiReferenceTagHelper { Document = json }.Process(Context(), Output("")));
    }

    [Theory]
    [InlineData("3.0.3", 0)]
    [InlineData("3.1.2", 0)]
    [InlineData("3.2.1", 2)]
    [InlineData("3.10.0", 2)]
    public async Task RendersAdditionalMethodsFrom32Onward(string version, int count)
    {
        var json = JsonSerializer.Serialize(new
        {
            openapi = version,
            info = new { title = "Methods", version = "1" },
            paths = new Dictionary<string, object>
            {
                ["/items"] = new
                {
                    parameters = new[] { new { name = "limit", @in = "query", schema = new { type = "integer" } } },
                    query = new { responses = new Dictionary<string, object> { ["200"] = new { description = "Query result" } } },
                    additionalOperations = new Dictionary<string, object>
                    {
                        ["customMethod"] = new { responses = new Dictionary<string, object> { ["204"] = new { description = "Custom result" } } }
                    }
                }
            }
        });
        var page = (await Render(new() { Document = json })).Content.GetContent();
        var reference = Output("");
        new FDCPApiReferenceTagHelper { Document = json }.Process(Context(), reference);
        foreach (var html in new[] { page, reference.Content.GetContent() })
        {
            var doc = Parse(html);
            Assert.Equal(count, doc.DocumentNode.SelectNodes("//section[@class='api-docs__endpoint']")?.Count ?? 0);
            if (count > 0)
            {
                Assert.Contains("data-method='QUERY'", html);
                Assert.Contains("data-method='customMethod'", html);
                Assert.Contains("Query result", html);
                Assert.Contains("Custom result", html);
                Assert.Equal(2, doc.DocumentNode.SelectNodes("//th/code[text()='limit']").Count);
            }
        }
    }

    [Fact]
    public async Task RejectsInvalidJson() =>
        await Assert.ThrowsAnyAsync<JsonException>(() => Render(new() { Document = "{" }));

    [Theory]
    [InlineData("#/components/parameters/Missing")]
    [InlineData("https://example.com/parameters.json")]
    [InlineData("#/paths/~1requests~1{id}/parameters/0")]
    public async Task RejectsBrokenExternalAndCyclicReferenceObjects(string reference) =>
        await Assert.ThrowsAsync<ArgumentException>(() => Render(new() { Document = Spec.Replace("#/components/parameters/Id", reference, StringComparison.Ordinal) }));

    [Fact]
    public async Task SupportsOpenApi30AndDoesNotApply31ReferenceOverrides()
    {
        var html = (await Render(new() { Document = Spec.Replace("3.1.0", "3.0.3", StringComparison.Ordinal) })).Content.GetContent();
        Assert.Contains("Original response", html);
        Assert.DoesNotContain("Overridden response", html);
    }

    [Fact]
    public async Task StandaloneReferenceMatchesPageEndpointsWithoutPageComposition()
    {
        var page = Parse((await Render(new() { Document = Spec, Id = "docs", DownloadUrl = "/spec.json" })).Content.GetContent());
        var output = Output("");
        new FDCPApiReferenceTagHelper { Document = Spec, Id = "docs" }.Process(Context(), output);
        var reference = Parse(output.Content.GetContent());

        Assert.Equal("api-docs api-docs--reference", output.Attributes["class"].Value);
        Assert.Null(reference.DocumentNode.SelectSingleNode("//nav"));
        Assert.Null(reference.DocumentNode.SelectSingleNode("//*[@id='docs-overview']"));
        Assert.Null(reference.DocumentNode.SelectSingleNode("//a[@download]"));
        Assert.Equal(
            page.DocumentNode.SelectNodes("//section[@class='api-docs__endpoint']").Select(n => n.OuterHtml),
            reference.DocumentNode.SelectNodes("//section[@class='api-docs__endpoint']").Select(n => n.OuterHtml));
        Assert.Contains("href='#docs-schema-Node'", output.Content.GetContent());
        Assert.NotNull(reference.DocumentNode.SelectSingleNode("//*[@id='docs-schema-Node']"));
        Assert.DoesNotContain("<img", output.Content.GetContent());
    }

    [Fact]
    public void StandaloneReferenceRequiresDocumentAndScopesEachInstance()
    {
        Assert.Throws<ArgumentException>(() => new FDCPApiReferenceTagHelper().Process(Context(), Output("")));
        var first = Output("");
        var second = Output("");
        new FDCPApiReferenceTagHelper { Document = Spec }.Process(Context(), first);
        new FDCPApiReferenceTagHelper { Document = Spec }.Process(Context(), second);
        Assert.NotEqual(first.Attributes["id"].Value, second.Attributes["id"].Value);
    }

    [Fact]
    public void FormerComponentRoutesRedirectToPageTemplates()
    {
        var controller = new GCFoundation.Web.Controllers.ComponentsController(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<GCFoundation.Web.Controllers.ComponentsController>.Instance,
            new GCFoundation.Web.Services.TagHelperCatalogService<GCFoundation.Web.Resources.Components>());
        var guide = Assert.IsType<Microsoft.AspNetCore.Mvc.RedirectToActionResult>(controller.ApiDocs());
        var demo = Assert.IsType<Microsoft.AspNetCore.Mvc.RedirectToActionResult>(controller.ApiDocsPreview());
        Assert.Equal("Template", guide.ControllerName);
        Assert.Equal("ApiDocs", guide.ActionName);
        Assert.Equal("Template", demo.ControllerName);
        Assert.Equal("ApiDocsDemo", demo.ActionName);
    }

    private static async Task<TagHelperOutput> Render(FDCPApiDocsTagHelper helper)
    {
        var context = Context();
        helper.Init(context);
        var output = Output("");
        await helper.ProcessAsync(context, output);
        return output;
    }
    private static TagHelperContext Context() => new(new TagHelperAttributeList(), new Dictionary<object, object>(), "test");
    private static TagHelperOutput Output(string html) => Output(() => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent().SetHtmlContent(html)));
    private static TagHelperOutput Output(Func<Task<TagHelperContent>> child) => new("fdcp-api-docs", new TagHelperAttributeList(), (_, _) => child());
    private static HtmlDocument Parse(string html) { var doc = new HtmlDocument(); doc.LoadHtml(html); return doc; }
}
