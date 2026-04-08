using MaklerWebApp.BLL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MaklerWebApp.BLL.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessLayer(this IServiceCollection services)
    {
        services.AddScoped<IListingService, ListingService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IFavoriteService, FavoriteService>();
        services.AddScoped<IPaymentService, PaymentService>();
        return services;
    }
}
