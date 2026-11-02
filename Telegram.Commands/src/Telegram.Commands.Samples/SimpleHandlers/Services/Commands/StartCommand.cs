using System.Threading.Tasks;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core;
using Bot.Commands.Core.Models;

namespace SimpleHandlers.Services.Commands
{
    [Command(Name = "start", Authorized = false)]
    public class StartCommand: IQueryCommand<Message>
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public StartCommand(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }
        public async Task<ICommandExecutionResult> Execute(QueryContext<Message> context)
        {
            var query = context.Query;
            await _telegramBotClient.SendMessage(context.GetChatId(),
                $"Hello, {query.From?.Username ?? "User"}! We are ready to start");
            return CommandExecutionResult.Break();
        }
    }
}