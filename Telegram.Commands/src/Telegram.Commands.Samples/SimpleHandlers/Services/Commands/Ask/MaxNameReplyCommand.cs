using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "name_reply", Authorized = false, Swarms = new[] { "max" })]
public sealed class MaxNameReplyCommand(ISampleMessageSender sender) : NameReplyCommand(sender), ISessionCommand<global::Max.Bot.Types.Message, NameReplyData>
{
    Task<ICommandExecutionResult> ISessionCommand<global::Max.Bot.Types.Message, NameReplyData>.Execute(IQueryContext<global::Max.Bot.Types.Message> query, NameReplyData data)
        => Execute(query, data);
}
