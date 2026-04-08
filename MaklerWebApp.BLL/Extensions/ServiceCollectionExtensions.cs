using MaklerWebApp.BLL.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MaklerWebApp.BLL.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OtpEmailOptions>(configuration.GetSection(OtpEmailOptions.SectionName));
        services.AddScoped<IListingService, ListingService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOtpDeliveryService, OtpDeliveryService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<IPaymentService, PaymentService>();
        return services;
    }
}
