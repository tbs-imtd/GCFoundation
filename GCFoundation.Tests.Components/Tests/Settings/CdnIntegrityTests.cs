using GCFoundation.Common.Settings;
using GCFoundation.Components.Configuration;

namespace GCFoundation.Tests.Components.Tests.Settings
{
    /// <summary>
    /// Verifies that CDN versions and Subresource Integrity hashes come from the generated manifest,
    /// and that overriding a CDN version without its hash fails validation instead of shipping a mismatched hash.
    /// </summary>
    public class CdnIntegrityTests
    {
        private readonly GCFoundationComponentsSettingsValidator _validator = new();

        [Fact]
        public void Manifest_HasSha384HashForEveryLoadedFile()
        {
            var manifest = CdnIntegrityManifest.Current;

            Assert.StartsWith("sha384-", manifest.FontAwesome.Css, StringComparison.Ordinal);
            Assert.StartsWith("sha384-", manifest.Gcds.Css, StringComparison.Ordinal);
            Assert.StartsWith("sha384-", manifest.Gcds.JavaScript, StringComparison.Ordinal);
            Assert.StartsWith("sha384-", manifest.GcdsCssShortcuts.Css, StringComparison.Ordinal);
        }

        [Fact]
        public void Defaults_UseManifestVersionsAndHashes_AndPassValidation()
        {
            var manifest = CdnIntegrityManifest.Current;
            var settings = new GCFoundationComponentsSettings();

            Assert.Equal(manifest.Gcds.Version, settings.GCDSVersion);
            Assert.Equal(manifest.Gcds.Css, settings.GCDSCssIntegrity);
            Assert.Equal(manifest.Gcds.JavaScript, settings.GCDSJavaScriptIntegrity);
            Assert.Equal(manifest.GcdsCssShortcuts.Css, settings.GCDSCssShortcutsIntegrity);
            Assert.Equal(manifest.FontAwesome.Css, settings.FontAwesomeCssIntegrity);
            Assert.True(_validator.Validate(null, settings).Succeeded);
        }

        [Fact]
        public void VersionOverride_WithoutHash_DropsShippedHash_AndFailsValidation()
        {
            var settings = new GCFoundationComponentsSettings { GCDSVersion = "9.9.9" };

            var result = _validator.Validate(null, settings);

            Assert.Empty(settings.GCDSCssIntegrity);
            Assert.Empty(settings.GCDSJavaScriptIntegrity);
            Assert.True(result.Failed);
            Assert.Contains(result.Failures!, f => f.Contains("FoundationComponentsSettings:GCDSCssIntegrity", StringComparison.Ordinal));
            Assert.Contains(result.Failures!, f => f.Contains("FoundationComponentsSettings:GCDSJavaScriptIntegrity", StringComparison.Ordinal));
            Assert.DoesNotContain(result.Failures!, f => f.Contains("FontAwesome", StringComparison.Ordinal));
        }

        [Fact]
        public void VersionOverride_WithHash_UsesConfiguredHash()
        {
            var settings = new GCFoundationComponentsSettings
            {
                FontAwesomeVersion = "9.9.9",
                FontAwesomeCssIntegrity = "sha384-configured"
            };

            Assert.Equal("sha384-configured", settings.FontAwesomeCssIntegrity);
            Assert.True(_validator.Validate(null, settings).Succeeded);
        }

        [Fact]
        public void VersionOverride_ForDisabledResource_PassesValidation()
        {
            var settings = new GCFoundationComponentsSettings
            {
                GCDSVersion = "9.9.9",
                IncludeGCDSResources = false
            };

            Assert.True(_validator.Validate(null, settings).Succeeded);
        }
    }
}
