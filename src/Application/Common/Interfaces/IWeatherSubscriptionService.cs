public interface IWeatherSubscriptionService
{
    void Subscribe(UserSubscription userSubscription);
    void Unsubscribe(UserSubscription userSubscription);
    Task CheckForUpdatesAsync();
}
