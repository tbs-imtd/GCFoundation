using GCFoundation.Components.Models;
using GCFoundation.Components.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace GCFoundation.Components.Services
{
    /// <summary>
    /// Discovers tag helpers at runtime and maps them to localized reference entries.
    /// </summary>
    /// <typeparam name="T">The type used to retrieve localization resources.</typeparam>
    public class TagHelperCatalogService<T> : ITagHelperCatalogService
    {
        // Matches ASP.NET's HtmlConventions.ToHtmlCase so RefreshURL becomes refresh-url.
        private static readonly Regex HtmlAttributeNameRegex = new(
            "(?<!^)((?<=[a-zA-Z0-9])[A-Z][a-z])|((?<=[a-z])[A-Z])",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly List<DiscoveredTagHelper> discoveredTagHelpers;
        private readonly ResourceManager resourceManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagHelperCatalogService{T}"/> class.
        /// </summary>
        // Loads the resource file and scans tag helpers once at startup.
        public TagHelperCatalogService()
        {
            resourceManager = ResolveResourceManager();
            discoveredTagHelpers = DiscoverTagHelpers();
        }

        // Adds an attribute name to the list if it is not already there.
        private static void AddKeyProperty(List<string> names, HashSet<string> seen, string attributeName)
        {
            if (string.IsNullOrWhiteSpace(attributeName) || !seen.Add(attributeName))
            {
                return;
            }

            names.Add(attributeName);
        }

        // Builds the Components.fr.resx key for a tag, e.g. fdcp-input -> Index_TagHelpers_Description_FDCP_Input.
        private static string BuildDescriptionResourceKey(string tagName)
        {
            return $"Index_TagHelpers_Description_{NormalizeTagName(tagName)}";
        }

        // Required [HtmlTargetElement] attributes first, then bindable properties on this type and its bases.
        private static List<string> BuildKeyProperties(Type type)
        {
            var names = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string requiredAttribute in type
                .GetCustomAttributes<HtmlTargetElementAttribute>(inherit: false)
                .SelectMany(attribute => ParseTargetAttributes(attribute.Attributes)))
            {
                AddKeyProperty(names, seen, requiredAttribute);
            }

            Type? currentType = type;
            while (currentType != null && currentType != typeof(TagHelper))
            {
                foreach (PropertyInfo property in currentType.GetProperties(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    if (!TryGetHtmlAttributeName(property, out string attributeName))
                    {
                        continue;
                    }

                    AddKeyProperty(names, seen, attributeName);
                }

                currentType = currentType.BaseType;
            }

            return names;
        }

        /// <inheritdoc />
        // Localizes the already-discovered helpers and groups them for the Components page.
        public IReadOnlyList<TagHelperReferenceGroupViewModel> BuildTagHelperGroups()
        {
            CultureInfo culture = CultureInfo.CurrentUICulture;
            // tryParents: false so a missing FR key is empty, not an English fallback.
            ResourceSet? frenchResources = culture.TwoLetterISOLanguageName.Equals("fr", StringComparison.OrdinalIgnoreCase)
                ? resourceManager.GetResourceSet(
                    CultureInfo.GetCultureInfo("fr"),
                    createIfNotExists: true,
                    tryParents: false)
                : null;

            var grouped = discoveredTagHelpers
                .OrderBy(item => item.TagName, StringComparer.OrdinalIgnoreCase)
                .GroupBy(item => item.GroupName, StringComparer.OrdinalIgnoreCase);

            var results = new List<TagHelperReferenceGroupViewModel>();

            foreach (IGrouping<string, DiscoveredTagHelper> group in grouped)
            {
                List<TagHelperReferenceViewModel> items = new();

                foreach (DiscoveredTagHelper tagHelper in group)
                {
                    string description = tagHelper.EnglishDescription;
                    if (frenchResources is not null)
                    {
                        string? frenchDescription = frenchResources.GetString(BuildDescriptionResourceKey(tagHelper.TagName));
                        if (string.IsNullOrWhiteSpace(frenchDescription))
                        {
                            continue;
                        }

                        description = frenchDescription;
                    }

                    TagHelperReferenceViewModel item = new()
                    {
                        Title = $"<{tagHelper.TagName}>",
                        Description = description,
                        UsageSnippet = tagHelper.UsageSnippet
                    };
                    foreach (string propertyName in tagHelper.KeyProperties)
                    {
                        item.KeyProperties.Add(propertyName);
                    }

                    items.Add(item);
                }

                if (items.Count == 0)
                {
                    continue;
                }

                TagHelperReferenceGroupViewModel groupViewModel = new()
                {
                    Title = ResolveGroupTitle(group.Key, culture)
                };
                foreach (TagHelperReferenceViewModel item in items)
                {
                    groupViewModel.Items.Add(item);
                }

                results.Add(groupViewModel);
            }

            return results;
        }

        // Scans FDCP/GCDS tag helper types and caches tag name, description, attributes, and snippet.
        private static List<DiscoveredTagHelper> DiscoverTagHelpers()
        {
            Assembly componentsAssembly = typeof(TagHelperCatalogService<>).Assembly;
            Dictionary<string, XmlMemberDocumentation> xmlDocumentation = LoadXmlDocumentation(componentsAssembly);
            IEnumerable<Type> tagHelperTypes = componentsAssembly
                .GetTypes()
                .Where(type =>
                    !type.IsAbstract &&
                    typeof(TagHelper).IsAssignableFrom(type) &&
                    type.Namespace != null &&
                    (type.Namespace.StartsWith("GCFoundation.Components.TagHelpers.FDCP", StringComparison.Ordinal) ||
                     type.Namespace.StartsWith("GCFoundation.Components.TagHelpers.GCDS", StringComparison.Ordinal)));

            // One entry per tag name; extra [HtmlTargetElement] declarations on the same helper are skipped.
            var discovered = new Dictionary<string, DiscoveredTagHelper>(StringComparer.OrdinalIgnoreCase);

            foreach (Type type in tagHelperTypes)
            {
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
                    if (discovered.ContainsKey(tagName))
                    {
                        continue;
                    }

                    discovered.Add(
                        tagName,
                        new DiscoveredTagHelper(
                            groupName,
                            tagName,
                            ResolveEnglishDescription(type, xmlDocumentation),
                            keyProperties,
                            ResolveUsageSnippet(type, xmlDocumentation)));
                }
            }

            return discovered.Values.ToList();
        }

        // Reads the usage snippet from XML <example>, or remarks/code if example is missing.
        private static string ExtractExample(XElement member)
        {
            XElement? example = member.Element("example");
            if (example is not null)
            {
                string rawExample = example.Element("code")?.Value ?? example.Value;
                string normalizedExample = NormalizeExample(rawExample);
                if (!string.IsNullOrWhiteSpace(normalizedExample))
                {
                    return normalizedExample;
                }
            }

            XElement? remarksCode = member.Element("remarks")?.Element("code");
            return remarksCode is null ? string.Empty : NormalizeExample(remarksCode.Value);
        }

        // Indexes GCFoundation.Components.xml (from /// comments) by member name for summaries and examples.
        private static Dictionary<string, XmlMemberDocumentation> LoadXmlDocumentation(Assembly componentsAssembly)
        {
            string xmlDocumentationPath = Path.ChangeExtension(componentsAssembly.Location, ".xml");
            if (!File.Exists(xmlDocumentationPath))
            {
                return new Dictionary<string, XmlMemberDocumentation>(StringComparer.Ordinal);
            }

            XDocument xmlDocument = XDocument.Load(xmlDocumentationPath);
            var documentation = new Dictionary<string, XmlMemberDocumentation>(StringComparer.Ordinal);

            foreach (XElement member in xmlDocument.Descendants("member"))
            {
                string? name = member.Attribute("name")?.Value;
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                documentation[name] = new XmlMemberDocumentation(
                    NormalizeSummary(member.Element("summary")?.Value),
                    ExtractExample(member));
            }

            return documentation;
        }

        // Strips shared leading indent from XML <example> lines so snippets are left-aligned.
        private static string NormalizeExample(string? example)
        {
            if (string.IsNullOrWhiteSpace(example))
            {
                return string.Empty;
            }

            string[] lines = example.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
            List<string> contentLines = lines
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            if (contentLines.Count == 0)
            {
                return string.Empty;
            }

            int indent = contentLines.Min(line => line.Length - line.TrimStart().Length);
            return string.Join(
                "\n",
                contentLines.Select(line => line.Length >= indent ? line[indent..] : line.TrimStart()));
        }

        // Collapses XML <summary> whitespace into a single line for the catalog description.
        private static string NormalizeSummary(string? summary)
        {
            if (string.IsNullOrWhiteSpace(summary))
            {
                return string.Empty;
            }

            string normalized = summary
                .Replace("\r", " ", StringComparison.Ordinal)
                .Replace("\n", " ", StringComparison.Ordinal)
                .Trim();
            while (normalized.Contains("  ", StringComparison.Ordinal))
            {
                normalized = normalized.Replace("  ", " ", StringComparison.Ordinal);
            }

            return normalized;
        }

        // fdcp-error-summary -> FDCP_ErrorSummary for Index_TagHelpers_Description_* resx keys.
        private static string NormalizeTagName(string tagName)
        {
            string[] parts = tagName
                .Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length == 0)
            {
                return tagName;
            }

            string prefix = parts[0].Equals("fdcp", StringComparison.OrdinalIgnoreCase)
                ? "FDCP"
                : parts[0].Equals("gcds", StringComparison.OrdinalIgnoreCase)
                    ? "GCDS"
                    : char.ToUpperInvariant(parts[0][0]) + parts[0][1..];

            string remainder = string.Concat(
                parts.Skip(1).Select(part => char.ToUpperInvariant(part[0]) + part[1..]));

            return remainder.Length == 0 ? prefix : $"{prefix}_{remainder}";
        }

        // Splits HtmlTargetElement.Attributes values such as "for, items" into individual names.
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

        // Uses the class XML <summary> as the English catalog description.
        private static string ResolveEnglishDescription(
            Type tagHelperType,
            IReadOnlyDictionary<string, XmlMemberDocumentation> xmlDocumentation)
        {
            string memberName = $"T:{tagHelperType.FullName}";
            if (xmlDocumentation.TryGetValue(memberName, out XmlMemberDocumentation? documentation) &&
                !string.IsNullOrWhiteSpace(documentation.Summary))
            {
                return documentation.Summary;
            }

            return $"Tag helper description for {tagHelperType.Name}.";
        }

        // Localized heading for the FDCP or GCDS group on the Components page.
        private string ResolveGroupTitle(string groupName, CultureInfo culture)
        {
            return groupName.Equals("FDCP", StringComparison.OrdinalIgnoreCase)
                ? resourceManager.GetString("Index_TagHelpers_Group_FDCP_Title", culture) ?? "FDCP Tag Helpers"
                : resourceManager.GetString("Index_TagHelpers_Group_GCDS_Title", culture) ?? "GCDS Tag Helpers";
        }

        // T is the resx Designer class (e.g. Web.Resources.Components) passed in at DI registration.
        private static ResourceManager ResolveResourceManager()
        {
            PropertyInfo? resourceManagerProperty = typeof(T).GetProperty("ResourceManager", BindingFlags.Public | BindingFlags.Static);
            if (resourceManagerProperty?.GetValue(null) is ResourceManager manager)
            {
                return manager;
            }

            throw new InvalidOperationException(
                $"Type '{typeof(T).FullName}' must expose a public static ResourceManager property.");
        }

        // Uses the class XML <example> as the catalog usage snippet.
        private static string ResolveUsageSnippet(
            Type tagHelperType,
            IReadOnlyDictionary<string, XmlMemberDocumentation> xmlDocumentation)
        {
            string memberName = $"T:{tagHelperType.FullName}";
            if (xmlDocumentation.TryGetValue(memberName, out XmlMemberDocumentation? documentation) &&
                !string.IsNullOrWhiteSpace(documentation.Example))
            {
                return documentation.Example;
            }

            return string.Empty;
        }

        // Converts a C# property name (CurrentStep) to the Razor attribute name (current-step).
        private static string ToHtmlAttributeName(string propertyName)
        {
            #pragma warning disable CA1308 // HTML attribute names are lowercase kebab-case.
            return HtmlAttributeNameRegex.Replace(propertyName, "-$1$2").ToLowerInvariant();
            #pragma warning restore CA1308
        }

        // Skips non-bindable properties; uses [HtmlAttributeName] when set, otherwise kebab-case.
        private static bool TryGetHtmlAttributeName(PropertyInfo property, out string attributeName)
        {
            attributeName = string.Empty;

            if (!property.CanRead || !property.CanWrite || property.GetIndexParameters().Length > 0)
            {
                return false;
            }

            if (property.GetCustomAttribute<HtmlAttributeNotBoundAttribute>() != null ||
                property.GetCustomAttribute<ViewContextAttribute>() != null ||
                property.GetCustomAttribute<ObsoleteAttribute>() != null)
            {
                return false;
            }

            HtmlAttributeNameAttribute? htmlAttributeName = property.GetCustomAttribute<HtmlAttributeNameAttribute>();
            if (htmlAttributeName is not null)
            {
                // Skip dictionary prefixes like attr-* which are not a single catalog attribute.
                if (string.IsNullOrWhiteSpace(htmlAttributeName.Name) || htmlAttributeName.Name.Contains('*', StringComparison.Ordinal))
                {
                    return false;
                }

                attributeName = htmlAttributeName.Name;
                return true;
            }

            attributeName = ToHtmlAttributeName(property.Name);
            return !string.IsNullOrWhiteSpace(attributeName);
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

            public string EnglishDescription { get; }
            public string GroupName { get; }
            public List<string> KeyProperties { get; }
            public string TagName { get; }
            public string UsageSnippet { get; }
        }

        private sealed class XmlMemberDocumentation
        {
            public XmlMemberDocumentation(string summary, string example)
            {
                Summary = summary;
                Example = example;
            }

            public string Example { get; }
            public string Summary { get; }
        }
    }
}
