using Bot.Commands.Abstract.Messages;
using Bot.Commands.Telegram.Extensions;
using Bot.Commands.Max.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Max.Bot;
using Max.Bot.Types.Requests;
using Max.Bot.Types.Enums;

namespace ReportsBot;

public interface IOutput
{
    Task Show(long chatId, IBotMessage message, string messageId = null, bool edit = false);
    Task File(long chatId, ReportFile report);
    Task Text(long chatId, string text) => Show(chatId, new BotMessage(text, null, null, null));
}

public sealed class ConsoleOutput(string platform) : IOutput
{
    public List<(string Text, string Payload)> Buttons { get; private set; } = new();
    public List<string> Texts { get; } = new();
    public List<ReportFile> Files { get; } = new();
    public Task Show(long chatId, IBotMessage message, string messageId = null, bool edit = false)
    {
        Texts.Add(message.Text);
        Console.WriteLine($"\nБот: {message.Text}");
        // Render with the actual adapter, preserving its generated callback payloads.
        var buttons = platform == "telegram"
            ? message.ToReplyMarkup().InlineKeyboard.SelectMany(x => x).Select(x => (x.Text, x.CallbackData)).ToList()
            : message.ToInlineKeyboard()?.Buttons.SelectMany(x => x).Select(x => (x.Text, x.Payload)).ToList() ?? new();
        if (buttons.Count > 0)
        {
            if (platform == "telegram" && buttons.Any(x => System.Text.Encoding.UTF8.GetByteCount(x.Item2) > 64))
                throw new InvalidOperationException("Generated Telegram callback exceeds 64 bytes.");
            Buttons = buttons;
            for (var i = 0; i < buttons.Count; i++) Console.WriteLine($"  #{i + 1} {buttons[i].Text}");
        }
        return Task.CompletedTask;
    }
    public Task File(long chatId, ReportFile report)
    {
        Files.Add(report);
        Console.WriteLine($"Файл: {report.Path}");
        return Task.CompletedTask;
    }
}

public sealed class TelegramOutput(ITelegramBotClient client) : IOutput
{
    public async Task Show(long chatId, IBotMessage message, string messageId = null, bool edit = false)
    {
        try
        {
            if (edit && int.TryParse(messageId, out var id))
                await client.EditMessageText(chatId, id, message.Text, replyMarkup: message.ToReplyMarkup());
            else
                await client.SendMessage(chatId, message.Text, replyMarkup: message.ToReplyMarkup());
        }
        catch (Telegram.Bot.Exceptions.ApiRequestException ex) when (ex.Message.Contains("message is not modified", StringComparison.OrdinalIgnoreCase)) { }
    }
    public async Task File(long chatId, ReportFile report)
    {
        await using var stream = System.IO.File.OpenRead(report.Path);
        await client.SendDocument(chatId, InputFile.FromStream(stream, Path.GetFileName(report.Path)), caption: "Демонстрационный отчёт");
    }
}

public sealed class MaxOutput(MaxClient client) : IOutput
{
    public async Task Show(long chatId, IBotMessage message, string messageId = null, bool edit = false)
    {
        var keyboard = message.ToInlineKeyboard();
        if (edit && messageId != null)
            await client.Messages.EditMessageAsync(messageId, new EditMessageRequest
            {
                Text = message.Text,
                Attachments = keyboard == null ? Array.Empty<AttachmentRequest>() : new[] { AttachmentRequest.Keyboard(keyboard) }
            });
        else
            await client.Messages.SendMessageAsync(chatId, message.Text, keyboard: keyboard);
    }
    public async Task File(long chatId, ReportFile report)
    {
        await using var stream = System.IO.File.OpenRead(report.Path);
        var uploaded = await client.Files.UploadAsync(UploadType.File, stream, Path.GetFileName(report.Path));
        await client.Messages.SendMessageWithAttachmentsAsync(new[] { AttachmentRequest.FromUpload(UploadType.File, uploaded) }, chatId: chatId, text: "Демонстрационный отчёт");
    }
}
