using Bot.Commands.Telegram.Extensions;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SimpleHandlers.Services.Commands.Models;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Services;

namespace SimpleHandlers.Services.Commands
{
    [Command(Name = "showresult_callback", Authorized = true)]
    public class ShowResultCallback : ISessionCommand<CallbackQuery, List<PhotoSessionObject>>
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public ShowResultCallback(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }
        public async Task<ICommandExecutionResult> Execute(QueryContext<CallbackQuery> context, List<PhotoSessionObject> sessionObject)
        {
            var query = context.Query;
            var sb = new StringBuilder();
            sb.AppendLine("Your result:");
            foreach (var photoSessionObject in sessionObject)
            {
                sb.AppendLine($"MessageId: {photoSessionObject.MessageId}, FileSize: {photoSessionObject.PhotoSize}");
            }

            var builder = new InlineMarkupQueryBuilder();
            builder.AddMenu();
            await _telegramBotClient.SendMessage(context.GetChatId(), sb.ToString(),
                replyMarkup: builder.GetResult());
            return CommandExecutionResult.Ahead<GetPhotoMenuBehavior, List<PhotoSessionObject>>(sessionObject, null);
        }
    }
}