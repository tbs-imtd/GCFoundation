using System.Reflection;
using System.Text.RegularExpressions;
using GCFoundation.Common.Settings;
using GCFoundation.Components.TagHelpers.GCDS;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GCFoundation.Tests.Components.Tests.TagHelpers.GCDS
{
    /// <summary>
    /// Ensures the GCDS tag helpers accept the same attribute names as the GCDS web components,
    /// so the GCDS documentation can be used as-is with GCFoundation.
    /// </summary>
    public partial class GcdsAttributeParityTests
    {
        private const string GcdsAttributesVersion = "1.7.0";

        private static readonly Dictionary<string, HashSet<string>> GcdsAttributes = new(StringComparer.Ordinal)
        {
            ["gcds-alert"] = ["alert-role", "container", "heading", "hide-close-btn", "hide-role-icon", "is-fixed"],
            ["gcds-breadcrumbs"] = ["hide-canada-link"],
            ["gcds-breadcrumbs-item"] = ["href"],
            ["gcds-button"] = ["button-id", "button-role", "disabled", "download", "href", "name", "rel", "size", "target", "type", "value"],
            ["gcds-card"] = ["badge", "card-title", "card-title-tag", "description", "href", "img-alt", "img-src", "rel", "target"],
            ["gcds-checkboxes"] = ["autofocus", "disabled", "error-message", "form", "hide-label", "hide-legend", "hint", "legend", "name", "options", "required", "validate-on", "value"],
            ["gcds-container"] = ["alignment", "border", "layout", "margin", "padding", "size", "tag"],
            ["gcds-date-input"] = ["autocomplete", "autofocus", "disabled", "error-message", "form", "format", "hint", "legend", "max", "min", "name", "required", "validate-on", "value"],
            ["gcds-date-modified"] = ["type"],
            ["gcds-details"] = ["details-title", "open"],
            ["gcds-error-message"] = ["message-id"],
            ["gcds-error-summary"] = ["error-links", "heading", "listen"],
            ["gcds-fieldset"] = ["hint", "legend", "legend-size"],
            ["gcds-file-uploader"] = ["accept", "autofocus", "disabled", "error-message", "form", "hide-label", "hint", "label", "multiple", "name", "required", "uploader-id", "validate-on", "value"],
            ["gcds-footer"] = ["contextual-heading", "contextual-links", "display", "sub-links"],
            ["gcds-grid"] = ["align-content", "align-items", "alignment", "columns", "columns-desktop", "columns-tablet", "container", "display", "equal-row-height", "gap", "gap-desktop", "gap-tablet", "justify-content", "justify-items", "place-content", "place-items", "tag"],
            ["gcds-header"] = ["lang-href", "signature-has-link", "skip-to-href"],
            ["gcds-heading"] = ["character-limit", "heading-role", "margin-bottom", "margin-top", "tag"],
            ["gcds-icon"] = ["label", "margin-left", "margin-right", "name", "size"],
            ["gcds-input"] = ["autocomplete", "autofocus", "disabled", "error-message", "form", "hide-label", "hint", "input-id", "inputmode", "label", "max", "maxlength", "min", "minlength", "name", "pattern", "readonly", "required", "size", "step", "suggestions", "type", "validate-on", "value"],
            ["gcds-lang-toggle"] = ["href"],
            ["gcds-link"] = ["display", "download", "external", "href", "link-role", "rel", "size", "target", "type"],
            ["gcds-nav-group"] = ["close-trigger", "menu-label", "open", "open-trigger"],
            ["gcds-nav-link"] = ["current", "href"],
            ["gcds-notice"] = ["notice-role", "notice-title", "notice-title-tag"],
            ["gcds-pagination"] = ["current-page", "display", "label", "next-href", "next-label", "previous-href", "previous-label", "total-pages", "url"],
            ["gcds-radios"] = ["autofocus", "disabled", "error-message", "form", "hide-legend", "hint", "legend", "name", "options", "required", "validate-on", "value"],
            ["gcds-search"] = ["action", "method", "name", "placeholder", "search-id", "suggested", "value"],
            ["gcds-select"] = ["autocomplete", "autofocus", "default-value", "disabled", "error-message", "form", "hide-label", "hint", "label", "name", "required", "select-id", "validate-on", "value"],
            ["gcds-side-nav"] = ["label"],
            ["gcds-signature"] = ["has-link", "type", "variant"],
            ["gcds-sr-only"] = ["tag"],
            ["gcds-stepper"] = ["current-step", "tag", "total-steps"],
            ["gcds-table"] = ["columns", "data", "filter", "filter-value", "pagination", "pagination-current-page", "pagination-size", "pagination-size-options", "sort"],
            ["gcds-text"] = ["character-limit", "display", "margin-bottom", "margin-top", "size", "text-role"],
            ["gcds-textarea"] = ["autofocus", "cols", "disabled", "error-message", "form", "hide-label", "hide-limit", "hint", "label", "maxlength", "minlength", "name", "required", "rows", "textarea-id", "validate-on", "value"],
            ["gcds-top-nav"] = ["alignment", "label"],
            ["gcds-topic-menu"] = ["home"],
        };

        /// <summary>
        /// GCFoundation-only attributes used for model binding on form components.
        /// </summary>
        private static readonly HashSet<string> FoundationAttributes = ["for", "id"];

        /// <summary>
        /// Known divergences from GCDS that are not yet resolved.
        /// </summary>
        private static readonly Dictionary<string, HashSet<string>> KnownExceptions = new(StringComparer.Ordinal)
        {
            ["gcds-fieldset"] = ["fieldset-id", "disabled", "error-message", "required", "validate-on"],
        };

        public static TheoryData<Type> GcdsTagHelperTypes()
        {
            var data = new TheoryData<Type>();
            foreach (Type type in typeof(BaseTagHelper).Assembly.GetTypes()
                .Where(t => t.Namespace == typeof(BaseTagHelper).Namespace && !t.IsAbstract && typeof(ITagHelper).IsAssignableFrom(t))
                .OrderBy(t => t.Name, StringComparer.Ordinal))
            {
                data.Add(type);
            }
            return data;
        }

        [Fact]
        public void GcdsAttributeList_MatchesShippedGcdsVersion()
        {
            Assert.True(
                CdnIntegrityManifest.Current.Gcds.Version == GcdsAttributesVersion,
                $"GCDS was upgraded to {CdnIntegrityManifest.Current.Gcds.Version}. Refresh {nameof(GcdsAttributes)} from @gcds-core/components dist/types/components.d.ts and update {nameof(GcdsAttributesVersion)}.");
        }

        [Theory]
        [MemberData(nameof(GcdsTagHelperTypes))]
        public void TagHelper_TargetsGcdsElement(Type tagHelperType)
        {
            string? tag = GetTargetTag(tagHelperType);

            Assert.True(tag != null && GcdsAttributes.ContainsKey(tag),
                $"{tagHelperType.Name} must target a GCDS element with [HtmlTargetElement(\"gcds-...\")]; found '{tag ?? "(none)"}'.");
        }

        [Theory]
        [MemberData(nameof(GcdsTagHelperTypes))]
        public void TagHelper_BindsOnlyGcdsAttributeNames(Type tagHelperType)
        {
            string? tag = GetTargetTag(tagHelperType);
            if (tag == null || !GcdsAttributes.TryGetValue(tag, out HashSet<string>? allowed))
            {
                return;
            }

            KnownExceptions.TryGetValue(tag, out HashSet<string>? exceptions);

            string[] unknown = GetBoundAttributeNames(tagHelperType)
                .Where(name => !allowed.Contains(name) && !FoundationAttributes.Contains(name) && exceptions?.Contains(name) != true)
                .ToArray();

            Assert.True(unknown.Length == 0,
                $"{tagHelperType.Name} binds attributes that <{tag}> does not define: {string.Join(", ", unknown)}. " +
                "Use the GCDS attribute name, and keep any legacy name as an [Obsolete] alias.");
        }

        private static string? GetTargetTag(Type type) =>
            type.GetCustomAttributes<HtmlTargetElementAttribute>().Select(a => a.Tag).FirstOrDefault();

        private static IEnumerable<string> GetBoundAttributeNames(Type type) =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.SetMethod?.IsPublic == true
                    && p.GetCustomAttribute<HtmlAttributeNotBoundAttribute>() == null
                    && p.GetCustomAttribute<ViewContextAttribute>() == null
                    && p.GetCustomAttribute<ObsoleteAttribute>() == null)
                .Select(p => p.GetCustomAttribute<HtmlAttributeNameAttribute>()?.Name ?? ToHtmlCase(p.Name));

        private static string ToHtmlCase(string name) =>
            HtmlCaseRegex().Replace(name, "-$1$2").ToLowerInvariant();

        [GeneratedRegex("(?<!^)((?<=[a-zA-Z0-9])[A-Z][a-z])|((?<=[a-z])[A-Z])")]
        private static partial Regex HtmlCaseRegex();
    }
}
