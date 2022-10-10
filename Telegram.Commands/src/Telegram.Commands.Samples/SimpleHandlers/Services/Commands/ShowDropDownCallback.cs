using System.Threading.Tasks;
using SimpleHandlers.Services.Commands.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core;
using Telegram.Commands.Core.Models;
using Telegram.Commands.Core.Services;
using Telegram.Commands.UI.DropDown;

namespace SimpleHandlers.Services.Commands
{
    [Command(Name = "showdropdown_callback")]
    public class ShowDropDownCallback : ISessionTelegramCommand<CallbackQuery, DropDownSessionObject>
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public ShowDropDownCallback(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }
        public async Task<ITelegramCommandExecutionResult> Execute(CallbackQuery query, DropDownSessionObject sessionObject)
        {
            sessionObject.DropDown.Show = !sessionObject.DropDown.Show;
            var builder = new InlineMarkupQueryBuilder();
            builder.AddDropDown<string, ShowDropDownCallback, SelectDropDownItemCallback, DropDownSessionObject>(
                sessionObject.DropDown);
            builder.AddInlineKeyboardButton<CancelCallback>(new CallbackData
            {
                Text = "Cancel",
                CallbackText = ""
            });
            await _telegramBotClient.EditMessageTextAsync(query.GetChatId(), query.Message.MessageId, $"Ok, your dropdown",
                            replyMarkup: (InlineKeyboardMarkup) builder.GetResult());
            return TelegramCommandExecutionResult.Ahead<DropDownBehavior, DropDownSessionObject>(sessionObject);

        }
    }
}