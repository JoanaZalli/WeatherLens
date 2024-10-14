using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;
using WeatherLens.Application;
using WeatherLens.Application.Common.Models;

public class WeatherLensService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
    private readonly AsyncTimeoutPolicy _timeoutPolicy;
    private readonly AsyncFallbackPolicy<HttpResponseMessage> _fallbackPolicy;
    private readonly WeatherApiOptions _options;

    public WeatherLensService(HttpClient httpClient, IOptions<WeatherApiOptions> weatherApiOptions)
    {
        _httpClient = httpClient;

        // Define policies
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2));

        _circuitBreakerPolicy = Policy
           .Handle<HttpRequestException>()
           .CircuitBreakerAsync(5, TimeSpan.FromMinutes(1));

        _timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(10));

        _fallbackPolicy = Policy<HttpResponseMessage>
           .Handle<HttpRequestException>()
           .FallbackAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
           {
               Content = new StringContent("Weather data unavailable, please try again later.")
           });
        _options = weatherApiOptions.Value;
    }

    /// <summary>
    /// Free version 2.5 needs to convert city name to latitude/longitude 
    /// Paid version 3.0 offers to get the data directly by providing a city and date
    /// </summary>
    /// <param name="city"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private async Task<(double lat, double lon)> GetCityCoordinates(string city)
    {
        var response = await _httpClient.GetAsync($"{_options.BaseUrl}{_options.GeoEndpoint}?q={city}&limit=1&appid={_options.ApiKey}&lang=en");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var locationData = JArray.Parse(content);

        // Check if locationData is empty or does not contain any items
        if (locationData.Count == 0)
        {
            throw new ArgumentException($"No coordinates found for city: {city}");
        }

        var lat = (double?)locationData[0]["lat"];
        var lon = (double?)locationData[0]["lon"];

        // Handle cases where lat or lon might still be null
        if (lat == null || lon == null)
        {
            throw new ArgumentException($"Coordinates for city '{city}' could not be determined.");
        }

        return (lat.Value, lon.Value);
    }

    /// <summary>
    /// Get weather data for a specific date
    /// Free version 2.5 does not have endpoints to get historical data
    /// </summary>
    /// <param name="city"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    public async Task<Result<WeatherInfo>> GetWeatherDataAsync(string city, DateTime date)
    {
        var (lat, lon) = await GetCityCoordinates(city);

        // Convert the date to Unix timestamp (required for past weather)
        long unixTimestamp = ((DateTimeOffset)date.Date).ToUnixTimeSeconds();

        // Use historical data for past dates and forecast for future dates
        //string url = date.Date < DateTime.Now.Date
        //    ? $"{_options.BaseUrl}{_options.TimeMachineEndpoint}?lat={lat}&lon={lon}&dt={unixTimestamp}&appid={_options.ApiKey}"
        //    : $"{_options.BaseUrl}{_options.ForecastEndpoint}?lat={lat}&lon={lon}&appid={_options.ApiKey}&units=metric";

        string url = $"{_options.BaseUrl}{_options.ForecastEndpoint}?lat={lat}&lon={lon}&appid={_options.ApiKey}&units=metric";

        try
        {
            // Execute the HTTP call with the combined policies
            HttpResponseMessage response = await _fallbackPolicy
                .WrapAsync(_timeoutPolicy)
                .WrapAsync(_circuitBreakerPolicy)
                .WrapAsync(_retryPolicy)
                .ExecuteAsync(() => _httpClient.GetAsync(url));

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                var weatherResponse = JsonConvert.DeserializeObject<WeatherForecast>(content);
                //Get closest forecast to required date since we using 2.5
                var result = GetClosestWeatherData(date.Date, weatherResponse!.List!);
                return new Result<WeatherInfo>(result);
            }
            else
            {
                return new Result<WeatherInfo>($"Error: {response.ReasonPhrase}");
            }
        }
        catch (HttpRequestException ex)
        {
            return new Result<WeatherInfo>($"Request error: {ex.Message}");
        }
        catch (TimeoutRejectedException)
        {
            return new Result<WeatherInfo>("Request timed out.");
        }
        catch (Exception ex)
        {
            return new Result<WeatherInfo>($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Parse the forecast response and find closest weather data for a specific date
    /// </summary>
    /// <param name="targetDate"></param>
    /// <param name="forecast"></param>
    /// <returns></returns>
    private WeatherInfo GetClosestWeatherData(DateTime targetDate, List<WeatherInfo> forecast)
    {
        WeatherInfo closestData = new WeatherInfo();
        TimeSpan closestTimeSpan = TimeSpan.MaxValue;

        foreach (var data in forecast)
        {
            DateTime forecastDate = DateTimeOffset.FromUnixTimeSeconds(data.Dt).DateTime;

            TimeSpan difference = (forecastDate.Date - targetDate.Date).Duration();

            // If this forecast is the closest to the target date, store it
            if (difference < closestTimeSpan)
            {
                closestTimeSpan = difference;
                closestData = data;
            }
        }

        return closestData;
    }
}
