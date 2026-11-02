using Bot.Commands.Telegram.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleHandlers.Services.Commands.Models;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.Enums;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core;
using Bot.Commands.Core.Exceptions;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Services;

namespace SimpleHandlers.Services.Commands
{
    [Command(Name = "getphotosize_session", Authorized = true)]
    public class GetPhotoSizeSession: ISessionCommand<Message, List<PhotoSessionObject>>
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public GetPhotoSizeSession(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }
        public async Task<ICommandExecutionResult> Execute(QueryContext<Message> context, List<PhotoSessionObject> sessionObject)
        {
            var query = context.Query;
            if (query.Type != MessageType.Photo)
                throw new BotDomainException("It's no photo");

            var maxSize = query.Photo?.Max(x => x.FileSize);
            sessionObject.Add(new PhotoSessionObject
            {
                MessageId = query.MessageId,
                PhotoSize = maxSize.Value
            });

            var builder = new InlineMarkupQueryBuilder();
            builder.AddMenu();
            await _telegramBotClient.SendMessage(context.GetChatId(),
                $"Your photo size=[{maxSize}]. What I should do next (only from menu)?",
                replyMarkup: builder.GetResult());

            return CommandExecutionResult.Ahead<GetPhotoMenuBehavior, List<PhotoSessionObject>>(sessionObject, null);
        }
    }
}