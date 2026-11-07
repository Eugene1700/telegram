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
    Task Edit(long chatId, string messageId, IBotMessage message);
}
