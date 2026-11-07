using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace SimpleHandlers.Services;

public sealed class BotRuntime(IHttpContextAccessor accessor)
{
    public SamplePlatform Platform =>
        accessor.HttpContext?.Request.Path.StartsWithSegments("/api/whatsapp") == true ? SamplePlatform.WhatsApp :
        accessor.HttpContext?.Request.Path.StartsWithSegments("/api/instagram") == true ? SamplePlatform.Instagram :
        accessor.HttpContext?.Request.Path.StartsWithSegments("/api/vk") == true ? SamplePlatform.Vk :
        accessor.HttpContext?.Request.Path.StartsWithSegments("/api/max") == true ? SamplePlatform.Max :
        SamplePlatform.Telegram;
    public bool IsMax => Platform == SamplePlatform.Max;
    public string ProfileSection => Section(Platform);
    public static string Section(SamplePlatform platform) => platform switch
    {
        SamplePlatform.WhatsApp => "WhatsAppBotProfile",
        SamplePlatform.Instagram => "InstagramBotProfile",
        SamplePlatform.Vk => "VkBotProfile",
        SamplePlatform.Max => "MaxBotProfile",
        _ => "BotProfile"
    };

    public static BotProfile ReadProfile(IConfiguration configuration, bool max)
        => ReadProfile(configuration, max ? SamplePlatform.Max : SamplePlatform.Telegram);

    public static BotProfile ReadProfile(IConfiguration configuration, SamplePlatform platform)
    {
        var section = configuration.GetSection(Section(platform));
        return new BotProfile
        {
            Key = section["Token"], BaseUrl = section["BaseUrl"], BotName = section["BotName"],
            UpdateRoute = platform switch
            {
                SamplePlatform.WhatsApp => Settings.WhatsAppRouteToUpdate,
                SamplePlatform.Instagram => Settings.InstagramRouteToUpdate,
                SamplePlatform.Vk => Settings.VkRouteToUpdate,
                SamplePlatform.Max => Settings.MaxRouteToUpdate,
                _ => Settings.RouteToUpdate
            },
            Swarms = new[] { platform.ToString().ToLowerInvariant() }
        };
    }
}
