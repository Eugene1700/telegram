using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SimpleHandlers.Domain;

namespace SimpleHandlers.Services;

public sealed class BotRuntime(IHttpContextAccessor accessor)
{
    public bool IsMax => accessor.HttpContext?.Request.Path.StartsWithSegments("/api/max") == true;
    public BotPlatform Platform => IsMax ? BotPlatform.Max : BotPlatform.Telegram;

    public static BotProfile ReadProfile(IConfiguration configuration, bool max)
    {
        var section = configuration.GetSection(max ? "MaxBotProfile" : "BotProfile");
        return new BotProfile
        {
            Key = section["Token"], BaseUrl = section["BaseUrl"], BotName = section["BotName"],
            UpdateRoute = max ? Settings.MaxRouteToUpdate : Settings.RouteToUpdate,
            Swarms = new[] { max ? "max" : "telegram" }
        };
    }
}
