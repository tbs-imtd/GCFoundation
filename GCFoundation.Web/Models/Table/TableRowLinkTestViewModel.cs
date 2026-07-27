using GCFoundation.Components.Attributes;
using GCFoundation.Components.Attributes.Table;
using System.ComponentModel.DataAnnotations;

namespace GCFoundation.Web.Models.Table
{
    /// <summary>
    /// Sample row model for demonstrating how to compose link text and a destination URL
    /// from row field values using <c>data-bind-template</c> and <c>data-bind-template-href</c>
    /// inside a slotted cell template.
    /// </summary>
    public class TableRowLinkTestViewModel
    {
        /// <summary>
        /// The submission's unique identifier.
        /// </summary>
        [DataType(DataType.Text)]
        public string SubmissionId { get; set; } = string.Empty;

        /// <summary>
        /// The name of the person who made the submission. Rendered as the row header.
        /// Also used as a <c>{submitterName}</c> token in <c>data-bind-template-href</c>
        /// to build the link's destination URL.
        /// </summary>
        [DataType(DataType.Text)]
        [TableColumnDefinition(Name = "Table_Submitter_Name_Header", ResourceType = typeof(Resources.Components), RowHeader = true)]
        public string SubmitterName { get; set; } = string.Empty;

        /// <summary>
        /// The date and time the submission was made. Formatted as <c>yyyy-MM-dd HH:mm</c>.
        /// </summary>
        [DataType(DataType.Date)]
        [DateFormat("yyyy-MM-dd HH:mm")]
        [TableColumnDefinition(Name = "Table_Date_Submitted_Header", ResourceType = typeof(Resources.Components))]
        public DateTime DateSubmitted { get; set; }

        /// <summary>
        /// The name of the reviewer assigned to the submission.
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Table_Assigned_Reviewer_Header", ResourceType = typeof(Resources.Components))]
        [TableColumnDefinition(Name = "Table_Assigned_Reviewer_Header", ResourceType = typeof(Resources.Components))]
        public string AssignedReviewer { get; set; } = string.Empty;

        /// <summary>
        /// The link label for this row. Rendered as a slotted <c>gcds-link</c> whose visible
        /// text is composed via <c>data-bind-template</c> using <c>{fieldName}</c> tokens.
        /// </summary>
        [DataType(DataType.Text)]
        [TableColumnDefinition(Name = "Table_Link_Header", ResourceType = typeof(Resources.Components), Slotted = true)]
        public string SubmissionLink { get; set; } = string.Empty;
    }
}
