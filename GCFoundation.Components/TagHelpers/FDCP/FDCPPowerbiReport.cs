using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    [HtmlTargetElement("fdcp-powerbi-report", Attributes = "report-id, embedded-url, embedded-token")]
    public class PowerBiReportTagHelper : TagHelper
    {
        [HtmlAttributeName("embedded-url")]
        public string? EmbeddedUrl { get; set; }

        [HtmlAttributeName("embedded-token")]
        public string? EmbeddedToken { get; set; }

        [HtmlAttributeName("report-id")]
        public string? ReportId { get; set; }

        [HtmlAttributeName("token-expiry")]
        public string? TokenExpiry { get; set; }

        [HtmlAttributeName("token-refresh-url")]
        public string? TokenRefreshUrl { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "gcds-container";
            output.Attributes.SetAttribute("size", "lg");

            var innerDiv = new TagBuilder("div");
            innerDiv.AddCssClass("fdcp-powerbi-container");
            innerDiv.Attributes["data-embedded-url"] = EmbeddedUrl;
            innerDiv.Attributes["data-embedded-token"] = EmbeddedToken;
            innerDiv.Attributes["data-report-id"] = ReportId;

            if (!string.IsNullOrEmpty(TokenExpiry))
                innerDiv.Attributes["data-token-expiry"] = TokenExpiry;
            if (!string.IsNullOrEmpty(TokenRefreshUrl))
                innerDiv.Attributes["data-token-refresh-url"] = TokenRefreshUrl;

            output.Content.AppendHtml(innerDiv);
        }
    }
}