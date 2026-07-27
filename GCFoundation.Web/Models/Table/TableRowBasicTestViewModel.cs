using GCFoundation.Components.Attributes;
using GCFoundation.Components.Attributes.Table;
using System.ComponentModel.DataAnnotations;

namespace GCFoundation.Web.Models.Table
{
    /// <summary>
    /// Sample row model for demonstrating <c>fdcp-table</c>'s automatic column inference
    /// from public properties and <see cref="TableColumnDefinitionAttribute"/> annotations.
    /// </summary>
    public class TableRowBasicTestViewModel
    {
        /// <summary>
        /// The submission's unique identifier.
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
        /// The date and time the submission was made. Sortable and formatted as <c>yyyy-MM-dd HH:mm</c>.
        /// </summary>
        [DataType(DataType.Date)]
        [DateFormat("yyyy-MM-dd HH:mm")]
        [TableColumnDefinition(Name = "Table_Date_Submitted_Header", ResourceType = typeof(Resources.Components), Sort = true)]
        public DateTime DateSubmitted { get; set; }

        /// <summary>
        /// The name of the reviewer assigned to the submission.
        /// </summary>
        [DataType(DataType.Text)]
        [TableColumnDefinition(Name = "Table_Assigned_Reviewer_Header", ResourceType = typeof(Resources.Components))]
        public string AssignedReviewer { get; set; } = string.Empty;
    }
}
