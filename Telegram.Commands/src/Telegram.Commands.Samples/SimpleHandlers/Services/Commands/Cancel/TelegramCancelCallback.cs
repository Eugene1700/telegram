using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "cancel_callback", Authorized = true, Platform = "telegram")]
public sealed class TelegramCancelCallback(ISampleMessageSender sender)
    : CancelCallback(sender), IQueryCommand<global::Telegram.Bot.Types.CallbackQuery>
{
    Task<ICommandExecutionResult> IQueryCommand<global::Telegram.Bot.Types.CallbackQuery>.Execute(IQueryContext<global::Telegram.Bot.Types.CallbackQuery> query)
        => Execute(query);
}
