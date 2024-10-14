using MediatR;
using Microsoft.AspNetCore.SignalR;
using WeatherLens.Application.Messaging;
using WeatherLens.Application.Messaging.SignalR;
using WeatherLens.Application.WeatherForecasts.Queries;

public class Scheduler
{
    private readonly ISender _sender;
    private readonly IHubContext<WeatherHub> _hubContext;
    private readonly IWeatherSubscriptionService _weatherSubscriptionService;

    public Scheduler(ISender sender, IHubContext<WeatherHub> hubContext, IWeatherSubscriptionService weatherSubscriptionService)
    {
        _sender = sender;
        _hubContext = hubContext;
        _weatherSubscriptionService = weatherSubscriptionService;
    }

    public async Task FetchWeatherData(GetWeatherForecastQuery query)
    {
        var data = await _sender.Send(query);
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

            // Send the notification to all connected clients
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
        }
        else
        {
            Console.WriteLine("Failed to fetch weather data.");
        }
    }
}
