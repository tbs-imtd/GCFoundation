using GCFoundation.Common.Settings;
using GCFoundation.Components.Configuration;
using GCFoundation.Components.Helpers;
using GCFoundation.Components.Services;
using GCFoundation.Components.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GCFoundation.Components.Middleware
{
    /// <summary>
    /// Provides extension methods for configuring and using foundation components in the application.
    /// These methods help register services and middleware related to foundation components.
    /// </summary>
    public static class GCFoundationComponentsExtensions
    {
        /// <summary>
        /// Registers the foundation components services into the application's dependency injection container.
        /// This method configures the <see cref="GCFoundationComponentsSettings"/> and registers the <see cref="GCFoundationComponentsCdnPolicyConfigurator"/>
        /// to handle content security policy configuration.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="configuration">The configuration instance used to retrieve settings for foundation components.</param>
        /// <returns>The updated <see cref="IServiceCollection"/> with foundation components services added.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        public static IServiceCollection AddGCFoundationComponents(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            var section = configuration.GetSection("FoundationComponentsSettings");
            services.Configure<GCFoundationComponentsSettings>(section);

            services.AddSingleton<IPostConfigureOptions<GCFoundationComponentsSettings>, StaticResourcePathsPostConfigure>();

            // Configure user login settings
            var userLoginSection = configuration.GetSection("UserLoginSettings");
            services.Configure<GCFoundationUserLoginSettings>(userLoginSection);

            // Register the CdnPolicyConfigurator
            services.AddSingleton<IConfigureOptions<GCFoundationContentPolicySettings>, GCFoundationComponentsCdnPolicyConfigurator>();

            // Register the GlobalResourceHelper for use in Razor views
            services.AddScoped<GlobalResourceHelper>();

            // Register the UserLoginService for managing user login view models
            services.AddScoped<UserLoginService>();

            return services;
        }

        /// <summary>
        /// Configures the application to use foundation components middleware.
        /// This method adds the necessary middleware to handle the processing of foundation component-related logic.
        /// </summary>
        /// <param name="app">The application builder to add middleware to.</param>
        /// <returns>The updated <see cref="IApplicationBuilder"/> with foundation components middleware added.</returns>
        public static IApplicationBuilder UseGCFoundationComponents(this IApplicationBuilder app)
        {
            app.UseMiddleware<GCFoundationComponentsMiddleware>();
            return app;
        }

    }
}