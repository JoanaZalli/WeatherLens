using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WeatherLens.Application.Messaging;
using WeatherLens.Application.Messaging.SignalR;
using WeatherLens.Application.WeatherForecasts.Queries;

public class Scheduler
{
    private readonly ISender _sender;
    private readonly IHubContext<WeatherHub> _hubContext;
    private readonly IWeatherSubscriptionService _weatherSubscriptionService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Scheduler> _logger;

    public Scheduler(ISender sender, IHubContext<WeatherHub> hubContext,
        IWeatherSubscriptionService weatherSubscriptionService,
        IServiceProvider serviceProvider,
        ILogger<Scheduler> logger)
    {
        _sender = sender;
        _hubContext = hubContext;
        _weatherSubscriptionService = weatherSubscriptionService;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task FetchWeatherData(GetWeatherForecastQuery query)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var weatherSubscriptionService = scope.ServiceProvider.GetRequiredService<IWeatherSubscriptionService>();

            var data = await sender.Send(query);
            await _weatherSubscriptionService.CheckForUpdatesAsync();

            if (data?.IsSuccess == true)
            {
                // Create a weather notification message
                var notification = new WeatherNotification
                {
                    City = query.City,
                    Date = DateTime.UtcNow,
                    Description = data?.Value?.Weather?.FirstOrDefault()?.Description,
                    Temperature = data?.Value?.Main?.Temp,
                    TempMax = data?.Value?.Main?.TempMax,
                    TempMin = data?.Value?.Main?.TempMin,
                    Humidity = data?.Value?.Main?.Humidity,
                    WindSpeed = data?.Value?.Wind?.Speed,
                };

                try
                {
                    // Send the notification to all connected clients
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, "Failed to send weather notification {WeatherNotification} to connected clients", notification);
                }
            }
            else
            {
                _logger.LogError(data?.Error, "Failed to retrieve weather data for city {City} on {Date}.", query.City, query.Date);
            }
        }
    }
}
