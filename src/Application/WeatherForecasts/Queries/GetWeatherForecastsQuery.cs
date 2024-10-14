using WeatherLens.Application.Common.Models;

namespace WeatherLens.Application.WeatherForecasts.Queries;

public record GetWeatherForecastQuery : IRequest<Result<WeatherInfo>?>
{
    public required string City { get; set; }
    public DateTime Date { get; set; }
}

public class GetWeatherForecastQueryHandler : IRequestHandler<GetWeatherForecastQuery, Result<WeatherInfo>?>
{
    private readonly IWeatherService _weatherService;

    public GetWeatherForecastQueryHandler(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    public async Task<Result<WeatherInfo>?> Handle(GetWeatherForecastQuery request, CancellationToken cancellationToken)
    {
        var data = await _weatherService.GetWeatherDataAsync(request.City, request.Date);
        return data;
    }
}
public class GetWeatherForecastQueryValidator : AbstractValidator<GetWeatherForecastQuery>
{
    public GetWeatherForecastQueryValidator()
    {
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MinimumLength(2).WithMessage("City name must be at least 2 characters long.")
            .Matches("^[a-zA-Z ]*$").WithMessage("City name can only contain letters.");

        RuleFor(x => x.Date)
            .NotNull();
    }
}

