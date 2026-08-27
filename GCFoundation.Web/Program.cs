using cloudscribe.Web.Localization;
using cloudscribe.Web.SiteMap;
using GCFoundation.Common.Utilities;
using GCFoundation.Components.Middleware;
using GCFoundation.Components.Services;
using GCFoundation.Components.Services.Interfaces;
using GCFoundation.Security.Middlewares;
using GCFoundation.Web.Infrastructure.Extensions;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// Add authentication for demo purposes
builder.Services.AddAuthentication("DemoAuth")
    .AddCookie("DemoAuth", options =>
    {
        options.LoginPath = "/examples/user-login";
        options.LogoutPath = "/examples/user-login";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = false;
        options.Cookie.Name = "GCFoundationDemo";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddScoped<ISiteMapNodeService, NavigationTreeSiteMapNodeService>();
builder.Services.AddCloudscribeNavigation(builder.Configuration.GetSection("NavigationOptions"));

// Localization configuration
builder.Services.Configure<GlobalResourceOptions>(builder.Configuration.GetSection("GlobalResourceOptions"));
builder.Services.AddSingleton<IStringLocalizerFactory, GlobalResourceManagerStringLocalizerFactory>();
builder.Services.AddLocalization();

// Configure breadcrumbs localization service
builder.Services.AddSingleton<IBreadcrumbsLocalizationService, BreadcrumbsLocalizationService<GCFoundation.Web.Resources.Navigation>>();

//Configure top nav localization service
builder.Services.AddSingleton<ITopNavigationLocalizationService, TopNavigationLocalizationService<GCFoundation.Web.Resources.Navigation>>();
builder.Services.AddSingleton<ITagHelperCatalogService, TagHelperCatalogService<GCFoundation.Web.Resources.Components>>();

// Configure GCFoundation
builder.Services.AddGCFoundationComponents(builder.Configuration);
builder.Services.AddGCFoundationContentPolicies(builder.Configuration);
builder.Services.AddGCFoundationSession(builder.Configuration);

// Language configuration
var supportedCultures = LanguageUtility.GetSupportedCulture();
var routeSegmentLocalizationProvider = new FirstUrlSegmentRequestCultureProvider(supportedCultures.ToList());

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(culture: "en-CA", uiCulture: "en-CA");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    options.RequestCultureProviders.Insert(0, routeSegmentLocalizationProvider);
});

// Add route localization using the custom extension method
builder.Services.AddCustomRouteLocalization();

builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Add("/Views/Shared/Components/Navigation/{0}.cshtml");
    options.ViewLocationFormats.Add("/contentFiles/any/net10.0/Views/Shared/Components/Navigation/{0}.cshtml");
    options.ViewLocationFormats.Add("/contentFiles/any/net8.0/Views/Shared/Components/Navigation/{0}.cshtml");
});

var app = builder.Build();

// Support running under a virtual directory (PathBase), e.g. "/gcfoundation"
var pathBase = builder.Configuration["ASPNETCORE_PATHBASE"];
if (!string.IsNullOrEmpty(pathBase))
{
    app.UsePathBase(pathBase);
}

// Add GCFoundation language middleware
app.UseMiddleware<GCFoundationLanguageMiddleware>();

// Secure Cookies
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,  // Prevent cross-site requests
    Secure = CookieSecurePolicy.Always,  // Only send cookies over HTTPS
    HttpOnly = HttpOnlyPolicy.Always  // Prevent JavaScript access to cookies
});

// Use GCFoundation
app.UseGCFoundationComponents();
app.UseGCFoundationContentPolicies();
app.UseGCFoundationSession();

// Configure exception handlers
if (!app.Environment.IsDevelopment())
{
    app.UseStatusCodePagesWithReExecute("/en/error/not-found");
    app.UseExceptionHandler("/en/error/global");
}

var disableHttpsRedirect = string.Equals(
    System.Environment.GetEnvironmentVariable("DISABLE_HTTPS_REDIRECT"),
    "true",
    System.StringComparison.OrdinalIgnoreCase);

if (!disableHttpsRedirect)
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();

// Use localization middleware
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

// Add authentication and authorization for demo
app.UseAuthentication();
app.UseAuthorization();

// Default route
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{culture=en}/{controller=Home}/{action=Index}/{id?}"
);

app.Run();