using GCFoundation.Web.Controllers;
using RouteLocalization.AspNetCore;

namespace GCFoundation.Web.Infrastructure.Extensions
{
    /// <summary>
    /// Provides extension methods for setting up route localization in the application.
    /// </summary>
    public static class RouteLocalizationExtensions
    {
        /// <summary>
        /// Supported cultures for the application routing.
        /// </summary>
        private static readonly string[] SupportedCultures = ["en", "fr"];

        /// <summary>
        /// Adds custom route localization configuration to the application.
        /// </summary>
        /// <param name="services">The service collection to which the route localization is added.</param>
        /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddCustomRouteLocalization(this IServiceCollection services)
        {
            services.AddRouteLocalization(setup =>
            {
                // Home Controller
                setup.UseCulture("fr")
                .WhereController(nameof(HomeController))
                .TranslateController("accueil")
                .WhereAction(nameof(HomeController.Index))
                .TranslateAction("");

                setup.UseCulture("fr")
                .WhereController(nameof(HomeController))
                .WhereAction(nameof(HomeController.AccessibilityStatement))
                .TranslateAction("declaration-daccessibilite");

                // Components Controller
                setup.UseCulture("fr")
                .WhereController(nameof(ComponentsController))
                .TranslateController("composants")
                .WhereAction(nameof(ComponentsController.Index))
                .TranslateAction("");

                setup.UseCulture("fr")
                .WhereController(nameof(ComponentsController))
                .WhereAction(nameof(ComponentsController.Gcds))
                .TranslateAction("gcds");

                setup.UseCulture("fr")
                .WhereController(nameof(ComponentsController))
                .WhereAction(nameof(ComponentsController.Card))
                .TranslateAction("carte");

                setup.UseCulture("fr")
                .WhereController(nameof(ComponentsController))
                .WhereAction(nameof(ComponentsController.Form))
                .TranslateAction("formulaire");

                setup.UseCulture("fr")
                .WhereController(nameof(ComponentsController))
                .WhereAction(nameof(ComponentsController.FilteredSearch))
                .TranslateAction("recherche-filtree");

                setup.UseCulture("fr")
                .WhereController(nameof(ComponentsController))
                .WhereAction(nameof(ComponentsController.Table))
                .TranslateAction("tableau");

                setup.UseCulture("fr")
                .WhereController(nameof(ComponentsController))
                .WhereAction(nameof(ComponentsController.FormBuilder))
                .TranslateAction("constructeur-formulaires");

                setup.UseCulture("fr")
                .WhereController(nameof(ComponentsController))
                .WhereAction(nameof(ComponentsController.Badge))
                .TranslateAction("badge");

                setup.UseCulture("fr")
                .WhereController(nameof(ComponentsController))
                .WhereAction(nameof(ComponentsController.Modal))
                .TranslateAction("modal");

                setup.UseCulture("fr")
                .WhereController(nameof(ComponentsController))
                .WhereAction(nameof(ComponentsController.PageHeading))
                .TranslateAction("titre-page");

                setup.UseCulture("fr")
                .WhereController(nameof(ComponentsController))
                .WhereAction(nameof(ComponentsController.Stepper))
                .TranslateAction("etapes");

                setup.UseCulture("fr")
                .WhereController(nameof(ComponentsController))
                .WhereAction(nameof(ComponentsController.Tabs))
                .TranslateAction("onglets");

                // Template Controller
                setup.UseCulture("fr")
                .WhereController(nameof(TemplateController))
                .TranslateController("modele")
                .WhereAction(nameof(TemplateController.Index))
                .TranslateAction("");

                setup.UseCulture("fr")
                .WhereController(nameof(TemplateController))
                .WhereAction(nameof(TemplateController.Basic))
                .TranslateAction("base");

                setup.UseCulture("fr")
                .WhereController(nameof(TemplateController))
                .WhereAction(nameof(TemplateController.BasicCode))
                .TranslateAction("base/code");

                setup.UseCulture("fr")
                .WhereController(nameof(TemplateController))
                .WhereAction(nameof(TemplateController.BasicDemo))
                .TranslateAction("base/demo");

                setup.UseCulture("fr")
                .WhereController(nameof(TemplateController))
                .WhereAction(nameof(TemplateController.BasicSideNavCode))
                .TranslateAction("base/navigation-laterale/code");

                setup.UseCulture("fr")
                .WhereController(nameof(TemplateController))
                .WhereAction(nameof(TemplateController.BasicSideNavDemo))
                .TranslateAction("base/navigation-laterale/demo");

                setup.UseCulture("fr")
                .WhereController(nameof(TemplateController))
                .WhereAction(nameof(TemplateController.Dashboard))
                .TranslateAction("tableau-de-bord");

                setup.UseCulture("fr")
                .WhereController(nameof(TemplateController))
                .WhereAction(nameof(TemplateController.DashboardCode))
                .TranslateAction("tableau-de-bord/code");

                setup.UseCulture("fr")
                .WhereController(nameof(TemplateController))
                .WhereAction(nameof(TemplateController.DashboardDemo))
                .TranslateAction("tableau-de-bord/demo");

                setup.UseCulture("fr")
                .WhereController(nameof(TemplateController))
                .WhereAction(nameof(TemplateController.Error))
                .TranslateAction("erreur");

                setup.UseCulture("fr")
                .WhereController(nameof(TemplateController))
                .WhereAction(nameof(TemplateController.ErrorCode))
                .TranslateAction("erreur/code");

                setup.UseCulture("fr")
                .WhereController(nameof(TemplateController))
                .WhereAction(nameof(TemplateController.ErrorDemo))
                .TranslateAction("erreur/demo");

                setup.UseCulture("fr")
                .WhereController(nameof(TemplateController))
                .WhereAction(nameof(TemplateController.LanguageChooser))
                .TranslateAction("selection-de-langue");

                setup.UseCulture("fr")
                .WhereController(nameof(TemplateController))
                .WhereAction(nameof(TemplateController.LanguageChooserCode))
                .TranslateAction("selection-de-langue/code");

                setup.UseCulture("fr")
                .WhereController(nameof(TemplateController))
                .WhereAction(nameof(TemplateController.LanguageChooserDemo))
                .TranslateAction("selection-de-langue/demo");

                setup.UseCulture("fr")
                .WhereController(nameof(TemplateController))
                .WhereAction(nameof(TemplateController.Stepper))
                .TranslateAction("indicateur-detapes");

                setup.UseCulture("fr")
                .WhereController(nameof(TemplateController))
                .WhereAction(nameof(TemplateController.StepperCode))
                .TranslateAction("indicateur-detapes/code");

                setup.UseCulture("fr")
                .WhereController(nameof(TemplateController))
                .WhereAction(nameof(TemplateController.StepperDemo))
                .TranslateAction("indicateur-detapes/demo");

                setup.UseCulture("fr")
                .WhereController(nameof(TemplateController))
                .WhereAction(nameof(TemplateController.StepperDemoSuccess))
                .TranslateAction("indicateur-detapes/demo/succes");

                // - Translate Template Actions.

                // Authentication Controller
                setup.UseCulture("fr")
                .WhereController(nameof(AuthenticationController))
                .TranslateController("authentification")
                .WhereAction(nameof(AuthenticationController.Login))
                .TranslateAction("connexion");

                setup.UseCulture("fr")
                .WhereController(nameof(AuthenticationController))
                .WhereAction(nameof(AuthenticationController.Logout))
                .TranslateAction("deconnexion");

                setup.UseCulture("fr")
                .WhereController(nameof(AuthenticationController))
                .WhereAction(nameof(AuthenticationController.RefreshSession))
                .TranslateAction("rafraichir-session");

                // Installation Controller
                setup.UseCulture("fr")
                .WhereController(nameof(InstallationController))
                .TranslateController("installation")
                .WhereAction(nameof(InstallationController.Index))
                .TranslateAction("");

                setup.UseCulture("fr")
                .WhereController(nameof(InstallationController))
                .WhereAction(nameof(InstallationController.GlobalResources))
                .TranslateAction("ressources-globales");

                setup.UseCulture("fr")
                .WhereController(nameof(InstallationController))
                .WhereAction(nameof(InstallationController.Metadata))
                .TranslateAction("metadonnees");

                setup.UseCulture("fr")
                .WhereController(nameof(InstallationController))
                .WhereAction(nameof(InstallationController.Translations))
                .TranslateAction("traductions");

                // Language Controller
                setup.UseCulture("fr")
                .WhereController(nameof(LanguageController))
                .TranslateController("langue")
                .WhereAction(nameof(LanguageController.Index))
                .TranslateAction("");

                // Styles Controller
                setup.UseCulture("fr")
                .WhereController(nameof(StylesController))
                .TranslateController("styles")
                .WhereAction(nameof(StylesController.Index))
                .TranslateAction("");

                // Ensure untranslated routes exist for controllers with attribute routes only
                setup.UseCultures(SupportedCultures)
                    .WhereUntranslated()
                    .AddDefaultTranslation();
            });

            return services;
        }
    }
}
