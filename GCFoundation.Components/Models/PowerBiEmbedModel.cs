namespace GCFoundation.Components.Models
{
    /// <summary>
    /// Represents the data required to embed a Power BI report on the client,
    /// including the embed URL, access token, and token lifecycle metadata.
    /// </summary>
    public class PowerBiEmbedModel
    {
        /// <summary>
        /// The Power BI embed URL for the report, as returned by the Power BI REST API.
        /// </summary>
        public required Uri EmbedUrl { get; set; } 

        /// <summary>
        /// The embed token used to authenticate the client-side Power BI SDK when rendering the report.
        /// </summary>
        public required string EmbedToken { get; set; }

        /// <summary>
        /// The unique identifier of the Power BI report to embed.
        /// </summary>
        public required Guid ReportId { get; set; } 

        /// <summary>
        /// The UTC date and time at which <see cref="EmbedToken"/> expires, if known.
        /// Used to schedule a proactive token refresh before expiry.
        /// </summary>
        public DateTime? TokenExpiry { get; set; }

        /// <summary>
        /// The URL the client can call to obtain a refreshed embed token before <see cref="TokenExpiry"/> is reached.
        /// </summary>
        public Uri? TokenRefreshUrl { get; set; }
    }
}