using GCFoundation.Web.Models.Components;

namespace GCFoundation.Web.Services
{
    /// <summary>
    /// Provides dynamically discovered tag helper references.
    /// </summary>
    public interface ITagHelperCatalogService
    {
        /// <summary>
        /// Builds tag helper reference groups for the current UI culture.
        /// </summary>
        /// <returns>Grouped tag helper references.</returns>
        List<TagHelperReferenceGroupViewModel> BuildTagHelperGroups();
    }
}
