using WeatherLens.Application.Common.Models;

public class SubscribeUserCommand : IRequest<Result>
{
    public string Email { get; set; }
    public string City { get; set; }

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
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.City))
        {
            return Task.FromResult(Result.Failure(new[] { "An error occurred." }));
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
