using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using QuickStart;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.DependencyInjection;
using Tg = global::Telegram.Bot.Types;
using MaxTypes = global::Max.Bot.Types;

var platform = args.FirstOrDefault()?.ToLowerInvariant() ?? "telegram";
if (platform is not ("telegram" or "max" or "vk" or "instagram" or "whatsapp"))
    throw new ArgumentException("Choose 'telegram', 'max', 'vk', 'instagram', or 'whatsapp'.");

var services = new ServiceCollection();
services.AddSingleton<IBotProfile, DemoProfile>();
services.AddSingleton<IClock, UtcClock>();
services.AddSingleton<IAuthProvider, DemoAuthorization>();
// This store survives per-update scopes, but not application restarts.
services.AddSingleton<ISessionsStore, DemoSessionStore>();
if (platform == "telegram") services.UseTelegramCommandsServices();
else if (platform == "max") services.UseMaxCommandsServices();
else if (platform == "vk") services.UseVkCommandsServices();
else if (platform == "instagram") services.UseInstagramCommandsServices();
else services.UseWhatsAppCommandsServices();

using var provider = services.BuildServiceProvider();
foreach (var text in new[] { "/hello", "/ask", "Ada", "/profile", "Grace" })
{
    Console.WriteLine($"[{platform}] user: {text}");
    using var scope = provider.CreateScope();
    if (platform == "telegram")
    {
        var update = new Tg.Update
        {
            Message = new Tg.Message
            {
                Chat = new Tg.Chat { Id = 1, Type = global::Telegram.Bot.Types.Enums.ChatType.Private },
                From = new Tg.User { Id = 2, FirstName = "Reader" }, Text = text
            }
        };
        await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<Tg.Update>>().Handle(update);
    }
    else if (platform == "max")
    {
        var update = JsonSerializer.Deserialize<MaxTypes.Update>(JsonSerializer.Serialize(new
        {
            update_type = "message_created",
            message = new
            {
                sender = new { user_id = 2, name = "Reader" },
                recipient = new { chat_id = 1, chat_type = "dialog" },
                body = new { mid = Guid.NewGuid().ToString(), text }
            }
        }));
        await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<MaxTypes.Update>>().Handle(update);
    }
    else if (platform == "vk")
    {
        // VkNet uses Newtonsoft.Json for Callback API / Bots Long Poll updates.
        var update = Newtonsoft.Json.JsonConvert.DeserializeObject<VkNet.Model.GroupUpdate>(
            JsonSerializer.Serialize(new
            {
                type = "message_new", group_id = 1,
                @object = new
                {
                    message = new { peer_id = 2, from_id = 2, conversation_message_id = 1, text },
                    client_info = new { keyboard = true, inline_keyboard = true }
                }
            }));
        await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<VkNet.Model.GroupUpdate>>().Handle(update);
    }
    else if (platform == "instagram")
    {
        var update = new Instagram.Messaging.Models.InstagramMessagingEvent
        {
            Sender = new("2"), Recipient = new("99"),
            Message = new() { Mid = Guid.NewGuid().ToString(), Text = text }
        };
        await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<Instagram.Messaging.Models.InstagramMessagingEvent>>().Handle(update);
    }
    else
    {
        var update = new WhatsApp.Messaging.Models.WhatsAppMessage
        {
            From = "2", Id = "wamid." + Guid.NewGuid(), Type = "text", Text = new() { Body = text }
        };
        await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<WhatsApp.Messaging.Models.WhatsAppMessage>>().Handle(update);
    }
}
