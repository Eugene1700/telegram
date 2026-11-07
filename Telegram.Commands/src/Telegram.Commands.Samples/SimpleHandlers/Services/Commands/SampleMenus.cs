using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Services;
using Bot.Commands.Core.Exceptions;
using Bot.Commands.UI.DropDown;
using SimpleHandlers.Services.Commands.Models;
namespace SimpleHandlers.Services.Commands;
internal static class SampleMenus
{
    public static DropDownSessionObject DropDown(bool multiple) => new()
    {
        DropDown = new DropDown<string>
        {
            Title = "DropDownMenu", CallbackData = "DropDownMenu", Show = false, NothingIsAll = multiple,
            Mode = multiple ? DropDownMode.Multiple : DropDownMode.Single,
            Items = Enumerable.Range(1, 3).Select(i => new DropDownMenuItem<string>
                { Title = $"Item{i}", Item = $"Item{i}", CallbackData = $"item{i}" }).ToArray()
        }
    };
    public static IBotMessage Message(DropDownSessionObject data)
    {
        var menu = data.DropDown;
        var selected = menu.IsAllSelected() ? "All" : string.Join(", ", menu.GetSelected().Select(i => i.Title));
        var builder = new InlineMarkupQueryBuilder().AddCommand<ShowDropDownCallback>(
            $"{menu.GetDropDownPrefix()} {menu.Title}: {selected}", menu.CallbackData);
        if (menu.Show)
            foreach (var item in menu.Items)
                builder.AddCommand<SelectDropDownItemCallback>((item.Selected ? "✓ " : "") + item.Title, item.CallbackData);
        builder.AddCommand<CancelCallback>("Cancel");
        return SampleMessages.WithKeyboard("Ok, your dropdown", builder);
    }
}
