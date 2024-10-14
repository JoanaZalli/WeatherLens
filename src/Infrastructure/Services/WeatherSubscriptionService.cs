using System.Collections.Concurrent;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using WeatherLens.Application.Messaging.SignalR;

public class WeatherSubscriptionService : IWeatherSubscriptionService
{
    private readonly ISender _sender;
    private readonly IHubContext<WeatherHub> _hubContext;
    private readonly EmailService _emailService;
    private readonly IWeatherService _weatherService;
    private readonly ILogger<WeatherSubscriptionService> _logger;

    // In-memory storage for subscriptions and weather data
    private readonly ConcurrentDictionary<string, UserSubscription> _subscriptions = new();
    private readonly ConcurrentDictionary<string, WeatherData> _weatherDataCache = new();

    public WeatherSubscriptionService(ISender sender, IHubContext<WeatherHub> hubContext,
        EmailService emailService, IWeatherService weatherService,
        ILogger<WeatherSubscriptionService> logger)
    {
        _sender = sender;
        _hubContext = hubContext;
        _emailService = emailService;
        _weatherService = weatherService;
        _logger = logger;
    }

    public void Subscribe(UserSubscription userSubscription)
    {
        _subscriptions[userSubscription.City + userSubscription.Email] = userSubscription;
    }

    public void Unsubscribe(UserSubscription userSubscription)
    {
        _subscriptions.TryRemove(userSubscription.City + userSubscription.Email, out _);
    }

    public async Task CheckForUpdatesAsync()
    {
        foreach (var subscription in _subscriptions.Values)
        {
            var currentWeather = await _weatherService.GetWeatherDataAsync(subscription.City, DateTime.Now);
            var newWeatherData = new WeatherData
            {
                City = subscription.City,
                Description = currentWeather?.Value?.Weather?.FirstOrDefault()?.Description,
                Temperature = currentWeather?.Value?.Main?.Temp,
                TempMax = currentWeather?.Value?.Main?.TempMax,
                TempMin = currentWeather?.Value?.Main?.TempMin,
                Humidity = currentWeather?.Value?.Main?.Humidity,
                WindSpeed = currentWeather?.Value?.Wind?.Speed,
            };

            if (_weatherDataCache.TryGetValue(subscription.City + DateTime.UtcNow.Date, out var oldWeatherData))
            {
                // Check if the weather data has changed
                if (HasWeatherChanged(oldWeatherData, newWeatherData))
                {
                    // Update the cache
                    _weatherDataCache[subscription.City + DateTime.UtcNow.Date] = newWeatherData;

                    // Send email notification
                    try
                    {
                        await SendEmailNotification(subscription.Email, newWeatherData);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while sending the weather data {WeatherData} to {Email} ", newWeatherData, subscription.Email);
                    }
                    subscription.LastNotifiedOn = DateTime.UtcNow; // Update the last notified timestamp
                }
            }
            else
            {
                // First time fetching data for the specified city
                _weatherDataCache[subscription.City + DateTime.UtcNow.Date] = newWeatherData;
                try
                {
                    await SendEmailNotification(subscription.Email, newWeatherData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while sending the weather data {WeatherData} to {Email} ", newWeatherData, subscription.Email);
                }
            }
        }
    }
    private bool HasWeatherChanged(WeatherData oldData, WeatherData newData)
    {
        return typeof(WeatherData).GetProperties()
                                  .Any(prop => !Equals(prop.GetValue(oldData), prop.GetValue(newData)));
    }

    private async Task SendEmailNotification(string email, WeatherData weatherData)
    {
        // Construct the subject and body of the email
        string subject = $"Weather Update for {weatherData.City}";
        string body = $"Hello,\n\n" +
                      $"Here is the latest weather update for {weatherData.City}:\n\n" +
                      $"Description: {weatherData.Description}\n" +
                      $"Temperature: {weatherData.Temperature}°C\n" +
                      $"Max Temperature: {weatherData.TempMax}°C\n" +
                      $"Min Temperature: {weatherData.TempMin}°C\n" +
                      $"Humidity: {weatherData.Humidity}%\n" +
                      $"Wind Speed: {weatherData.WindSpeed} m/s\n\n" +
                      "Thank you for subscribing to our weather updates!\n\n" +
                      "Best regards,\nWeather Lens";

        // Send the email
        await _emailService.SendEmailAsync(email, subject, body);
    }
}
