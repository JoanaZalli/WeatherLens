using WeatherLens.Application.Common.Models;

public interface IWeatherService
{
    Task<Result<WeatherInfo>> GetWeatherDataAsync(string city, DateTime date);
}
