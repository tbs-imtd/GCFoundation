using GCFoundation.Components.Controllers;
using GCFoundation.Components.Models;
using GCFoundation.Web.Models.Components;
using GCFoundation.Web.Models.Template;
using GCFoundation.Web.Models.Template.Crud;
using GCFoundation.Web.Resources;
using Microsoft.AspNetCore.Mvc;

namespace GCFoundation.Web.Controllers
{
    /// <summary>
    /// Controller responsible for serving the template demonstration or sample view.
    /// </summary>
    /// <param name="logger">The logger used to record controller activity.</param>
    [Route("template")]
    public class TemplateController(ILogger<TemplateController> logger) : GCFoundationBaseController(logger)
    {
        /// <summary>
        /// Displays the default template view.
        /// </summary>
        /// <returns>
        /// The template view result.
        /// </returns>
        [HttpGet("")]
        public IActionResult Index()
        {
            SetPageTitle(Menu.Menu_Template);

            return View();
        }


        /// <summary>
        /// Displays the old template view (deprecated).
        /// </summary>
        /// <returns>
        /// The old template view result.
        /// </returns>
        [HttpGet("old-index")]
        public IActionResult OldIndex()
        {
            SetPageTitle($"{Menu.Menu_Template}");

            return View("_OldIndex");
        }


        /// <summary>
        /// Displays the Basic page template demo page.
        /// </summary>
        /// <returns>
        /// The view for the Basic page template.
        /// </returns>
        [HttpGet("basic")]
        public IActionResult Basic()
        {
            SetPageTitle($"{Menu.Menu_Template} : {Resources.Template.Index_Basic_Title}");

            return View();
        }

        /// <summary>
        /// Displays the CRUD page template demo page.
        /// </summary>
        /// <returns>
        /// The view for the CRUD page template.
        /// </returns>
        [HttpGet("crud")]
        public IActionResult Crud()
        {
            SetPageTitle($"{Menu.Menu_Template} : {Resources.Template.Index_Crud_Title}");

            return View();
        }

        /// <summary>
        /// Displays the Dashboard page template demo page.
        /// </summary>
        /// <returns>
        /// The view for the Dashboard page template.
        /// </returns>
        [HttpGet("dashboard")]
        public IActionResult Dashboard()
        {
            SetPageTitle($"{Menu.Menu_Template} : {Resources.Template.Index_Dashboard_Title}");

            return View();
        }

        /// <summary>
        /// Displays the Error page template demo page.
        /// </summary>
        /// <returns>
        /// The view for the Error page template.
        /// </returns>
        [HttpGet("error")]
        public IActionResult Error()
        {
            SetPageTitle($"{Menu.Menu_Template} : {Resources.Template.Index_Error_Title}");

            return View();
        }

        /// <summary>
        /// Displays the Language Chooser page template demo page.
        /// </summary>
        /// <returns>
        /// The view for the Language Chooser page template.
        /// </returns>
        [HttpGet("language-chooser")]
        public IActionResult LanguageChooser()
        {
            SetPageTitle($"{Menu.Menu_Template} : {Resources.Template.Index_LanguageChooser_Title}");

            return View();
        }

        /// <summary>
        /// Displays the Stepper page template documentation page.
        /// </summary>
        /// <returns>
        /// The documentation view for the Stepper page template.
        /// </returns>
        [HttpGet("stepper")]
        public IActionResult Stepper()
        {
            SetPageTitle($"{Menu.Menu_Template} : {Resources.Template.Index_Stepper_Title}");

            return View();
        }

        #region Basic Page Template (Code, Demo) Controller Actions
        /// <summary>
        /// Displays a page containing sample code for the use of a Basic page template.
        /// </summary>
        /// <returns>
        /// The view for the sample code for a Basic page template.
        /// </returns>
        [HttpGet("basic/code")]
        public IActionResult BasicCode()
        {
            SetPageTitle($"{Menu.Menu_Template} : {Resources.Navigation.Nav_Template_Basic_Code}");

            return View("basic/code");
        }

        /// <summary>
        /// Displays a page containing a demo of a Basic page template.
        /// </summary>
        /// <returns>
        /// The view for the demo of a Basic page template.
        /// </returns>
        [HttpGet("basic/demo")]
        public IActionResult BasicDemo()
        {
            SetPageTitle($"{Menu.Menu_Template} : {Resources.Navigation.Nav_Template_Basic_Demo}");

            return View("basic/demo");
        }

        /// <summary>
        /// Displays a page containing sample code for the use of a Basic page template with side navigation.
        /// </summary>
        /// <returns>
        /// The view for the sample code for a Basic page template with side navigation.
        /// </returns>
        [HttpGet("basic/side-navigation/code")]
        public IActionResult BasicSideNavCode()
        {
            SetPageTitle($"{Menu.Menu_Template} : {Resources.Navigation.Nav_Template_Basic_Code}");

            return View("basic/sidenavigation-code");
        }

        /// <summary>
        /// Displays a page containing a demo of a Basic page template with side navigation.
        /// </summary>
        /// <returns>
        /// The view for the demo of a Basic page template with side navigation.
        /// </returns>
        [HttpGet("basic/side-navigation/demo")]
        public IActionResult BasicSideNavDemo()
        {
            SetPageTitle($"{Menu.Menu_Template} : {Resources.Navigation.Nav_Template_Basic_Demo}");

            return View("basic/sidenavigation-demo");
        }
        #endregion Basic Page Template Controller Actions

        #region Dashboard Page Template (Code, Demo) Controller Actions
        /// <summary>
        /// Displays a page containing sample code for the use of a generic Dashboard page template.
        /// </summary>
        /// <returns>
        /// The view for the sample code for a generic Dashboard page template.
        /// </returns>
        [HttpGet("dashboard/code")]
        public IActionResult DashboardCode()
        {
            SetPageTitle($"{Menu.Menu_Template} : {Resources.Navigation.Nav_Template_Dashboard_Code}");

            return View("dashboard/code");
        }

        /// <summary>
        /// Displays a page containing a demo of a generic Dashboard page template.
        /// </summary>
        /// <returns>
        /// The view for the demo of a generic Dashboard page template.
        /// </returns>
        [HttpGet("dashboard/demo")]
        public IActionResult DashboardDemo()
        {
            SetPageTitle($"{Menu.Menu_Template} : {Resources.Navigation.Nav_Template_Dashboard_Demo}");

            return View("dashboard/demo");
        }
        #endregion Dashboard Page Template (Code, Demo) Controller Actions

        #region Error Page Template (Code, Demo) Controller Actions
        /// <summary>
        /// Displays a page containing sample code for the use of a generic Error page template.
        /// </summary>
        /// <returns>
        /// The view for the sample code for a generic Error page template.
        /// </returns>
        [HttpGet("error/code")]
        public IActionResult ErrorCode()
        {
            SetPageTitle($"{Menu.Menu_Template} : {Resources.Navigation.Nav_Template_Error_Code}");

            return View("error/code");
        }

        /// <summary>
        /// Displays a page containing a demo of a generic Error page template.
        /// </summary>
        /// <returns>
        /// The view for the demo of a generic Error page template.
        /// </returns>
        [HttpGet("error/demo")]
        public IActionResult ErrorDemo()
        {
            SetPageTitle($"{Menu.Menu_Template} : {Resources.Navigation.Nav_Template_Error_Demo}");

            return View("error/demo");
        }
        #endregion Error Page Template (Code, Demo) Controller Actions

        #region Language Chooser Page Template (Code, Demo) Controller Actions
        /// <summary>
        /// Displays a page containing sample code for the use of a generic Language Chooser page template.
        /// </summary>
        /// <returns>
        /// The view for the sample code for a generic Language Chooser page template.
        /// </returns>
        [HttpGet("language-chooser/code")]
        public IActionResult LanguageChooserCode()
        {
            SetPageTitle($"{Menu.Menu_Template} : {Resources.Navigation.Nav_Template_LanguageChooser_Code}");

            return View("languagechooser/code");
        }

        /// <summary>
        /// Displays a page containing a demo of a generic Language Chooser page template.
        /// </summary>
        /// <returns>
        /// The view for the demo of a generic Language Chooser page template.
        /// </returns>
        [HttpGet("language-chooser/demo")]
        public IActionResult LanguageChooserDemo()
        {
            SetPageTitle($"{Menu.Menu_Template} : {Resources.Navigation.Nav_Template_LanguageChooser_Demo}");

            LanguageChooserModel model = new()
            {
                ApplicationTitleEn = "GCFoundation Demo",
                ApplicationTitleFr = "Démo GCFoundation",
                EnglishAction = Url.Action("Index", "Home", new { culture = "en" }) ?? "#",
                FrenchAction = Url.Action("Index", "Home", new { culture = "fr" }) ?? "#",
                TermLinkEn = "",
                TermLinkFr = ""
            };

            return View("~/Views/Language/Index.cshtml", model);
        }
        #endregion Language Chooser Page Template (Code, Demo) Controller Actions

        #region Stepper Page Template (Code, Demo) Controller Actions
        /// <summary>
        /// Displays a page containing sample code for the Stepper page template.
        /// </summary>
        /// <returns>
        /// The view for the sample code for a Stepper page template.
        /// </returns>
        [HttpGet("stepper/code")]
        public IActionResult StepperCode()
        {
            SetPageTitle($"{Menu.Menu_Template} : {Resources.Template.Index_Stepper_Title}");

            return View("stepper/code");
        }

        /// <summary>
        /// Displays a page containing a demo of a Stepper page template.
        /// </summary>
        /// <returns>
        /// The view for the demo of a Stepper page template.
        /// </returns>
        [HttpGet("stepper/demo")]
        public IActionResult StepperDemo()
        {
            SetPageTitle($"{Menu.Menu_Template} : {Resources.Template.Index_Stepper_Title}");

            return View("stepper/demo", new TemplateStepperFormViewModel());
        }

        /// <summary>
        /// Handles postbacks from the Stepper demo form.
        /// </summary>
        /// <param name="model">Bound form model.</param>
        /// <param name="nav">Navigation intent (prev/next).</param>
        /// <returns>The demo view.</returns>
        [HttpPost("stepper/demo")]
        [ValidateAntiForgeryToken]
        public IActionResult StepperDemo(TemplateStepperFormViewModel model, string? nav)
        {
            SetPageTitle($"{Menu.Menu_Template} : {Resources.Template.Index_Stepper_Title}");

            // This demo intentionally posts back to the same view regardless of navigation intent.
            // Validation errors will be surfaced via the error summary in the view.
            return View("stepper/demo", model);
        }
        #endregion Stepper Page Template (Code, Demo) Controller Actions

        #region Crud Page Template (Code, Demo) Controller Actions
        /// <summary>
        /// Displays the sample code for the CRUD page template.
        /// </summary>
        /// <returns>The CRUD code view.</returns>
        [HttpGet("crud/code")]
        public IActionResult CrudCode()
        {
            SetPageTitle($"{Menu.Menu_Template} : {Resources.Template.Index_Crud_Title}");

            return View("crud/code");
        }

        /// <summary>
        /// Displays the CRUD demonstration with the sample employee records.
        /// </summary>
        /// <returns>The CRUD demonstration view.</returns>
        [HttpGet("crud/demo")]
        public IActionResult CrudDemo()
        {
            SetPageTitle(Resources.Template.Crud_Demo_PageTitle);
            var vm = new DemoViewModel()
            {
                EmployeeModels = MockEmployeeList()
            };

            return View("crud/demo", vm);
        }

        /// <summary>
        /// Displays the CRUD demonstration after removing an employee from the sample records.
        /// </summary>
        /// <param name="id">The identifier of the employee to remove.</param>
        /// <returns>The updated CRUD demonstration view.</returns>
        public IActionResult CrudDemo(string id)
        {
            SetPageTitle(Resources.Template.Crud_Demo_PageTitle);
            var vm = new DemoViewModel()
            {
                EmployeeModels = MockEmployeeList()
            };
            vm.DeleteEmployee(id);
            return View("crud/demo", vm);
        }

        /// <summary>
        /// Displays the CRUD demonstration after adding or updating an employee.
        /// </summary>
        /// <param name="evm">The employee record to add or update.</param>
        /// <returns>The updated CRUD demonstration view.</returns>
        public IActionResult CrudDemo(EmployeeViewModel evm)
        {
            SetPageTitle(Resources.Template.Crud_Demo_PageTitle);
            var employees = MockEmployeeList();

            if (evm != null)
            {
                var index = employees.ToList().FindIndex(e => e.Id == evm.Id);
                if (index >= 0)
                    employees[index] = evm;  
                else
                    employees.Add(evm);       
            }

            var vm = new DemoViewModel
            {
                EmployeeModels = employees
            };

            return View("crud/demo", vm);
        }

        /// <summary>
        /// Removes an employee from the CRUD demonstration data.
        /// </summary>
        /// <param name="id">The identifier of the employee to remove.</param>
        /// <returns>The updated CRUD demonstration view.</returns>
        [HttpPost("crud/delete")]
        [ValidateAntiForgeryToken]
        public IActionResult CrudDelete(string id)
        {
            return CrudDemo(id);
        }

        /// <summary>
        /// Displays the form used to add or edit an employee.
        /// </summary>
        /// <param name="id">The optional identifier of the employee to edit.</param>
        /// <returns>The employee edit view.</returns>
        [HttpGet("crud/edit/{id?}")]
        public IActionResult CrudEdit(string? id)
        {
            SetPageTitle(Resources.Template.Crud_Edit_PageTitle);

            var employee = MockEmployeeList().FirstOrDefault(e => e.Id == id);
            if (employee == null)
                employee = new EmployeeViewModel();

            return View("crud/edit", employee);
        }

        /// <summary>
        /// Saves an employee submitted through the CRUD edit form.
        /// </summary>
        /// <param name="evm">The submitted employee record.</param>
        /// <returns>The updated CRUD demonstration view.</returns>
        [HttpPost("crud/edit")]
        [ValidateAntiForgeryToken]
        public IActionResult CrudSave(EmployeeViewModel evm)
        {
            return CrudDemo(evm);
        }

        /// <summary>
        /// Displays the profile for an employee in the demonstration data.
        /// </summary>
        /// <param name="id">The identifier of the employee to display.</param>
        /// <returns>The employee profile view, or the CRUD demonstration when no employee is found.</returns>
        [HttpGet("crud/view/{id}")]
        public IActionResult CrudView(string id)
        {
            SetPageTitle(Resources.Template.Crud_Profile_PageTitle);
            var employee = MockEmployeeList().FirstOrDefault(e => e.Id == id);
            if (employee == null)
                return CrudDemo();
            return View("crud/view", employee);

        }


        #region Helpers
        /// <summary>
        /// Creates the sample employee records used by the CRUD demonstration.
        /// </summary>
        /// <returns>The sample employee records.</returns>
        private static IList<EmployeeViewModel> MockEmployeeList()
        {
            return new List<EmployeeViewModel>
            {
                new EmployeeViewModel { Id = "10045821", Name = "Sarah Thompson", Level = "IT-01", DepartmentEn = "IT Operations", DepartmentFr = "Opérations TI", ManagerName = "David Chen", Salary = 68500, StartDate = new DateTime(2021, 3, 15), DateOfBirth = new DateTime(1994, 6, 22), Address = "123 Elgin St, Ottawa, ON" },
                new EmployeeViewModel { Id = "10045822", Name = "Michael Roy", Level = "IT-02", DepartmentEn = "Application Development", DepartmentFr = "Développement d'applications", ManagerName = "David Chen", Salary = 78200, StartDate = new DateTime(2020, 7, 6), DateOfBirth = new DateTime(1991, 2, 14), Address = "45 Bank St, Ottawa, ON" },
                new EmployeeViewModel { Id = "10045823", Name = "Émilie Gagnon", Level = "IT-03", DepartmentEn = "Cybersecurity", DepartmentFr = "Cybersécurité", ManagerName = "David Chen", Salary = 92300, StartDate = new DateTime(2019, 1, 20), DateOfBirth = new DateTime(1988, 11, 3), Address = "78 Rideau St, Ottawa, ON" },
                new EmployeeViewModel { Id = "10045824", Name = "James Wilson", Level = "IT-04", DepartmentEn = "Enterprise Architecture", DepartmentFr = "Architecture d'entreprise", ManagerName = "Karen Wu", Salary = 105600, StartDate = new DateTime(2017, 9, 11), DateOfBirth = new DateTime(1985, 4, 30), Address = "12 Sparks St, Ottawa, ON" },
                new EmployeeViewModel { Id = "10045825", Name = "Priya Sharma", Level = "IT-05", DepartmentEn = "IT Strategy", DepartmentFr = "Stratégie TI", ManagerName = "Karen Wu", Salary = 118400, StartDate = new DateTime(2015, 5, 4), DateOfBirth = new DateTime(1982, 8, 19), Address = "200 Wellington St, Ottawa, ON" },
                new EmployeeViewModel { Id = "10045826", Name = "Marc Tremblay", Level = "AS-01", DepartmentEn = "Administrative Services", DepartmentFr = "Services administratifs", ManagerName = "Linda Osei", Salary = 55300, StartDate = new DateTime(2022, 2, 8), DateOfBirth = new DateTime(1996, 12, 5), Address = "34 Somerset St, Ottawa, ON" },
                new EmployeeViewModel { Id = "10045827", Name = "Jennifer Lee", Level = "AS-02", DepartmentEn = "Administrative Services", DepartmentFr = "Services administratifs", ManagerName = "Linda Osei", Salary = 61800, StartDate = new DateTime(2021, 11, 1), DateOfBirth = new DateTime(1993, 3, 27), Address = "56 Preston St, Ottawa, ON" },
                new EmployeeViewModel { Id = "10045828", Name = "Ahmed Hassan", Level = "AS-03", DepartmentEn = "Program Support", DepartmentFr = "Soutien aux programmes", ManagerName = "Linda Osei", Salary = 67900, StartDate = new DateTime(2018, 6, 18), DateOfBirth = new DateTime(1990, 9, 9), Address = "89 Bronson Ave, Ottawa, ON" },
                new EmployeeViewModel { Id = "10045829", Name = "Chantal Bouchard", Level = "CS-01", DepartmentEn = "Application Development", DepartmentFr = "Développement d'applications", ManagerName = "David Chen", Salary = 74100, StartDate = new DateTime(2020, 10, 12), DateOfBirth = new DateTime(1992, 7, 16), Address = "23 Gladstone Ave, Ottawa, ON" },
                new EmployeeViewModel { Id = "10045830", Name = "Robert Kim", Level = "CS-02", DepartmentEn = "Application Development", DepartmentFr = "Développement d'applications", ManagerName = "David Chen", Salary = 83700, StartDate = new DateTime(2019, 4, 25), DateOfBirth = new DateTime(1989, 1, 11), Address = "67 Booth St, Ottawa, ON" }
            };
        }
        #endregion

        #endregion
    }
}