using GCFoundation.Components.Attributes;
using GCFoundation.Web.Models.Components;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GCFoundation.Web.Models
{
    /// <summary>
    /// ViewModel for testing form validation with multiple input types.
    /// </summary>
    public class FormTestViewModel : ComponentViewModel
    {
        /// <summary>
        /// The full name of the user.
        /// </summary>
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Form_FullName_Label", ResourceType = typeof(Resources.Components))]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// The user's email address.
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Form_Email_Label", ResourceType = typeof(Resources.Components))]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The user's password.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Form_Password_Label", ResourceType = typeof(Resources.Components))]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// The user's website URL.
        /// </summary>
        [Required]
        [DataType(DataType.Url, ErrorMessageResourceType = typeof(Resources.Components), ErrorMessageResourceName = "InvalidUrl")]
        [Display(Name = "Form_Website_Label", ResourceType = typeof(Resources.Components))]
        public string Website { get; set; } = string.Empty;

        /// <summary>
        /// The user's age. Must be between 18 and 100.
        /// </summary>
        [Required]
        [Range(18, 100, ErrorMessage = "Age must be between 18 and 100.")]
        [Display(Name = "Form_Age_Label", ResourceType = typeof(Resources.Components))]
        public int? Age { get; set; }

        /// <summary>
        /// The user's date of birth.
        /// </summary>
        [DataType(DataType.Date)]
        [Display(Name = "Form_DateOfBirth_Label", Description = "Form_DateOfBirth_Hint", ResourceType = typeof(Resources.Components))]
        [DateFormat("full")]
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// A short biography of the user.
        /// </summary>
        [Required]
        [MinLength(10)]
        [MaxLength(2000)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Form_Bio_Label", Description = "Form_Bio_Hint", ResourceType = typeof(Resources.Components))]
        public string Bio { get; set; } = string.Empty;

        /// <summary>
        /// The country selected by the user.
        /// </summary>
        [Required]
        [Display(Name = "Form_Country_Label", Description = "Form_Country_Hint", ResourceType = typeof(Resources.Components))]
        public string? SelectedCountry { get; set; }

        /// <summary>
        /// Available country options.
        /// </summary>
        public IEnumerable<SelectListItem> CountryOptions { get; set; } =
        [
            new() { Value = "CA", Text = "Canada" },
            new() { Value = "US", Text = "United States" },
            new() { Value = "FR", Text = "France" },
            new() { Value = "DE", Text = "Germany" }
        ];

        /// <summary>
        /// The country selected with the searchable select example.
        /// </summary>
        [Required]
        [Display(Name = "Form_SearchableCountry_Label", Description = "Form_SearchableCountry_Hint", ResourceType = typeof(Resources.Components))]
        public string? SearchableCountry { get; set; }

        /// <summary>
        /// The gender selected by the user.
        /// </summary>
        [Required]
        [Display(Name = "Form_Gender_Label", ResourceType = typeof(Resources.Components))]
        public string Gender { get; set; } = string.Empty;

        /// <summary>
        /// Available gender options.
        /// </summary>
        public IEnumerable<SelectListItem> GenderOptions { get; set; } =
        [
            new() { Value = "Male", Text = "Male" },
            new() { Value = "Female", Text = "Female" },
            new() { Value = "Other", Text = "Other" }
        ];

        /// <summary>
        /// Indicates whether the user agrees to the terms.
        /// </summary>
        [Required]
        [Display(Name = "Form_AgreeToTerms_Label", Description = "Form_AgreeToTerms_Hint", ResourceType = typeof(Resources.Components))]
        public bool AgreeToTerms { get; set; }

        /// <summary>
        /// Indicates whether the user wants to receive the newsletter.
        /// </summary>
        [Display(Name = "Form_Newsletter_Label", Description = "Form_Newsletter_Hint", ResourceType = typeof(Resources.Components))]
        public bool ReceiveNewsletter { get; set; }


        /// <summary>
        /// The list of interests selected by the user.
        /// </summary>
        [Required]
        [Display(Name = "Form_Interests_Label", ResourceType = typeof(Resources.Components))]
        public IEnumerable<string> SelectedInterests { get; set; } = new List<string>();

        /// <summary>
        /// Available interest options.
        /// </summary>
        public IEnumerable<SelectListItem> InterestOptions { get; set; } =
        [
            new() { Value = "sports", Text = "Sports" },
            new() { Value = "music", Text = "Music" },
            new() { Value = "travel", Text = "Travel" },
            new() { Value = "reading", Text = "Reading" }
        ];

        /// <summary>
        /// The interests selected with the searchable select example.
        /// </summary>
        [Required]
        [Display(Name = "Form_SearchableInterests_Label", Description = "Form_SearchableInterests_Hint", ResourceType = typeof(Resources.Components))]
        public IEnumerable<string> SearchableInterests { get; set; } = new List<string>();
    }
}
