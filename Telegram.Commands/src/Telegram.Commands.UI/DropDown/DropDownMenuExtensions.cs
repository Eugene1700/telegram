using System.Linq;
using System.Text;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Messages;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.UI.DropDown
{
    public static class DropDownMenuExtensions
    {
        public static InlineMarkupQueryBuilder AddDropDown<TItem, TShowCommand, TSelectCommand, TSessionObject, TQuery>(
            this InlineMarkupQueryBuilder builder,
            DropDown<TItem> dropDown) 
            where TShowCommand : ISessionTelegramCommand<TQuery, TSessionObject> 
            where TSelectCommand : ISessionTelegramCommand<TQuery, TSessionObject> 
        {
            var devPrefix = dropDown.GetDropDownPrefix();
            var allDevs = dropDown.IsAllSelected();
            var text = allDevs
                ? $"{devPrefix} {dropDown.Title}: Все"
                : $"{devPrefix} {dropDown.Title}: {string.Join(", ", dropDown.GetSelected().Select(x => x.Title))}";

            builder.AddInlineKeyboardButton<TShowCommand, TSessionObject, TQuery>(new CallbackData
            {
                Text =text,
                CallbackText = dropDown.CallbackData,
            });
            
            builder.AddDropDownItems<TItem, TSelectCommand, TSessionObject, TQuery>(dropDown);
            return builder;
        }

        public static InlineMarkupQueryBuilder AddDropDownItems<TItem, TSelectCommand, TSessionObject, TQuery>(
            this InlineMarkupQueryBuilder builder,
            DropDown<TItem> dropDown)
            where TSelectCommand : ISessionTelegramCommand<TQuery, TSessionObject>
        {
            if (!dropDown.Show) return builder;
            foreach (var item in dropDown.Items)
            {
                builder.AddInlineKeyboardButton<TSelectCommand, TSessionObject, TQuery>(
                    new CallbackData
                    {
                        Text = item.Selected
                            ? $"{Emoji.CheckMarkButton} {item.Title}"
                            : item.Title,
                        CallbackText = item.CallbackData
                    });
            }

            return builder;
        }
    }
}