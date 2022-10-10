using System;
using System.Linq;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.UI.DropDown
{
    public static class DropDownMenuExtensions
    {
        public static InlineMarkupQueryBuilder AddDropDown<TItem, TShowCommand, TSelectCommand, TSessionObject>(
            this InlineMarkupQueryBuilder builder,
            DropDown<TItem> dropDown) 
            where TShowCommand : ISessionTelegramCommand<CallbackQuery, TSessionObject> 
            where TSelectCommand : ISessionTelegramCommand<CallbackQuery, TSessionObject> 
        {
            var devPrefix = dropDown.GetDropDownPrefix();
            var allDevs = dropDown.IsAllSelected();
            var text = allDevs
                ? $"{devPrefix} {dropDown.Title}: Все"
                : $"{devPrefix} {dropDown.Title}: {string.Join(", ", dropDown.GetSelected().Select(x => x.Title))}";

            builder.AddInlineKeyboardButton<TShowCommand, TSessionObject>(new CallbackData
            {
                Text =text,
                CallbackText = dropDown.CallbackData,
            });
            
            builder.AddDropDownItems<TItem, TSelectCommand, TSessionObject>(dropDown);
            return builder;
        }

        public static InlineMarkupQueryBuilder AddDropDownItems<TItem, TSelectCommand, TSessionObject>(
            this InlineMarkupQueryBuilder builder,
            DropDown<TItem> dropDown)
            where TSelectCommand : ISessionTelegramCommand<CallbackQuery, TSessionObject>
        {
            if (!dropDown.Show) return builder;
            foreach (var item in dropDown.Items)
            {
                builder.AddInlineKeyboardButton<TSelectCommand, TSessionObject>(
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