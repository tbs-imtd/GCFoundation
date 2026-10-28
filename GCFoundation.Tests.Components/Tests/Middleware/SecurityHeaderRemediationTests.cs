using GCFoundation.Components.Middleware;
using Microsoft.AspNetCore.Http;

namespace GCFoundation.Tests.Components.Tests.Middleware
{
    /// <summary>
    /// Covers the AppScan remediations that live in foundation code.
    /// </summary>
    public class SecurityHeaderRemediationTests
    {
        [Fact]
        public async Task CompressedFileRequest_Returns404AndDoesNotContinue()
        {
            var continued = false;
            var middleware = new GCFoundationBlockedCompressedFileMiddleware(_ =>
            {
                continued = true;
                return Task.CompletedTask;
            });
            var context = new DefaultHttpContext();
            context.Request.Path = "/_content/GCFoundation.Components/js/foundation.min.js.gz";

            await middleware.InvokeAsync(context);

            Assert.False(continued);
            Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        }

        [Fact]
        public async Task ScriptRequest_ContinuesThroughPipeline()
        {
            var continued = false;
            var middleware = new GCFoundationBlockedCompressedFileMiddleware(_ =>
            {
                continued = true;
                return Task.CompletedTask;
            });
            var context = new DefaultHttpContext();
            context.Request.Path = "/_content/GCFoundation.Components/js/foundation.min.js";

            await middleware.InvokeAsync(context);

            Assert.True(continued);
            Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        }
    }
}
