using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "start", Authorized = false, Platform = "max")]
public sealed class MaxStartCommand(ISampleMessageSender sender) : StartCommand(sender), IQueryCommand<global::Max.Bot.Types.Message>
{
    Task<ICommandExecutionResult> IQueryCommand<global::Max.Bot.Types.Message>.Execute(IQueryContext<global::Max.Bot.Types.Message> query)
        => Execute(query);
}
