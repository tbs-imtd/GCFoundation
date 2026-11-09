using System.Globalization;
using GCFoundation.Common.Settings;
using Microsoft.Extensions.Options;

namespace GCFoundation.Components.Configuration
{
    /// <summary>
    /// Validates <see cref="GCFoundationComponentsSettings"/> at startup so that every enabled CDN resource has a
    /// Subresource Integrity hash. A missing hash usually means a CDN version was overridden in configuration without
    /// the hash for that version.
    /// </summary>
    public sealed class GCFoundationComponentsSettingsValidator : IValidateOptions<GCFoundationComponentsSettings>
    {
        private const string SectionName = "FoundationComponentsSettings";

        /// <summary>
        /// Validates the foundation components settings.
        /// </summary>
        /// <param name="name">The name of the options instance being validated.</param>
        /// <param name="options">The settings to validate.</param>
        /// <returns>The validation result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        public ValidateOptionsResult Validate(string? name, GCFoundationComponentsSettings options)
        {
            ArgumentNullException.ThrowIfNull(options, nameof(options));

            var failures = new List<string>();

            if (options.IncludeGCDSResources)
            {
                RequireIntegrity(failures, nameof(options.GCDSCssIntegrity), options.GCDSCssIntegrity, nameof(options.GCDSVersion), options.GCDSVersion);
                RequireIntegrity(failures, nameof(options.GCDSJavaScriptIntegrity), options.GCDSJavaScriptIntegrity, nameof(options.GCDSVersion), options.GCDSVersion);
                RequireIntegrity(failures, nameof(options.GCDSCssShortcutsIntegrity), options.GCDSCssShortcutsIntegrity, nameof(options.GCDSCssShortcutsVersion), options.GCDSCssShortcutsVersion);
            }

            if (options.IncludeFontAwesome)
            {
                RequireIntegrity(failures, nameof(options.FontAwesomeCssIntegrity), options.FontAwesomeCssIntegrity, nameof(options.FontAwesomeVersion), options.FontAwesomeVersion);
            }

            return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
        }

        private static void RequireIntegrity(List<string> failures, string integrityName, string integrity, string versionName, string version)
        {
            if (!string.IsNullOrWhiteSpace(integrity))
            {
                return;
            }

            failures.Add(string.Format(
                CultureInfo.InvariantCulture,
                "{0}:{1} is required because {0}:{2} is {3}, and GCFoundation only ships integrity hashes for its own CDN versions. Set the sha384 hash for {3}, or remove the version override.",
                SectionName,
                integrityName,
                versionName,
                version));
        }
    }
}
