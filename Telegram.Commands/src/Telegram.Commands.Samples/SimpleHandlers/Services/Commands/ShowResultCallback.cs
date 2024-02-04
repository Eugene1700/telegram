using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SimpleHandlers.Services.Commands.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core;
using Telegram.Commands.Core.Models;
using Telegram.Commands.Core.Services;

namespace SimpleHandlers.Services.Commands
{
    [Command(Name = "showresult_callback", Authorized = true)]
    public class ShowResultCallback : ISessionTelegramCommand<CallbackQuery, List<PhotoSessionObject>>
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public ShowResultCallback(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }
        public async Task<ITelegramCommandExecutionResult> Execute(CallbackQuery query, List<PhotoSessionObject> sessionObject)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Your result:");
            foreach (var photoSessionObject in sessionObject)
            {
                sb.AppendLine($"MessageId: {photoSessionObject.MessageId}, FileSize: {photoSessionObject.PhotoSize}");
            }

            var builder = new InlineMarkupQueryBuilder();
            builder.AddMenu();
            await _telegramBotClient.SendTextMessageAsync(query.GetChatId(), sb.ToString(),
                replyMarkup: builder.GetResult());
            return TelegramCommandExecutionResult.Ahead<GetPhotoMenuBehavior, List<PhotoSessionObject>>(sessionObject, null);
        }
    }
}