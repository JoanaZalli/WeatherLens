using Microsoft.AspNetCore.Mvc;
using WeatherLens.Application.Common.Models;

namespace WeatherLens.Web.Endpoints;
public class UserSubscription : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapPost("/api/UserSubscription", SubscribeUser);
    }

    public async Task<Result> SubscribeUser(ISender sender, [FromBody] SubscribeUserCommand command)
    {
        return await sender.Send(command);
    }
}
