using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Exceptions;
using Bot.Commands.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using QuickStart;
using ReportsBot;
using Telegram.Bot;
using Tg = Telegram.Bot.Types;
using Mx = Max.Bot.Types;

Console.OutputEncoding = System.Text.Encoding.UTF8;
var mode = args.FirstOrDefault() ?? "demo";
if (mode is not ("demo" or "telegram" or "max" or "self-test"))
    throw new ArgumentException("Usage: dotnet run -- [demo [telegram|max] | telegram | max | self-test]");
if (mode == "self-test")
{
    await Demo.Verify("telegram");
    await Demo.Verify("max");
    Console.WriteLine("PASS: report flows and archives on both adapters.");
    return;
}
if (mode == "demo")
{
    await Demo.Interactive(args.ElementAtOrDefault(1) ?? "telegram");
    return;
}

var tokenName = mode == "telegram" ? "TELEGRAM_BOT_TOKEN" : "MAX_BOT_TOKEN";
var token = Environment.GetEnvironmentVariable(tokenName);
if (string.IsNullOrWhiteSpace(token))
{
    Console.Error.WriteLine($"Set {tokenName} in your environment, or run 'demo' without a token.");
    Environment.ExitCode = 1;
    return;
}

using var stopping = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; stopping.Cancel(); };
try
{
    if (mode == "telegram")
    {
        var client = new TelegramBotClient(token);
        var me = await client.GetMe(stopping.Token);
        var webhook = await client.GetWebhookInfo(stopping.Token);
        if (!string.IsNullOrEmpty(webhook.Url))
            throw new InvalidOperationException("This bot has a webhook. Use a separate demo bot or remove the webhook yourself before polling.");
        IOutput output = new TelegramOutput(client);
        using var provider = App.Create("telegram", output, me.Username ?? "reports_bot");
        Console.WriteLine("Telegram bot is polling. Ctrl+C to stop. Open your bot and send /report.");
        var offset = 0;
        while (!stopping.IsCancellationRequested)
        {
            var updates = await client.GetUpdates(offset: offset, timeout: 25, cancellationToken: stopping.Token);
            foreach (var update in updates)
            {
                var chat = update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id;
                await App.DispatchSafely(provider, update, output, chat, async () =>
                {
                    if (update.CallbackQuery is { } callback) await client.AnswerCallbackQuery(callback.Id, cancellationToken: stopping.Token);
                });
                offset = update.Id + 1;
            }
        }
    }
    else
    {
        using var client = new Max.Bot.MaxClient(token);
        var me = await client.Bot.GetMeAsync(stopping.Token);
        IOutput output = new MaxOutput(client);
        using var provider = App.Create("max", output, me.Username ?? "reports_bot");
        Console.WriteLine("Max bot is polling. Ctrl+C to stop. Send /report to your bot.");
        long? marker = null;
        while (!stopping.IsCancellationRequested)
        {
            var batch = await client.Subscriptions.GetUpdatesAsync(new Max.Bot.Types.Requests.GetUpdatesRequest { Marker = marker, Timeout = 25 }, stopping.Token);
            foreach (var update in batch.Updates)
            {
                var chat = update.Message?.Recipient?.ChatId ?? update.ChatId;
                if (update.Type == Max.Bot.Types.Enums.UpdateType.BotStarted && chat is { } startedChat)
                    await output.Text(startedChat, "Добро пожаловать! /report — создать отчёт, /help — справка.");
                else
                    await App.DispatchSafely(provider, update, output, chat, async () =>
                    {
                        if (update.Callback is { } callback)
                            await client.Messages.AnswerCallbackQueryAsync(callback.CallbackId, new Max.Bot.Types.Requests.AnswerCallbackQueryRequest { Notification = "Принято" });
                    });
            }
            marker = batch.Marker;
        }
    }
}
catch (OperationCanceledException) when (stopping.IsCancellationRequested) { }
catch (Exception ex)
{
    // Do not print SDK exception bodies or request URLs that might contain credentials.
    Console.Error.WriteLine($"Bot stopped ({ex.GetType().Name}). Check token, connection and polling configuration.");
    Environment.ExitCode = 1;
}

namespace ReportsBot
{
    public static class App
    {
        public static ServiceProvider Create(string platform, IOutput output, string name = "reports_bot", string directory = null)
        {
            if (platform is not ("telegram" or "max")) throw new ArgumentException("Choose telegram or max.");
            var services = new ServiceCollection();
            services.AddSingleton<IBotProfile>(new Profile(platform, name));
            services.AddSingleton<IClock, UtcClock>();
            services.AddSingleton<IAuthProvider, DemoAuthorization>();
            services.AddSingleton<ISessionsStore, DemoSessionStore>();
            services.AddSingleton(output);
            services.AddSingleton(new ReportRepository(directory ?? Path.Combine(AppContext.BaseDirectory, "artifacts", platform)));
            if (platform == "telegram") services.UseTelegramCommandsServices();
            else services.UseMaxCommandsServices();
            return services.BuildServiceProvider();
        }

        public static async Task Dispatch<T>(ServiceProvider provider, T update)
        {
            using var scope = provider.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<T>>().Handle(update);
        }

        public static async Task DispatchSafely<T>(ServiceProvider provider, T update, IOutput output, long? chat, Func<Task> acknowledge)
        {
            try { await acknowledge(); await Dispatch(provider, update); }
            catch (CommandExtractionException)
            {
                if (chat is { } id) await output.Text(id, "Команда или кнопка недоступна. Используйте /cancel, затем /report или /archive.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Update failed ({ex.GetType().Name}).");
                if (chat is { } id) await output.Text(id, "Не удалось обработать запрос. Попробуйте ещё раз или отправьте /cancel.");
            }
        }
    }

    public sealed class Profile(string platform, string name) : IBotProfile
    {
        public string Platform => platform;
        public string Key => "";
        public string BaseUrl => "";
        public string UpdateRoute => "/updates";
        public string BotName => name;
        public GroupMessageHandlingStrategy GroupMessageHandlingStrategy => GroupMessageHandlingStrategy.Direct;
        public string[] Swarms => null;
    }
}
