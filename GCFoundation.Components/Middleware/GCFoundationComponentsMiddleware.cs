using GCFoundation.Common.Settings;
using GCFoundation.Components.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Text;

namespace GCFoundation.Components.Middleware
{
    /// <summary>
    /// Middleware that processes HTTP requests and injects foundation component resources (e.g., CSS, JavaScript) into the HTML response.
    /// This middleware checks if the response content type is HTML and then modifies the response by injecting the appropriate CDN or local resources.
    /// </summary>
    public class GCFoundationComponentsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly GCFoundationComponentsSettings _foundationComponentsSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="GCFoundationComponentsMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="foundationComponentsSettings">The foundation components settings for configuring CDN usage and resources.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="foundationComponentsSettings"/> is null.</exception>
        public GCFoundationComponentsMiddleware(RequestDelegate next, IOptions<GCFoundationComponentsSettings> foundationComponentsSettings)
        {
            ArgumentNullException.ThrowIfNull(foundationComponentsSettings, nameof(foundationComponentsSettings));

            _next = next;
            _foundationComponentsSettings = foundationComponentsSettings.Value;
        }

        /// <summary>
        /// Invokes the middleware to process the HTTP request and response.
        /// This method checks if the response is HTML and injects the appropriate CDN or local resources (CSS, JavaScript) into the response body.
        /// </summary>
        /// <param name="context">The HTTP context for the request and response.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            var originalBodyStream = context.Response.Body;

            using (var newBodyStream = new MemoryStream())
            {
                context.Response.Body = newBodyStream;

                await _next(context).ConfigureAwait(false);

                if (context.Response.ContentType?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true)
                {
                    newBodyStream.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(newBodyStream))
                    {
                        var html = await reader.ReadToEndAsync().ConfigureAwait(false);
                        html = html.Replace("</head>", @$"
                        <link rel=""stylesheet"" href=""{_foundationComponentsSettings.FontAwesomeCDN}"" integrity=""{_foundationComponentsSettings.FontAwesomeCssIntegrity}"" crossorigin=""anonymous"">
                        <link rel=""stylesheet"" href=""{_foundationComponentsSettings.GCDSCssCDN}"" integrity=""{_foundationComponentsSettings.GCDSCssIntegrity}"" crossorigin=""anonymous"">
                        <link rel=""stylesheet"" href=""{_foundationComponentsSettings.GCDSCssShortcutsCDN}"" integrity=""{_foundationComponentsSettings.GCDSCssShortcutsIntegrity}"" crossorigin=""anonymous"">
                        <script type=""module"" src=""{_foundationComponentsSettings.GCDSJavaScriptCDN}"" integrity=""{_foundationComponentsSettings.GCDSJavaScriptIntegrity}"" crossorigin=""anonymous""></script>
                    </head>", StringComparison.OrdinalIgnoreCase);

                        var modifiedHtml = Encoding.UTF8.GetBytes(html);
                        ReadOnlyMemory<byte> memory = new ReadOnlyMemory<byte>(modifiedHtml);
                        context.Response.Body = originalBodyStream;
                        await context.Response.Body.WriteAsync(memory, CancellationToken.None).ConfigureAwait(false);
                    }
                }
                else
                {
                    newBodyStream.Seek(0, SeekOrigin.Begin);
                    await newBodyStream.CopyToAsync(originalBodyStream).ConfigureAwait(false);
                }
            }
        }
    }
}
