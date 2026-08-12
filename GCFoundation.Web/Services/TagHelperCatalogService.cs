using GCFoundation.Web.Models.Components;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Xml.Linq;

namespace GCFoundation.Web.Services
{
    /// <summary>
    /// Discovers tag helpers at runtime and maps them to localized reference entries.
    /// </summary>
    public class TagHelperCatalogService : ITagHelperCatalogService
    {
        //List of tags already showing as Featured in Components -> Gcfoundation Components Page
        private static readonly HashSet<string> FeaturedTagNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "fdcp-badge",
            "fdcp-card",
            "fdcp-filters-box",
            "fdcp-form-builder",
            "fdcp-form",
            "fdcp-modal",
            "fdcp-page-heading",
            "fdcp-searchable-select",
            "fdcp-stepper",
            "fdcp-tabs",
            "fdcp-table"
        };

        private readonly List<DiscoveredTagHelper> discoveredTagHelpers;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagHelperCatalogService"/> class.
        /// </summary>
        public TagHelperCatalogService()
        {
            discoveredTagHelpers = DiscoverTagHelpers();
        }

        /// <inheritdoc />
        public List<TagHelperReferenceGroupViewModel> BuildTagHelperGroups()
        {
            CultureInfo culture = CultureInfo.CurrentUICulture;
            ResourceSet? frenchResources = culture.TwoLetterISOLanguageName.Equals("fr", StringComparison.OrdinalIgnoreCase)
                ? GCFoundation.Web.Resources.Components.ResourceManager.GetResourceSet(
                    CultureInfo.GetCultureInfo("fr"),
                    createIfNotExists: true,
                    tryParents: false)
                : null;

            // The catalog is already discovered once in the constructor; here we only shape it for rendering.
            var grouped = discoveredTagHelpers
                .OrderBy(item => item.TagName, StringComparer.OrdinalIgnoreCase)
                .GroupBy(item => item.GroupName, StringComparer.OrdinalIgnoreCase);

            var results = new List<TagHelperReferenceGroupViewModel>();

            foreach (IGrouping<string, DiscoveredTagHelper> group in grouped)
            {
                // Each group (FDCP/GCDS) gets its own list of renderable items.
                List<TagHelperReferenceViewModel> items = new();

                foreach (DiscoveredTagHelper tagHelper in group)
                {
                    // English XML summary is the baseline description.
                    string description = tagHelper.EnglishDescription;
                    if (frenchResources is not null)
                    {
                        string? frenchDescription = frenchResources.GetString(BuildDescriptionResourceKey(tagHelper.TagName));
                        if (string.IsNullOrWhiteSpace(frenchDescription))
                        {
                            // French pages only show entries that have a French description key.
                            continue;
                        }

                        description = frenchDescription;
                    }

                    items.Add(new TagHelperReferenceViewModel
                    {
                        Title = $"<{tagHelper.TagName}>",
                        Description = description,
                        KeyProperties = tagHelper.KeyProperties,
                        UsageSnippet = tagHelper.UsageSnippet
                    });
                }

                if (items.Count == 0)
                {
                    continue;
                }

                // Emit one view-model group consumed by the Razor section loop.
                results.Add(new TagHelperReferenceGroupViewModel
                {
                    Title = ResolveGroupTitle(group.Key, culture),
                    Items = items
                });
            }

            return results;
        }

        private static List<DiscoveredTagHelper> DiscoverTagHelpers()
        {
            // Discover tag helper types from the shared components assembly.
            Assembly componentsAssembly = typeof(GCFoundation.Components.TagHelpers.GCDS.ButtonTagHelper).Assembly;
            // XML summaries are used as the English description baseline.
            IReadOnlyDictionary<string, string> xmlTypeSummaries = LoadTypeSummaries(componentsAssembly);
            IEnumerable<Type> tagHelperTypes = componentsAssembly
                .GetTypes()
                .Where(type =>
                    !type.IsAbstract &&
                    typeof(TagHelper).IsAssignableFrom(type) &&
                    type.Namespace != null &&
                    (type.Namespace.StartsWith("GCFoundation.Components.TagHelpers.FDCP", StringComparison.Ordinal) ||
                     type.Namespace.StartsWith("GCFoundation.Components.TagHelpers.GCDS", StringComparison.Ordinal)));

            // Keyed by tag name so repeated HtmlTargetElement declarations collapse to one entry.
            var discovered = new Dictionary<string, DiscoveredTagHelper>(StringComparer.OrdinalIgnoreCase);

            foreach (Type type in tagHelperTypes)
            {
                // Keep a simple bucket used by the page section headings.
                string groupName = type.Namespace!.Contains(".FDCP", StringComparison.Ordinal) ? "FDCP" : "GCDS";

                List<string> tagNames = type
                    .GetCustomAttributes<HtmlTargetElementAttribute>(inherit: false)
                    .Select(attribute => attribute.Tag)
                    .Where(tag => !string.IsNullOrWhiteSpace(tag))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()!;

                if (tagNames.Count == 0)
                {
                    continue;
                }

                List<string> keyProperties = BuildKeyProperties(type);

                foreach (string tagName in tagNames)
                {
                    // Featured tags already have dedicated component demos/cards.
                    if (FeaturedTagNames.Contains(tagName) || discovered.ContainsKey(tagName))
                    {
                        continue;
                    }

                    // One model per unique tag name with prebuilt metadata for the view.
                    discovered.Add(
                        tagName,
                        new DiscoveredTagHelper(
                            groupName,
                            tagName,
                            ResolveEnglishDescription(type, xmlTypeSummaries),
                            keyProperties,
                            BuildUsageSnippet(tagName, keyProperties)));
                }
            }

            return discovered.Values.ToList();
        }

        private static IReadOnlyDictionary<string, string> LoadTypeSummaries(Assembly componentsAssembly)
        {
            string xmlDocumentationPath = Path.ChangeExtension(componentsAssembly.Location, ".xml");
            if (!File.Exists(xmlDocumentationPath))
            {
                return new Dictionary<string, string>(StringComparer.Ordinal);
            }

            XDocument xmlDocument = XDocument.Load(xmlDocumentationPath);
            return xmlDocument
                .Descendants("member")
                .Select(member => new
                {
                    Name = member.Attribute("name")?.Value,
                    Summary = NormalizeSummary(member.Element("summary")?.Value)
                })
                .Where(item => !string.IsNullOrWhiteSpace(item.Name) && !string.IsNullOrWhiteSpace(item.Summary))
                .ToDictionary(item => item.Name!, item => item.Summary!, StringComparer.Ordinal);
        }

        private static string ResolveEnglishDescription(
            Type tagHelperType,
            IReadOnlyDictionary<string, string> xmlTypeSummaries)
        {
            string memberName = $"T:{tagHelperType.FullName}";
            if (xmlTypeSummaries.TryGetValue(memberName, out string? summary) &&
                !string.IsNullOrWhiteSpace(summary))
            {
                return summary;
            }

            return $"Tag helper description for {tagHelperType.Name}.";
        }

        private static string NormalizeSummary(string? summary)
        {
            if (string.IsNullOrWhiteSpace(summary))
            {
                return string.Empty;
            }

            string normalized = summary.Replace("\r", " ").Replace("\n", " ").Trim();
            while (normalized.Contains("  ", StringComparison.Ordinal))
            {
                normalized = normalized.Replace("  ", " ", StringComparison.Ordinal);
            }

            return normalized;
        }

        private static List<string> BuildKeyProperties(Type type)
        {
            List<string> targetAttributes = type
                .GetCustomAttributes<HtmlTargetElementAttribute>(inherit: false)
                .SelectMany(attribute => ParseTargetAttributes(attribute.Attributes))
                .ToList();

            List<string> propertyAttributes = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Select(property => property.GetCustomAttribute<HtmlAttributeNameAttribute>()?.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!)
                .ToList();

            return targetAttributes
                .Concat(propertyAttributes)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(6)
                .ToList();
        }

        private static IEnumerable<string> ParseTargetAttributes(string? attributes)
        {
            if (string.IsNullOrWhiteSpace(attributes))
            {
                return Array.Empty<string>();
            }

            return attributes
                .Split([','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(attribute => !string.IsNullOrWhiteSpace(attribute));
        }

        private static string BuildUsageSnippet(string tagName, List<string> keyProperties)
        {
            if (keyProperties.Count == 0)
            {
                return $"<{tagName}></{tagName}>";
            }

            string inlineAttributes = string.Join(" ", keyProperties.Take(3).Select(attribute => $"{attribute}=\"...\""));
            return $"<{tagName} {inlineAttributes}></{tagName}>";
        }

        private static string BuildDescriptionResourceKey(string tagName)
        {
            return $"Index_TagHelpers_Description_{NormalizeTagName(tagName)}";
        }

        private static string NormalizeTagName(string tagName)
        {
            string[] parts = tagName
                .Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return string.Join(
                "_",
                parts.Select(part => part.Equals("fdcp", StringComparison.OrdinalIgnoreCase)
                    ? "FDCP"
                    : part.Equals("gcds", StringComparison.OrdinalIgnoreCase)
                        ? "GCDS"
                        : char.ToUpperInvariant(part[0]) + part[1..]));
        }

        private static string ResolveGroupTitle(string groupName, CultureInfo culture)
        {
            return groupName.Equals("FDCP", StringComparison.OrdinalIgnoreCase)
                ? GCFoundation.Web.Resources.Components.ResourceManager.GetString("Index_TagHelpers_Group_FDCP_Title", culture) ?? "FDCP Tag Helpers"
                : GCFoundation.Web.Resources.Components.ResourceManager.GetString("Index_TagHelpers_Group_GCDS_Title", culture) ?? "GCDS Tag Helpers";
        }

        private sealed class DiscoveredTagHelper
        {
            public DiscoveredTagHelper(
                string groupName,
                string tagName,
                string englishDescription,
                List<string> keyProperties,
                string usageSnippet)
            {
                GroupName = groupName;
                TagName = tagName;
                EnglishDescription = englishDescription;
                KeyProperties = keyProperties;
                UsageSnippet = usageSnippet;
            }

            public string GroupName { get; }
            public string TagName { get; }
            public string EnglishDescription { get; }
            public List<string> KeyProperties { get; }
            public string UsageSnippet { get; }
        }
    }
}
