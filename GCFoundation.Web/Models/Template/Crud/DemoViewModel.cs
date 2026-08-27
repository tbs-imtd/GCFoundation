using GCFoundation.Components.Models;

namespace GCFoundation.Web.Models.Template.Crud
{
    /// <summary>
    /// Provides the employee records displayed by the CRUD demonstration page.
    /// </summary>
    public class DemoViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets or sets the employee records displayed in the demonstration table.
        /// </summary>
        public required IList<EmployeeViewModel> EmployeeModels { get; set; }

        /// <summary>
        /// Removes the employee with the specified identifier from the demonstration data.
        /// </summary>
        /// <param name="id">The identifier of the employee to remove.</param>
        public void DeleteEmployee(string id)
        {
            if (EmployeeModels == null)
                return;

            var employee = EmployeeModels.FirstOrDefault(e => e.Id == id);
            if (employee != null)
                EmployeeModels.Remove(employee);
        }
    }
}
