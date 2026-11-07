using System.Text.Json;
using Bot.Commands.Abstract.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Tg = Telegram.Bot.Types;
using Mx = Max.Bot.Types;

namespace ReportsBot;

// Uses genuine SDK updates and generated callback payloads; no alternate flow implementation.
public sealed class Demo(string platform, ServiceProvider provider, ConsoleOutput output)
{
    private int _messageId = 1;
    public async Task Input(string text, long chat = 1, long user = 2)
    {
        string payload = null;
        if (text.StartsWith('#'))
        {
            if (!int.TryParse(text.AsSpan(1), out var index) || index < 1 || index > output.Buttons.Count)
                throw new ArgumentException("Введите номер видимой кнопки, например #1.");
            payload = output.Buttons[index - 1].Payload;
        }
        Console.WriteLine($"\nПользователь: {text}");
        if (platform == "telegram")
        {
            var message = new Tg.Message { Id = _messageId++, Chat = new Tg.Chat { Id = chat, Type = Telegram.Bot.Types.Enums.ChatType.Private }, From = new Tg.User { Id = user, FirstName = "Reader" }, Text = text };
            var update = payload == null ? new Tg.Update { Message = message }
                : new Tg.Update { CallbackQuery = new Tg.CallbackQuery { Id = Guid.NewGuid().ToString("N"), From = message.From, Message = message, Data = payload } };
            await App.Dispatch(provider, update);
        }
        else
        {
            var message = new { sender = new { user_id = user, name = "Reader" }, recipient = new { chat_id = chat, chat_type = "dialog" }, body = new { mid = (_messageId++).ToString(), text } };
            object update = payload == null ? new { update_type = "message_created", message }
                : new { update_type = "message_callback", message, callback = new { callback_id = Guid.NewGuid().ToString("N"), user = new { user_id = user, name = "Reader" }, payload } };
            await App.Dispatch(provider, JsonSerializer.Deserialize<Mx.Update>(JsonSerializer.Serialize(update)));
        }
    }

    public Task Click(string label)
    {
        var index = output.Buttons.FindIndex(x => x.Text == label || x.Text == "✅ " + label);
        if (index < 0) throw new InvalidOperationException($"Button not found: {label}");
        return Input($"#{index + 1}");
    }

    public async Task Event()
    {
        if (platform == "telegram")
            await App.Dispatch(provider, new Tg.Update { Message = new Tg.Message
            {
                Chat = new Tg.Chat { Id = -100, Type = Telegram.Bot.Types.Enums.ChatType.Supergroup },
                From = new Tg.User { Id = 2, FirstName = "Reader" }, NewChatMembers = new[] { new Tg.User { Id = 3, FirstName = "Guest" } }
            }});
        else
            await App.Dispatch(provider, JsonSerializer.Deserialize<Mx.Update>("""
                {"update_type":"user_added","chat_id":-100,"user":{"user_id":3,"name":"Guest"},"inviter_id":2,"timestamp":0}
                """));
    }

    public static async Task Interactive(string platform)
    {
        var output = new ConsoleOutput(platform);
        using var provider = App.Create(platform, output);
        var demo = new Demo(platform, provider, output);
        Console.WriteLine($"ReportsBot — {platform}, offline. #N — кнопка, /event — новый участник, /quit — выход.");
        await demo.Input("/start");
        while (Console.ReadLine() is { } text && text != "/quit")
        {
            try { if (text == "/event") await demo.Event(); else await demo.Input(text); }
            catch (Exception ex) { Console.WriteLine($"Не удалось обработать ввод ({ex.GetType().Name}). /cancel — сбросить диалог."); }
        }
    }

    public static async Task Verify(string platform)
    {
        var output = new ConsoleOutput(platform);
        using var provider = App.Create(platform, output, directory: Path.Combine(AppContext.BaseDirectory, "artifacts", "self-test", platform, Guid.NewGuid().ToString("N")));
        var demo = new Demo(platform, provider, output);
        var sessions = provider.GetRequiredService<ISessionsStore>();
        var reports = provider.GetRequiredService<ReportRepository>();
        void Check(bool condition, string description) { if (!condition) throw new InvalidOperationException($"{platform}: {description}"); }

        await demo.Input("/report");
        await demo.Input("not-a-month");
        Check(output.Texts.Last().Contains("Некорректный"), "period validation");
        await demo.Input("/help");
        Check(sessions.GetSessionInfo(DateTime.UtcNow, 1, 2) != null, "help preserves session");
        await demo.Input("2026-10");
        await demo.Click("Продолжить");
        Check(output.Texts.Last().Contains("Выберите подразделение перед"), "empty selection rejected");
        await demo.Click("Продажи");
        await demo.Input("#1");
        Check(!output.Buttons.Any(x => x.Text == "✅ Продажи"), "dropdown collapse");
        await demo.Input("#1");
        Check(output.Buttons.Any(x => x.Text == "✅ Продажи"), "dropdown expand preserves selection");
        await demo.Click("Логистика");
        await demo.Click("Логистика");
        await demo.Click("Продолжить");
        await demo.Click("HTML");
        await demo.Click("Изменить период");
        await demo.Input("2026-09");
        Check(output.Buttons.Any(x => x.Text.Contains("✅ Продажи")), "selection retained on back navigation");
        await demo.Click("Продолжить");
        await demo.Click("HTML");
        await demo.Click("Сформировать");
        var html = output.Files.Single();
        var body = await File.ReadAllTextAsync(html.Path);
        Check(body.Contains("2026-09") && body.Contains("Продажи") && !body.Contains("Логистика"), "HTML uses selected parameters");
        Check(sessions.GetSessionInfo(DateTime.UtcNow, 1, 2) == null, "completion releases session");
        await demo.Input("/report");
        await demo.Input("/cancel");
        Check(sessions.GetSessionInfo(DateTime.UtcNow, 1, 2) == null && reports.For(1, 2).Count == 1, "cancel does not generate report");

        for (var month = 1; month <= 4; month++)
        {
            await demo.Input("/report");
            await demo.Input($"2026-{month:00}");
            await demo.Click("Продажи");
            await demo.Click("Продолжить");
            await demo.Click("CSV");
            await demo.Click("Сформировать");
        }
        Check((await File.ReadAllTextAsync(output.Files.Last().Path)).Contains("2026-04,Продажи,41"), "CSV values");
        await demo.Input("/archive");
        await demo.Click("> 2");
        Check(output.Texts.Last().Contains("Страница 2"), "pagination next");
        var filesBefore = output.Files.Count;
        await demo.Input("#1");
        Check(output.Files.Count == filesBefore + 1, "archive download");
        await demo.Click("1 <");
        Check(output.Texts.Last().Contains("Страница 1"), "pagination previous");
        await demo.Click("Закрыть архив");
        await demo.Input("/archive", user: 99);
        Check(output.Texts.Last().Contains("Архив пуст"), "user isolation");
        await demo.Input("/archive", chat: 99);
        Check(output.Texts.Last().Contains("Архив пуст"), "chat isolation");
        await demo.Event();
        Check(output.Texts.Last().Contains("Добро пожаловать"), "member event");
        Console.WriteLine($"PASS: {platform}");
    }
}
