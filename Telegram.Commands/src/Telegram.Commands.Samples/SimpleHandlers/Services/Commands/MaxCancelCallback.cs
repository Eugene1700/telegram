using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "cancel_callback", Authorized = true, Swarms = new[] { "max" })]
public sealed class MaxCancelCallback(ISampleMessageSender sender)
    : CancelCallback(sender), IQueryCommand<global::Max.Bot.Types.CallbackQuery>
{
    Task<ICommandExecutionResult> IQueryCommand<global::Max.Bot.Types.CallbackQuery>.Execute(QueryContext<global::Max.Bot.Types.CallbackQuery> query)
        => Execute(query);
}
