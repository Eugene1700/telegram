using Bot.Commands.Telegram.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleHandlers.Services.Commands.Models;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
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
    [Command(Name = "dropdown")]
    public class DropDownMenuCommand : IQueryCommand<Message>
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public DropDownMenuCommand(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }

        public async Task<ICommandExecutionResult> Execute(QueryContext<Message> context)
        {
            var query = context.Query;
            var dropDown = new DropDown<string>
            {
                Show = false, Title = "DropDownMenu", CallbackData = "DropDownMenu", NothingIsAll = false
            };
            DropDownMenuItem<string>[] items =
            {
                new DropDownMenuItem<string>
                {
                    Title = "Item1",
                    CallbackData = "item1",
                    Item = "Item1",
                    Selected = false
                },
                new DropDownMenuItem<string>
                {
                    Title = "Item2",
                    CallbackData = "item2",
                    Item = "Item2",
                    Selected = false
                },
                new DropDownMenuItem<string>
                {
                    Title = "Item3",
                    CallbackData = "item3",
                    Item = "Item3",
                    Selected = false
                },
            };
            dropDown.Items = items;
            var builder = new InlineMarkupQueryBuilder();
            builder.AddDropDown<string, ShowDropDownCallback, SelectDropDownItemCallback, DropDownSessionObject, CallbackQuery>(
                dropDown);
            builder.AddInlineKeyboardButton<CancelCallback, CallbackQuery>(new CallbackData
            {
                Text = "Cancel",
                CallbackText = ""
            });
            await _telegramBotClient.SendMessage(context.GetChatId(), $"Ok, your dropdown",
                replyMarkup: builder.GetResult());
            return CommandExecutionResult.Ahead<DropDownBehavior, DropDownSessionObject>(
                new DropDownSessionObject
                {
                    DropDown = dropDown
                });
        }
    }
}