using System;
using GCFoundation.Common.Settings;
using GCFoundation.Components.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace GCFoundation.Components.Configuration
{
    /// <summary>
    /// Aligns <see cref="StaticResourceHelper"/> with the virtual directory / path base used for <c>/_content/...</c> links.
    /// </summary>
    public sealed class StaticResourcePathsPostConfigure : IPostConfigureOptions<GCFoundationComponentsSettings>
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticResourcePathsPostConfigure"/> class.
        /// </summary>
        /// <param name="configuration">Application configuration (used to read <c>ASPNETCORE_PATHBASE</c>).</param>
        public StaticResourcePathsPostConfigure(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <inheritdoc />
        public void PostConfigure(string? name, GCFoundationComponentsSettings options)
        {
            ArgumentNullException.ThrowIfNull(options);

            var prefix = options.VirtualDirectoryName;
            if (string.IsNullOrWhiteSpace(prefix))
            {
                var pathBase = _configuration["ASPNETCORE_PATHBASE"];
                if (!string.IsNullOrWhiteSpace(pathBase))
                {
                    prefix = pathBase.Trim().Trim('/');
                }
            }

            StaticResourceHelper.ConfigureVirtualDirectoryPrefix(string.IsNullOrWhiteSpace(prefix) ? null : prefix);
        }
    }
}
