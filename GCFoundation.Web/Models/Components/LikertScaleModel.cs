using System.ComponentModel.DataAnnotations;

namespace GCFoundation.Web.Models.Components
{
    /// <summary>
    /// Model used by the Likert Scale documentation examples.
    /// </summary>
    public class LikertScaleModel : ComponentViewModel
    {
        /// <summary>
        /// Gets or sets the selected service satisfaction response.
        /// </summary>
        [Required]
        [Display(Name = "LikertScale_Basic_Legend", Description = "LikertScale_Basic_Hint", ResourceType = typeof(Resources.Components))]
        public string ServiceSatisfaction { get; set; } = string.Empty;
    }
}
