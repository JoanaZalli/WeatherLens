namespace WeatherLens.Application;
public class WeatherApiOptions
{
    public const string Key = "OpenWeatherMap";
    public required string BaseUrl { get; set; }
    public required string Name { get; set; }
    public required string ApiKey { get; set; }
    public required string WeatherEndpoint { get; set; }
    public required string GeoEndpoint { get; set; }
    public required string TimeMachineEndpoint { get; set; }
    public required string ForecastEndpoint { get; set; }
}
