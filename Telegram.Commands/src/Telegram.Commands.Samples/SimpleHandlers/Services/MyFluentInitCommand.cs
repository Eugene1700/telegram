using System.Threading.Tasks;
using SimpleHandlers.Services.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core;
using Telegram.Commands.Core.Models;

namespace SimpleHandlers.Services;

[Command(Name="otherfluent")]
public class MyFluentInitCommand : IQueryTelegramCommand<Message>
{
    private readonly ITelegramBotClient _telegramBotClient;

    public MyFluentInitCommand(ITelegramBotClient telegramBotClient)
    {
        _telegramBotClient = telegramBotClient;
    }
    public async Task<ITelegramCommandExecutionResult> Execute(Message query)
    {
        await _telegramBotClient.SendTextMessageAsync(query.GetChatId(),"Hi! Tell me your name, please");
        return TelegramCommandExecutionResult.AheadFluent<MyFluentCommandFluent, MyObject, States, FluentCallbacks>(new MyObject
        {
            ChatId = query.GetChatId()
        }, false, null);
    }
}