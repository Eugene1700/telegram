using System.Collections.Generic;
using System.Linq;
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
using Telegram.Commands.UI.DropDown;

namespace SimpleHandlers.Services.Commands
{
    [Command(Name = "dropdown")]
    public class DropDownMenuCommand : IQueryTelegramCommand<Message>
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public DropDownMenuCommand(ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient;
        }

        public async Task<ITelegramCommandExecutionResult> Execute(Message query)
        {
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
            builder.AddDropDown<string, ShowDropDownCallback, SelectDropDownItemCallback, DropDownSessionObject>(
                dropDown);
            builder.AddInlineKeyboardButton<CancelCallback>(new CallbackData
            {
                Text = "Cancel",
                CallbackText = ""
            });
            await _telegramBotClient.SendTextMessageAsync(query.GetChatId(), $"Ok, your dropdown",
                replyMarkup: builder.GetResult());
            return TelegramCommandExecutionResult.Ahead<DropDownBehavior, DropDownSessionObject>(
                new DropDownSessionObject
                {
                    DropDown = dropDown
                });
        }
    }
}