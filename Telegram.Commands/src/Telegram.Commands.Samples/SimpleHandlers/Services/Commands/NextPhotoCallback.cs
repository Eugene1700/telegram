using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleHandlers.Services.Commands.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core;
using Telegram.Commands.Core.Models;

namespace SimpleHandlers.Services.Commands
{
    [Command(Name = "nextphoto_callback", Authorized = true)]
    public class NextPhotoCallback : ISessionTelegramCommand<CallbackQuery, List<PhotoSessionObject>>
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public NextPhotoCallback(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }
        public async Task<ITelegramCommandExecutionResult> Execute(CallbackQuery query, List<PhotoSessionObject> sessionObject)
        {
            await _telegramBotClient.SendTextMessageAsync(query.GetChatId(), $"Ok, send me photo");
            return TelegramCommandExecutionResult.Ahead<GetPhotoSizeSession, Message, List<PhotoSessionObject>>(
                sessionObject, null);
        }
    }
}