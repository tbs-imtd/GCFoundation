using GCFoundation.Components.Enums;
using System.ComponentModel.DataAnnotations;

namespace GCFoundation.Components.Attributes.Table
{
    /// <summary>
    /// Marks a property as a column definition for a table component. When applied to a property in a model class, it indicates that the property should be treated as a column in the table, and its values will be used to define the table column.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class TableColumnDefinitionAttribute : Attribute
    {
        /// <summary>
        /// The resource key used to resolve this column's header text, or the literal header
        /// text itself if <see cref="ResourceType"/> is not set. Behaves the same way as
        /// <see cref="DisplayAttribute.Name"/> — when paired with <see cref="ResourceType"/>,
        /// the value is looked up as a resource key; otherwise it is used as-is.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The type whose associated resource file contains the key specified by <see cref="Name"/>.
        /// If <c>null</c>, <see cref="Name"/> is treated as a literal string instead of a resource key.
        /// Behaves the same way as <see cref="DisplayAttribute.ResourceType"/>.
        /// </summary>
        public Type? ResourceType { get; set; }

        /// <summary>
        /// Use <see cref="Alignment"/> to control how the content inside the column cells is positioned horizontally.
        /// </summary>
        public CellAlignment Alignment { get; set; } = CellAlignment.start;

        /// <summary>
        /// Set <see cref="IsHidden"/> to <c>true</c> if you want to hide the column.
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Set <see cref="RowHeader"/> to <c>true</c> if you want to mark each cell in the column as a row header. Row headers label what each row is about.
        /// </summary>
        public bool RowHeader { get; set; }

        /// <summary>
        /// Set <see cref="Slotted"/> to <c>true</c> to flag that the cell will render custom content. To see how each framework handles this, go to <see href="https://design-system.canada.ca/en/components/table/code/#framework-specific-slots-for-custom-content">Framework-specific slots for custom content</see>.
        /// </summary>
        public bool Slotted { get; set; }

        /// <summary>
        /// Set <see cref="Sort"/> to <c>true</c> to allow people to sort the table by that column.
        /// </summary>
        public bool Sort { get; set; }

        /// <summary>
        /// Use <see cref="SortDirection"/> to set a default sort order for the column when the page loads.
        /// </summary>
        public SortDirection SortDirection { get; set; } = SortDirection.none;
    }
}