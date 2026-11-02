using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using QuickStart;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.DependencyInjection;
using Tg = global::Telegram.Bot.Types;
using MaxTypes = global::Max.Bot.Types;

var platform = args.FirstOrDefault()?.ToLowerInvariant() ?? "telegram";
if (platform is not ("telegram" or "max"))
    throw new ArgumentException("Choose 'telegram' or 'max'.");

var services = new ServiceCollection();
services.AddSingleton<IBotProfile, DemoProfile>();
services.AddSingleton<IClock, UtcClock>();
services.AddSingleton<IAuthProvider, DemoAuthorization>();
// This store survives per-update scopes, but not application restarts.
services.AddSingleton<ISessionsStore, DemoSessionStore>();
if (platform == "telegram") services.UseTelegramCommandsServices();
else services.UseMaxCommandsServices();

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
    else
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
}
