using System.ComponentModel.DataAnnotations;

namespace GCFoundation.Web.Models.Components
{
    /// <summary>
    /// Model for the Autocomplete sample that binds the component with <c>for</c>.
    /// </summary>
    public class AutocompleteBoundSampleModel
    {
        /// <summary>
        /// The value of the chosen employee. The label, hint and required state come from these attributes.
        /// </summary>
        [Required]
        [Display(Name = "Autocomplete_Sample_Bound_Label", Description = "Autocomplete_Sample_Bound_Hint", ResourceType = typeof(Resources.Components))]
        public string? SupervisorId { get; set; }

        /// <summary>
        /// The display text of the chosen employee, shown without calling the data source.
        /// </summary>
        public string? SupervisorName { get; set; }
    }
}
