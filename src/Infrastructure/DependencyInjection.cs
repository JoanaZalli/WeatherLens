using Amazon.SQS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeatherLens.Application;
using WeatherLens.Application.Common.Interfaces;
using WeatherLens.Infrastructure.Data;

namespace WeatherLens.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            options.UseSqlite(connectionString);
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddSingleton(TimeProvider.System);
        services.Configure<ApiOptions>(configuration.GetSection(ApiOptions.Key));
        services.Configure<WeatherApiOptions>(configuration.GetSection(WeatherApiOptions.Key));

        services.AddHttpClient<IWeatherService, WeatherLensService>();
        services.AddSignalR();
        return services;
    }
}
