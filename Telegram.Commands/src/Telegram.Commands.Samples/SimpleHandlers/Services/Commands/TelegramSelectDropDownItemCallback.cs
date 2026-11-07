using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "selectdropdownitem_callback", Authorized = true, Swarms = new[] { "telegram" })]
public sealed class TelegramSelectDropDownItemCallback(ISampleMessageSender sender)
    : SelectDropDownItemCallback(sender), ISessionCommand<global::Telegram.Bot.Types.CallbackQuery, DropDownSessionObject>
{
    Task<ICommandExecutionResult> ISessionCommand<global::Telegram.Bot.Types.CallbackQuery, DropDownSessionObject>.Execute(QueryContext<global::Telegram.Bot.Types.CallbackQuery> query, DropDownSessionObject data)
        => Execute(query, data);
}
