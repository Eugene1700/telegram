using System.Threading.Tasks;
using SimpleHandlers.Services.Commands;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core;
using Bot.Commands.Core.Models;

namespace SimpleHandlers.Services
{
    [Command(Name="otherfluent")]
    public class MyFluentInitCommand : IQueryCommand<Message>
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public MyFluentInitCommand(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }

        public async Task<ICommandExecutionResult> Execute(QueryContext<Message> query)
        {
            await _telegramBotClient.SendMessage(query.GetChatId(),"Hi! Tell me your name, please");
            return CommandExecutionResult.AheadFluent<MyFluentCommandFluent, MyObject, States>(new MyObject
            {
                ChatId = query.GetChatId()
            }, false, null);
        }
    }
}