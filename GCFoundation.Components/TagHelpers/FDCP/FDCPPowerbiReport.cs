using GCFoundation.Components.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;

namespace GCFoundation.Components.TagHelpers.FDCP
{
    /// <summary>
    /// Renders the markup required to embed a Power BI report, sourcing its data either from
    /// a bound <see cref="PowerBiEmbedModel"/> (via the <c>for</c> attribute) or from individual
    /// flat attributes. When both are supplied, the bound model takes precedence.
    /// </summary>
    public class PowerBiReportTagHelper : TagHelper
    {
        /// <summary>
        /// The Power BI embed URL for the report, used when no <see cref="Model"/> is bound.
        /// </summary>
        [HtmlAttributeName("embedded-url")]
        public Uri? EmbeddedUrl { get; set; }

        /// <summary>
        /// The embed token used to authenticate the client-side Power BI SDK, used when no <see cref="Model"/> is bound.
        /// </summary>
        [HtmlAttributeName("embedded-token")]
        public string? EmbeddedToken { get; set; }

        /// <summary>
        /// A bound <see cref="PowerBiEmbedModel"/> supplying all embed data at once. Takes precedence
        /// over the individual flat attributes when set.
        /// </summary>
        [HtmlAttributeName("for")]
        public PowerBiEmbedModel? Model { get; set; }

        /// <summary>
        /// The unique identifier of the Power BI report to embed, used when no <see cref="Model"/> is bound.
        /// </summary>
        [HtmlAttributeName("report-id")]
        public string? ReportId { get; set; }

        /// <summary>
        /// The UTC expiry timestamp of the embed token, in ISO 8601 round-trip format, used when no <see cref="Model"/> is bound.
        /// </summary>
        [HtmlAttributeName("token-expiry")]
        public string? TokenExpiry { get; set; }

        /// <summary>
        /// The URL the client can call to obtain a refreshed embed token, used when no <see cref="Model"/> is bound.
        /// </summary>
        [HtmlAttributeName("token-refresh-url")]
        public Uri? TokenRefreshUrl { get; set; }

        /// <inheritdoc/>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(output);

            if (Model != null)
            {
                EmbeddedUrl = Model.EmbedUrl;
                EmbeddedToken = Model.EmbedToken;
                ReportId = Model.ReportId.ToString();
                TokenExpiry = Model.TokenExpiry?.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
                TokenRefreshUrl = Model.TokenRefreshUrl;
            }

            output.TagName = "gcds-container";
            output.Attributes.SetAttribute("size", "lg");

            var innerDiv = new TagBuilder("div");
            innerDiv.AddCssClass("fdcp-powerbi-container");
            innerDiv.Attributes["data-embedded-url"] = EmbeddedUrl != null ? EmbeddedUrl.ToString() : "";
            innerDiv.Attributes["data-embedded-token"] = EmbeddedToken;
            innerDiv.Attributes["data-report-id"] = ReportId;

            if (!string.IsNullOrEmpty(TokenExpiry))
                innerDiv.Attributes["data-token-expiry"] = TokenExpiry;
            if (TokenRefreshUrl != null)
                innerDiv.Attributes["data-token-refresh-url"] = TokenRefreshUrl.ToString();

            output.Content.AppendHtml(innerDiv);
        }
    }
}