using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core;
using Telegram.Commands.Core.Models;

namespace SimpleHandlers.Services.Commands
{
    [Command(Name = "start", Authorized = false)]
    public class StartCommand: IQueryTelegramCommand<Message>
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public StartCommand(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }
        public async Task<ITelegramCommandExecutionResult> Execute(Message query)
        {
            await _telegramBotClient.SendTextMessageAsync(query.GetChatId(),
                $"Hello, {query.From?.Username ?? "User"}! We are ready to start");
            return TelegramCommandExecutionResult.Break();
        }
    }
}