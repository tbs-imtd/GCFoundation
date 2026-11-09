using GCFoundation.Common.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GCFoundation.Security.Middlewares
{
    public static class GCFoundationContentPoliciesExtensions
    {

        public static IServiceCollection AddGCFoundationContentPolicies(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            var section = configuration.GetSection("ContentPolicySettings");
            services.Configure<GCFoundationContentPolicySettings>(section);

            return services;
        }

        public static IApplicationBuilder UseGCFoundationContentPolicies(this IApplicationBuilder app)
        {
            app.UseMiddleware<GCFoundationContentPoliciesMiddleware>();
            app.UseMiddleware<GCFoundationBlockedCompressedFileMiddleware>();
            return app;
        }
    }
}
