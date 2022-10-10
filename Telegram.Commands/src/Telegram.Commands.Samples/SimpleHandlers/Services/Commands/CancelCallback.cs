using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core;
using Telegram.Commands.Core.Models;

namespace SimpleHandlers.Services.Commands
{
    [Command(Name = "cancel_callback", Authorized = true)]
    public class CancelCallback : IQueryTelegramCommand<CallbackQuery>
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public CancelCallback(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }
        public async Task<ITelegramCommandExecutionResult> Execute(CallbackQuery query)
        {
            await _telegramBotClient.SendTextMessageAsync(query.GetChatId(),
                $"You press cancelation. Type {TelegramCommandExtensions.GetCommandQuery<SendPhotoCommand>()} or {TelegramCommandExtensions.GetCommandQuery<DropDownMenuCommand>()}");
            return TelegramCommandExecutionResult.Break();
        }
    }
}