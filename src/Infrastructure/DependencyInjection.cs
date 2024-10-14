using Amazon.SQS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeatherLens.Application;
using WeatherLens.Application.Common.Interfaces;
using WeatherLens.Infrastructure.Data;
using WeatherLens.Infrastructure.Data.Interceptors;

namespace WeatherLens.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            options.UseSqlite(connectionString);
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        //services.AddScoped<ApplicationDbContextInitialiser>();

        //services.AddAuthentication();
        //  .AddBearerToken(IdentityConstants.BearerScheme);

        //services.AddAuthorizationBuilder();

        services.AddSingleton(TimeProvider.System);
        //services.AddTransient<IIdentityService, IdentityService>();

        //services.AddAuthorization(options =>
        //    options.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));

        // Load configuration from appsettings.json
        services.Configure<WeatherApiOptions>(configuration.GetSection(WeatherApiOptions.Key));

        services.AddHttpClient<IWeatherService, WeatherLensService>();
        services.AddSignalR();
        services.Configure<ApiOptions>(configuration.GetSection("ApiSettings"));

        services.AddSingleton<IAmazonSQS, AmazonSQSClient>();
        return services;
    }
}
