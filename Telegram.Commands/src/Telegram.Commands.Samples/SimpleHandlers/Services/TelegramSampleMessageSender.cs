using System.Threading.Tasks;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Max.Models;
using Bot.Commands.Telegram.Extensions;
using global::Telegram.Bot;
using global::Max.Bot;

namespace SimpleHandlers.Services;

public sealed class TelegramSampleMessageSender(ITelegramBotClient client) : ISampleMessageSender
{
    public async Task Edit(long chatId, string messageId, IBotMessage message)
    {
        if (message.Photo is not { Length: > 0 })
        {
            await client.EditMessageText(chatId, int.Parse(messageId), message.Text,
                parseMode: message.Format.ToParseMode(), replyMarkup: message.ToReplyMarkup());
            return;
        }
        using var stream = new System.IO.MemoryStream(message.Photo, writable: false);
        var file = global::Telegram.Bot.Types.InputFile.FromStream(stream,
            message.FileName ?? (message.SendAsFile ? "attachment.bin" : "photo.jpg"));
        global::Telegram.Bot.Types.InputMedia media = message.SendAsFile
            ? new global::Telegram.Bot.Types.InputMediaDocument(file) { Caption = message.Text, ParseMode = message.Format.ToParseMode() }
            : new global::Telegram.Bot.Types.InputMediaPhoto(file) { Caption = message.Text, ParseMode = message.Format.ToParseMode() };
        await client.EditMessageMedia(chatId, int.Parse(messageId), media, replyMarkup: message.ToReplyMarkup());
    }
    public async Task Send(long chatId, IBotMessage message)
    {
        if (message.Photo is not { Length: > 0 })
        {
            await client.SendMessage(chatId, message.Text, parseMode: message.Format.ToParseMode(), replyMarkup: message.ToReplyMarkup());
            return;
        }
        using var stream = new System.IO.MemoryStream(message.Photo, writable: false);
        var file = global::Telegram.Bot.Types.InputFile.FromStream(stream,
            message.FileName ?? (message.SendAsFile ? "attachment.bin" : "photo.jpg"));
        if (message.SendAsFile)
            await client.SendDocument(chatId, file, caption: message.Text, parseMode: message.Format.ToParseMode(), replyMarkup: message.ToReplyMarkup());
        else
            await client.SendPhoto(chatId, file, caption: message.Text, parseMode: message.Format.ToParseMode(), replyMarkup: message.ToReplyMarkup());
    }
}