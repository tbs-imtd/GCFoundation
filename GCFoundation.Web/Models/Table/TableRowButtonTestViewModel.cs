using GCFoundation.Components.Attributes;
using GCFoundation.Components.Attributes.Table;
using System.ComponentModel.DataAnnotations;

namespace GCFoundation.Web.Models.Table
{
    /// <summary>
    /// Sample row model for demonstrating how to bind a row field to a button attribute
    /// using <c>data-bind-template-{attribute}</c> inside a slotted cell template.
    /// </summary>
    public class TableRowButtonTestViewModel
    {
        /// <summary>
        /// The submission's unique identifier. Used as the value bound to the button's
        /// <c>button-id</c> attribute via <c>data-bind-button-id</c>.
        /// </summary>
        [DataType(DataType.Text)]
        public string SubmissionId { get; set; } = string.Empty;

        /// <summary>
        /// The name of the person who made the submission. Rendered as the row header.
        /// </summary>
        [DataType(DataType.Text)]
        [TableColumnDefinition(Name = "Table_Submitter_Name_Header", ResourceType = typeof(Resources.Components), RowHeader = true)]
        public string SubmitterName { get; set; } = string.Empty;

        /// <summary>
        /// The date and time the submission was made. Formatted as <c>yyyy-MM-dd HH:mm</c>.
        /// </summary>
        [DataType(DataType.Date)]
        [TableColumnDefinition(Name = "Table_Date_Submitted_Header", ResourceType = typeof(Resources.Components))]
        [DateFormat("yyyy-MM-dd HH:mm")]
        public DateTime DateSubmitted { get; set; }

        /// <summary>
        /// The name of the reviewer assigned to the submission.
        /// </summary>
        [DataType(DataType.Text)]
        [TableColumnDefinition(Name = "Table_Assigned_Reviewer_Header", ResourceType = typeof(Resources.Components))]
        public string AssignedReviewer { get; set; } = string.Empty;

        /// <summary>
        /// The action column. Rendered as a slotted danger <c>gcds-button</c> whose
        /// <c>button-id</c> attribute is bound to <c>SubmissionId</c> via <c>data-bind-button-id</c>.
        /// </summary>
        [TableColumnDefinition(Name = "Table_Actions_Header", ResourceType = typeof(Resources.Components), Slotted = true)]
        public string Action { get; set; } = string.Empty;
    }
}
