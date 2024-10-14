using WeatherLens.Application.Common.Models;
using WeatherLens.Application.WeatherForecasts.Queries;

namespace WeatherLens.Web.Endpoints;
public class WeatherForecasts : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGet("/api/WeatherForecast", GetWeatherForecasts);
    }

    public async Task<Result<WeatherInfo>?> GetWeatherForecasts(ISender sender, [AsParameters] GetWeatherForecastQuery query)
    {
        return await sender.Send(query);
    }
}
