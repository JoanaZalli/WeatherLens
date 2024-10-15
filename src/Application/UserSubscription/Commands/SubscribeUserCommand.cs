using WeatherLens.Application.Common.Models;

public class SubscribeUserCommand : IRequest<Result>
{
    public required string Email { get; set; }
    public required string City { get; set; }

    public SubscribeUserCommand(string email, string city)
    {
        Email = email;
        City = city;
    }
}
public class SubscribeUserCommandHandler : IRequestHandler<SubscribeUserCommand, Result>
{
    private readonly IWeatherSubscriptionService _subscriptionService;

    public SubscribeUserCommandHandler(IWeatherSubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    public Task<Result> Handle(SubscribeUserCommand request, CancellationToken cancellationToken)
    {
        if (_subscriptionService.IsUserSubscribed(request.Email, request.City))
        {
            return Task.FromResult(Result.Failure(new[] { $"User already is subscribed  with email '{request.Email}' to updates for '{request.City}'." }));
        }

        // Create a new subscription
        var subscription = new UserSubscription
        {
            Email = request.Email,
            City = request.City,
            SubscribedOn = DateTime.UtcNow,
            LastNotifiedOn = DateTime.UtcNow
        };

        _subscriptionService.Subscribe(subscription);

        return Task.FromResult(Result.Success("User subscribed successfully."));
    }
}
