using GCFoundation.Web.Models;

namespace GCFoundation.Web.Services.Interfaces
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
        IReadOnlyList<TagHelperReferenceGroupViewModel> BuildTagHelperGroups();
    }
}