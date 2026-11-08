using System.Text.Json;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Exceptions;
using Bot.Commands.Core.Models;
using Bot.Commands.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SurveyComparison;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

if (args.Length != 1) throw new ArgumentException("Pass a storage file path or --memory.");
var services = new ServiceCollection();
services.AddSingleton<IBotProfile, DemoProfile>();
services.AddSingleton<IClock, DemoClock>();
services.AddSingleton<IAuthProvider, DemoAuth>();
services.AddSingleton<Output>();
services.UseTelegramCommandsServices();
if (args[0] == "--memory") services.UseInMemoryStorage();
else services.AddScoped<ISessionsStore>(sp => new FileSessionsStore(args[0], sp.GetRequiredService<IBotProfile>()));
using var provider = services.BuildServiceProvider();
string line;
while ((line = Console.ReadLine()) != null)
{
    using var input = JsonDocument.Parse(line);
    using var scope = provider.CreateScope();
    var root = input.RootElement;
    var message = new Message { Id = 1, Date = DateTime.UtcNow,
        Chat = new Chat { Id = 2, Type = ChatType.Private }, From = new User { Id = 2, FirstName = "Reader" },
        Text = root.TryGetProperty("text", out var text) ? text.GetString() : null };
    var update = root.TryGetProperty("callback", out var callback)
        ? new Update { CallbackQuery = new CallbackQuery { Id = "callback", From = message.From,
            Message = message, Data = callback.GetString(), ChatInstance = "demo" } }
        : new Update { Message = message };
    var output = provider.GetRequiredService<Output>();
    try { await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<Update>>().Handle(update); }
    catch (CommandExtractionException) { output.Show(new Bot.Commands.Abstract.Messages.BotMessage("Начните /order.", null, null, null)); }
    output.Flush();
}
