using WeatherLens.Application.Common.Models;

public class UnsubscribeUserCommand : IRequest<Result>
{
    public required string Email { get; set; }
    public required string City { get; set; }

    public UnsubscribeUserCommand(string email, string city)
    {
        Email = email;
        City = city;
    }
}
public class UnsubscribeUserCommandHandler : IRequestHandler<UnsubscribeUserCommand, Result>
{
    private readonly IWeatherSubscriptionService _subscriptionService;

    public UnsubscribeUserCommandHandler(IWeatherSubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    public Task<Result> Handle(UnsubscribeUserCommand request, CancellationToken cancellationToken)
    {
        _subscriptionService.Unsubscribe(request.Email, request.City);

        return Task.FromResult(Result.Success("User unsubscribed successfully."));
    }
}
