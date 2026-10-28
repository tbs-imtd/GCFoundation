using Microsoft.AspNetCore.Http;

namespace GCFoundation.Components.Middleware
{
    /// <summary>
    /// Rejects direct requests for pre-compressed static files such as <c>script.js.gz</c>.
    /// Browsers request the original file and receive <c>Content-Encoding</c>; a downloadable <c>.gz</c> copy exposes the same source.
    /// </summary>
    public class GCFoundationBlockedCompressedFileMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="GCFoundationBlockedCompressedFileMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="next"/> is null.</exception>
        public GCFoundationBlockedCompressedFileMiddleware(RequestDelegate next)
        {
            ArgumentNullException.ThrowIfNull(next, nameof(next));
            _next = next;
        }

        /// <summary>
        /// Returns 404 when the request path ends with <c>.gz</c>. Other requests continue down the pipeline.
        /// </summary>
        /// <param name="context">The HTTP context for the request and response.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
        public Task InvokeAsync(HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            var path = context.Request.Path.Value;
            if (!string.IsNullOrEmpty(path) && path.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return Task.CompletedTask;
            }

            return _next(context);
        }
    }
}
