public class UserSubscription
{
    public required string Email { get; set; }
    public required string City { get; set; }
    public DateTime SubscribedOn { get; set; }
    public DateTime? LastNotifiedOn { get; set; }
}
