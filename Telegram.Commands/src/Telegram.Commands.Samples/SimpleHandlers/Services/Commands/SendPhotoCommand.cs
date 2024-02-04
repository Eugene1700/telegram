using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleHandlers.Services.Commands.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core;
using Telegram.Commands.Core.Models;

namespace SimpleHandlers.Services.Commands
{
    [Command(Name = "sendphoto", Authorized = true)]
    public class SendPhotoCommand : IQueryTelegramCommand<Message>
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public SendPhotoCommand(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }
        public async Task<ITelegramCommandExecutionResult> Execute(Message query)
        {
            await _telegramBotClient.SendTextMessageAsync(query.GetChatId(), $"Ok, send me photo");
            return TelegramCommandExecutionResult.Ahead<GetPhotoSizeSession, Message, List<PhotoSessionObject>>(
                new List<PhotoSessionObject>(), null);
        }
    }
}