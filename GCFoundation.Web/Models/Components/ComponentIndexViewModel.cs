namespace GCFoundation.Web.Models.Components
{
    /// <summary>
    /// ViewModel representing a component for the list on the index page.
    /// </summary>
    public class ComponentIndexViewModel : ComponentViewModel
    {
        /// <summary>
        /// Description of the component.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Url to the component's documentation page.
        /// </summary>
        public string? Href { get; set; }

        /// <summary>
        /// Alternate text of the component's preview image.
        /// </summary>
        public string? ImgAlt { get; set; }

        /// <summary>
        /// Url of the component's preview image.
        /// </summary>
        public string? ImgSrc { get; set; }

        /// <summary>
        /// Identifies if the component is new. If true, a "New" badge will be displayed on the component card.
        /// </summary>
        public bool? IsNew { get; set; }

        /// <summary>
        /// Short description of the component.
        /// </summary>
        public string? ShortDescription { get; set; }
    }
}