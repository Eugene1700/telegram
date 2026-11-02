using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleHandlers.Services.Commands.Models;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core;
using Bot.Commands.Core.Models;

namespace SimpleHandlers.Services.Commands
{
    [Command(Name = "nextphoto_callback", Authorized = true)]
    public class NextPhotoCallback : ISessionCommand<CallbackQuery, List<PhotoSessionObject>>
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public NextPhotoCallback(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }
        public async Task<ICommandExecutionResult> Execute(QueryContext<CallbackQuery> context, List<PhotoSessionObject> sessionObject)
        {
            var query = context.Query;
            await _telegramBotClient.SendMessage(context.GetChatId(), $"Ok, send me photo");
            return CommandExecutionResult.Ahead<GetPhotoSizeSession, Message, List<PhotoSessionObject>>(
                sessionObject, null);
        }
    }
}