using System.Threading.Tasks;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core;
using Bot.Commands.Core.Models;
using Bot.Commands.Telegram.Extensions;

namespace SimpleHandlers.Services.Commands
{
    [Command(Name = "cancel_callback", Authorized = true)]
    public class CancelCallback : IQueryCommand<CallbackQuery>
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public CancelCallback(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }
        public async Task<ICommandExecutionResult> Execute(QueryContext<CallbackQuery> query)
        {
            await _telegramBotClient.SendMessage(query.GetChatId(),
                $"You press cancelation. Type {TelegramCommandExtensions.GetCommandQuery<SendPhotoCommand>()} or {TelegramCommandExtensions.GetCommandQuery<DropDownMenuCommand>()}");
            return CommandExecutionResult.Break();
        }
    }
}