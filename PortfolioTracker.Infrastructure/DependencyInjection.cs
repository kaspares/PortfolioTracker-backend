using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PortfolioTracker.Domain.Interfaces;
using PortfolioTracker.Domain.Repositories;
using PortfolioTracker.Infrastructure.Identity;
using PortfolioTracker.Infrastructure.MarketData;
using PortfolioTracker.Infrastructure.Persistence;
using PortfolioTracker.Infrastructure.Repositories;

namespace PortfolioTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PortfolioTrackerDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));

        services.AddIdentityApiEndpoints<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
        })
         .AddEntityFrameworkStores<PortfolioTrackerDbContext>();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = ".PortfolioTracker.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = 403;
                return Task.CompletedTask;
            };
        });

        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<IPortfolioItemRepository, PortfolioItemRepository>();
        services.AddHttpClient<IMarketDataProvider, YahooFinanceProvider>(client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
        });


        return services;
    }

    public static WebApplication MapIdentityEndpoints(this WebApplication app)
    {
        app.MapIdentityApi<ApplicationUser>();
        return app;
    }
}
