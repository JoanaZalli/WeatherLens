using Microsoft.AspNetCore.SignalR;

namespace WeatherLens.Application.Messaging.SignalR
{
    public class WeatherHub : Hub
    {
        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }
    }
}
