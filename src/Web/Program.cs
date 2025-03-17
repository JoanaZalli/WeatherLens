using Hangfire;
using Hangfire.MemoryStorage;
using Serilog;
using WeatherLens.Application;
using WeatherLens.Application.Messaging.SignalR;
using WeatherLens.Application.WeatherForecasts.Queries;
using WeatherLens.Infrastructure;
using WeatherLens.Web;

var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Replace the default .NET logger with Serilog
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddKeyVaultIfConfigured(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices();

builder.Services.AddHangfire(config => config.UseMemoryStorage());
builder.Services.AddHangfireServer();
builder.Services.AddSingleton<Scheduler>();
var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "default.smtp.host";
var smtpPort = Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587"; // Default SMTP port
var smtpUser = Environment.GetEnvironmentVariable("SMTP_USER") ?? throw new ArgumentNullException("SMTP_USER", "SMTP_USER environment variable is required.");
var smtpPass = Environment.GetEnvironmentVariable("SMTP_PASS") ?? throw new ArgumentNullException("SMTP_PASS", "SMTP_PASS environment variable is required.");
var appPass = Environment.GetEnvironmentVariable("APP_PASS") ?? throw new ArgumentNullException("APP_PASS", "APP_PASS environment variable is required.");

if (!int.TryParse(smtpPort, out var port))
{
    throw new ArgumentException("Invalid SMTP_PORT value. It must be a valid integer.");
}

builder.Services.AddSingleton<EmailService>(sp =>
    new EmailService(smtpHost, port, smtpUser, appPass));
builder.Services.AddSingleton<IWeatherSubscriptionService, WeatherSubscriptionService>();
builder.Services.AddSingleton<List<UserSubscription>>(); // In-memory storage for subscriptions
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    //await app.InitialiseDatabaseAsync();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapHub<WeatherHub>("/weatherHub");

app.MapEndpoints();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherLens V1");
    c.RoutePrefix = "";
});

app.UseExceptionHandler(options => { });

app.MapGet("", () => Results.Redirect("/swagger"));

app.UseMiddleware<ApiKeyMiddleware>();

// Fire-and-forget background job
var backgroundJobs = app.Services.GetRequiredService<IBackgroundJobClient>();

// Schedule the recurring job
RecurringJob.AddOrUpdate<IWeatherSubscriptionService>(
    "check-weather-updates",
    service => service.CheckForUpdatesAsync(),
    Cron.Hourly); // Run every hour

//Create a recurring job to fetch weather data every 2 hours
RecurringJob.AddOrUpdate<Scheduler>(
    "weather-fetch-job",
   job => job.FetchWeatherData(new GetWeatherForecastQuery { City = "Tirana", Date = DateTime.Now }),
    Cron.Minutely());

app.Run();

public partial class Program { } 
