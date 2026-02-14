using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders;
using Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;

namespace Telegram.Commands.UI.DropDown
{
    public static class DropDownMenuFluentExtensions
    {
        public static void AddDropDown<TItem, TObj, TStates>(this ICallbacksBuilderBase<TObj, TStates> builder,
            DropDown<TItem> dropDown, Func<CallbackQuery, TStates, TObj, string, Task<TStates>> selectHandler = null,
            Func<CallbackQuery, TStates, TObj, string, Task<TStates>> showHandler = null)
        {
            var devPrefix = dropDown.GetDropDownPrefix();
            var allDevs = dropDown.IsAllSelected();
            var text = allDevs
                ? $"{devPrefix} {dropDown.Title}: Все"
                : $"{devPrefix} {dropDown.Title}: {string.Join(", ", dropDown.GetSelected().Select(x => x.Title))}";

            builder.Row().OnCallback(text, dropDown.CallbackData, async (q, s, o, d) =>
            {
                dropDown.Show = !dropDown.Show;
                if (showHandler != null)
                {
                    return await showHandler(q, s, o, d);
                }

                return s;
            }, true);

            AddDropDownItems(builder, dropDown, selectHandler);
        }

        public static void AddDropDownItems<TItem, TStates, TObj>(this ICallbacksBuilderBase<TObj, TStates> builder,
            DropDown<TItem> dropDown, Func<CallbackQuery, TStates, TObj, string, Task<TStates>> selectHandler = null)
        {
            if (!dropDown.Show) return;
            foreach (var item in dropDown.Items)
            {
                builder.Row()
                    .OnCallback(item.Selected
                        ? $"{Emoji.CheckMarkButton} {item.Title}"
                        : item.Title, item.CallbackData, async (q, s, o, d) =>
                    {
                        dropDown.SelectItem(d);
                        if (selectHandler != null)
                        {
                            return await selectHandler(q, s, o, d);
                        }
                        return s;
                    }, true);
            }
        }
    }
}