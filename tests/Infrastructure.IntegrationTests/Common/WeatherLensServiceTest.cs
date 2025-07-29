using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WeatherLens.Application;
using Xunit;

public class WeatherLensServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly WeatherApiOptions _options;
    private readonly WeatherLensService _weatherService;
    private readonly Mock<ILogger<WeatherLensService>> _loggerMock;

    public WeatherLensServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<WeatherLensService>>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _options = new WeatherApiOptions
        {
            BaseUrl = "https://api.openweathermap.org/data/2.5/",
            GeoEndpoint = "weather",
            ForecastEndpoint = "data/2.5/forecast",
            ApiKey = "0f9d8c24e4c15521dcaac5fbf3c74e04",
            TimeMachineEndpoint = "data/2.5/onecall/timemachine",
            WeatherEndpoint = "/weather",
            Name = "Default"
        };
        _weatherService = new WeatherLensService(_httpClient, Options.Create(_options), _loggerMock.Object);
    }
     
    [Fact]
    public async Task GetWeatherDataAsync_ShouldReturnWeatherInfo_WhenApiCallIsSuccessful()
    {
        // Arrange
        var city = "London";
        var date = new DateTime(2024, 10, 14);
        var expectedLat = 51.5074;
        var expectedLon = -0.1278;

        var geoResponse = JArray.FromObject(new[]
        {
            new { lat = expectedLat, lon = expectedLon }
        });

        var forecastResponse = new WeatherForecast
        {
            List = new List<WeatherInfo>
            {
                new WeatherInfo { Dt = ((DateTimeOffset)date).ToUnixTimeSeconds(), Main = new Main { Temp = 15 } }
            }
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri == new Uri($"{_options.BaseUrl}{_options.GeoEndpoint}?q={city}&limit=1&appid={_options.ApiKey}&lang=en")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(geoResponse.ToString())
            });

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri == new Uri($"{_options.BaseUrl}{_options.ForecastEndpoint}?lat={expectedLat}&lon={expectedLon}&appid={_options.ApiKey}&units=metric")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(forecastResponse))
            });

        // Act
        var result = await _weatherService.GetWeatherDataAsync(city, date);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value?.Main?.Temp.Should().Be(15);
    }

    [Fact]
    public async Task GetWeatherDataAsync_ShouldReturnFallback_WhenApiCallFails()
    {
        // Arrange
        var city = "London";
        var date = new DateTime(2024, 10, 14);
        var expectedLat = 51.5074;
        var expectedLon = -0.1278;

        var geoResponse = JArray.FromObject(new[]
        {
            new { lat = expectedLat, lon = expectedLon }
        });

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri == new Uri($"{_options.BaseUrl}{_options.GeoEndpoint}?q={city}&limit=1&appid={_options.ApiKey}&lang=en")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(geoResponse.ToString())
            });

        // Simulate an API failure for weather data
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri == new Uri($"{_options.BaseUrl}{_options.ForecastEndpoint}?lat={expectedLat}&lon={expectedLon}&appid={_options.ApiKey}&units=metric")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        // Act
        var result = await _weatherService.GetWeatherDataAsync(city, date);

        // Assert
        result.Should().NotBeNull();
        result.Error.Should().Contain("Error: Internal Server Error");
    }

    [Fact]
    public async Task GetCityCoordinates_ShouldThrowArgumentException_WhenCoordinatesNotFound()
    {
        // Arrange
        var city = "UnknownCity";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri == new Uri($"{_options.BaseUrl}{_options.GeoEndpoint}?q={city}&limit=1&appid={_options.ApiKey}&lang=en")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]")
            });

        // Act
        Func<Task> act = async () => await _weatherService.GetWeatherDataAsync(city, DateTime.Now);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"No coordinates found for city: {city}");
    }
}
