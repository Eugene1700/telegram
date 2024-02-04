using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleHandlers.Services.Commands.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core;
using Telegram.Commands.Core.Exceptions;
using Telegram.Commands.Core.Models;
using Telegram.Commands.Core.Services;

namespace SimpleHandlers.Services.Commands
{
    [Command(Name = "getphotosize_session", Authorized = true)]
    public class GetPhotoSizeSession: ISessionTelegramCommand<Message, List<PhotoSessionObject>>
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public GetPhotoSizeSession(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }
        public async Task<ITelegramCommandExecutionResult> Execute(Message query, List<PhotoSessionObject> sessionObject)
        {
            if (query.Type != MessageType.Photo)
                throw new TelegramDomainException("It's no photo");

            var maxSize = query.Photo?.Max(x => x.FileSize);
            sessionObject.Add(new PhotoSessionObject
            {
                MessageId = query.MessageId,
                PhotoSize = maxSize.Value
            });

            var builder = new InlineMarkupQueryBuilder();
            builder.AddMenu();
            await _telegramBotClient.SendTextMessageAsync(query.GetChatId(),
                $"Your photo size=[{maxSize}]. What I should do next (only from menu)?",
                replyMarkup: builder.GetResult());

            return TelegramCommandExecutionResult.Ahead<GetPhotoMenuBehavior, List<PhotoSessionObject>>(sessionObject, null);
        }
    }
}