namespace WeatherLens.Application.Messaging;
public class WeatherNotification
{
    public required string City { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public double? Temperature { get; set; }
    public double? TempMin { get; set; }
    public double? TempMax { get; set; }
    public double? Humidity { get; set; }
    public double? WindSpeed { get; set; }
}
