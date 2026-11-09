using System.ComponentModel.DataAnnotations;

namespace GCFoundation.Web.Models.Components
{
    /// <summary>
    /// Model used by the Horizontal Radios documentation examples.
    /// </summary>
    public class HorizontalRadioModel : ComponentViewModel
    {
        /// <summary>
        /// Gets or sets the selected service satisfaction response.
        /// </summary>
        [Required]
        [Display(
            Name = "HorizontalRadios_Basic_Legend",
            Description = "HorizontalRadios_Basic_Hint",
            ResourceType = typeof(Resources.Components))]
        public string ServiceSatisfaction { get; set; } = string.Empty;
    }
}
