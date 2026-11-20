using GCFoundation.Components.Models;

namespace GCFoundation.Web.Models.Template.Crud
{
    /// <summary>
    /// Provides the employee records displayed by the CRUD demonstration page.
    /// </summary>
    public class DemoViewModel : BaseViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DemoViewModel"/> class.
        /// </summary>
        /// <param name="employeeModels">The employee records displayed in the demonstration table.</param>
        public DemoViewModel(IList<EmployeeViewModel> employeeModels)
        {
            EmployeeModels = employeeModels;
        }

        /// <summary>
        /// Gets the employee records displayed in the demonstration table.
        /// </summary>
        public IList<EmployeeViewModel> EmployeeModels { get; }

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
