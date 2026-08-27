using GCFoundation.Common.Utilities;
using GCFoundation.Components.Attributes;
using GCFoundation.Components.Attributes.Table;
using GCFoundation.Components.Models;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security;

namespace GCFoundation.Web.Models.Template.Crud
{
    /// <summary>
    /// Represents an employee record used by the CRUD forms, table, and profile page.
    /// </summary>
    public class EmployeeViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the value used by the table's custom actions column.
        /// </summary>
        [TableColumnDefinition(Slotted = true, Order = 4)]
        public string Actions { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the employee's mailing address.
        /// </summary>
        [Display(Name = "Crud_Demo_Address_Label", ResourceType = typeof(Resources.Template), Order = 9)]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the employee's date of birth.
        /// </summary>
        [DataType(DataType.Date)]
        [DateFormat("yyyy-MM-dd")]
        [Display(Name = "Crud_Demo_DateOfBirth_Label", ResourceType = typeof(Resources.Template), Order = 8)]
        public DateTime DateOfBirth { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets the employee's department name in the current language.
        /// </summary>
        [Display(Name = "Crud_Demo_Department_Label", ResourceType = typeof(Resources.Template), Order = 5)]
        public string Department => LanguageUtility.IsFrench() ? DepartmentFr : DepartmentEn;

        /// <summary>
        /// Gets or sets the employee's department name in English.
        /// </summary>
        public string DepartmentEn { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the employee's department name in French.
        /// </summary>
        public string DepartmentFr { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the employee's unique identifier.
        /// </summary>
        [Display(Name = "Crud_Demo_EmployeeId_Label", ResourceType = typeof(Resources.Template), Order = 2)]
        [TableColumnDefinition(RowHeader = true, Sort = true, Order = 1)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the employee's classification level.
        /// </summary>
        [Display(Name = "Crud_Demo_EmployeeLevel_Label", ResourceType = typeof(Resources.Template), Order = 3)]
        [TableColumnDefinition(Sort = true, Order = 3)]
        public string Level { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the employee's manager name.
        /// </summary>
        [Display(Name = "Crud_Demo_ManagerName_Label", ResourceType = typeof(Resources.Template), Order = 6)]
        public string ManagerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the employee's full name.
        /// </summary>
        [Display(Name = "Crud_Demo_EmployeeName_Label", ResourceType = typeof(Resources.Template), Order = 2)]
        [TableColumnDefinition(Sort = true, Order = 2)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the employee's annual salary.
        /// </summary>
        [Display(Name = "Crud_Demo_Salary_Label", ResourceType = typeof(Resources.Template), Order = 4)]
        public double Salary { get; set; }

        /// <summary>
        /// Gets or sets the employee's employment start date.
        /// </summary>
        [DataType(DataType.Date)]
        [DateFormat("yyyy-MM-dd")]
        [Display(Name = "Crud_Demo_StartDate_Label", ResourceType = typeof(Resources.Template), Order = 7)]
        public DateTime StartDate { get; set; } = DateTime.Now;
    }
}
