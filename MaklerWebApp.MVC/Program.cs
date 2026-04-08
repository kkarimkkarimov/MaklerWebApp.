using MaklerWebApp.MVC.Localization;
using MaklerWebApp.MVC.Options;
using MaklerWebApp.MVC.Services.Api;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.Extensions.Options;

namespace MaklerWebApp.MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            builder.Services.Configure<ApiClientOptions>(builder.Configuration.GetSection(ApiClientOptions.SectionName));
            builder.Services.AddHttpClient<IMaklerApiClient, MaklerApiClient>((serviceProvider, httpClient) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<ApiClientOptions>>().Value;
                if (string.IsNullOrWhiteSpace(options.BaseUrl))
                {
                    throw new InvalidOperationException("API base URL is missing. Configure Api:BaseUrl in appsettings or environment variables.");
                }

                httpClient.BaseAddress = new Uri(options.BaseUrl, UriKind.Absolute);
            });

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = AppLanguageOptions.SupportedCultures;
                options.DefaultRequestCulture = new RequestCulture(AppLanguageOptions.DefaultLanguage);
                options.SupportedCultures = supportedCultures.ToList();
                options.SupportedUICultures = supportedCultures.ToList();
                options.ApplyCurrentCultureToResponseHeaders = true;

                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new RouteDataRequestCultureProvider { RouteDataStringKey = "culture", UIRouteDataStringKey = "culture" },
                    new QueryStringRequestCultureProvider { QueryStringKey = "lang", UIQueryStringKey = "lang" },
                    new CookieRequestCultureProvider(),
                    new AcceptLanguageHeaderRequestCultureProvider()
                };
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            var requestLocalizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
            app.UseRequestLocalization(requestLocalizationOptions);

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "localized-default",
                pattern: "{culture=az}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
