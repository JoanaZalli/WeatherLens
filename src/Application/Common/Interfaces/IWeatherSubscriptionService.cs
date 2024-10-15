public interface IWeatherSubscriptionService
{
    void Subscribe(UserSubscription userSubscription);
    bool IsUserSubscribed(string email, string city);
    void Unsubscribe(string email, string city);
    Task CheckForUpdatesAsync();
}
