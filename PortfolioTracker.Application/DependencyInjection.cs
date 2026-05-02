using Microsoft.Extensions.DependencyInjection;
using PortfolioTracker.Application.Interfaces;
using PortfolioTracker.Application.Profiles;
using PortfolioTracker.Application.Services;

namespace PortfolioTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPortfolioService, PortfolioService>();
        services.AddScoped<IPortfolioItemService, PortfolioItemService>();
        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(typeof(DependencyInjection).Assembly);
        });
        return services;
    }
}
