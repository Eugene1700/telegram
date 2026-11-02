using Bot.Commands.Telegram.Extensions;
using System.Threading.Tasks;
using SimpleHandlers.Services.Commands.Models;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.ReplyMarkups;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Services;
using Bot.Commands.UI.DropDown;

namespace SimpleHandlers.Services.Commands
{
    [Command(Name = "showdropdown_callback")]
    public class ShowDropDownCallback : ISessionCommand<CallbackQuery, DropDownSessionObject>
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public ShowDropDownCallback(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }
        public async Task<ICommandExecutionResult> Execute(QueryContext<CallbackQuery> context, DropDownSessionObject sessionObject)
        {
            var query = context.Query;
            sessionObject.DropDown.Show = !sessionObject.DropDown.Show;
            var builder = new InlineMarkupQueryBuilder();
            builder.AddDropDown<string, ShowDropDownCallback, SelectDropDownItemCallback, DropDownSessionObject, CallbackQuery>(
                sessionObject.DropDown);
            builder.AddInlineKeyboardButton<CancelCallback, CallbackQuery>(new CallbackData
            {
                Text = "Cancel",
                CallbackText = ""
            });
            await _telegramBotClient.EditMessageText(context.GetChatId(), query.Message.MessageId, $"Ok, your dropdown",
                            replyMarkup: (InlineKeyboardMarkup) builder.GetResult());
            return CommandExecutionResult.Ahead<DropDownBehavior, DropDownSessionObject>(sessionObject);

        }
    }
}