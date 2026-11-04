using System.Threading.Tasks;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Max.Models;
using Bot.Commands.Telegram.Extensions;
using global::Telegram.Bot;
using global::Max.Bot;

namespace SimpleHandlers.Services;

public interface ISampleMessageSender
{
    Task Send(long chatId, IBotMessage message);
}

public sealed class TelegramSampleMessageSender(ITelegramBotClient client) : ISampleMessageSender
{
    public Task Send(long chatId, IBotMessage message) => client.SendMessage(
            chatId, message.Text, parseMode: message.ParseMode.ToParseMode(), replyMarkup: message.ToReplyMarkup());
}

public sealed class MaxSampleMessageSender(MaxClient client) : ISampleMessageSender
{
    public Task Send(long chatId, IBotMessage message) => client.Messages.SendMessageAsync(
        chatId, message.Text, keyboard: message.ToInlineKeyboard(), format: message.ParseMode.ToTextFormat());
}
