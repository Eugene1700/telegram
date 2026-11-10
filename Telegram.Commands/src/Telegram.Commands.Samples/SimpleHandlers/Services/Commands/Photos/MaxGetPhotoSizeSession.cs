using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "getphotosize_session", Authorized = true, Platform = "max")]
public sealed class MaxGetPhotoSizeSession(ISampleMessageSender sender)
    : GetPhotoSizeSession(sender), ISessionCommand<global::Max.Bot.Types.Message, List<PhotoSessionObject>>
{
    Task<ICommandExecutionResult> ISessionCommand<global::Max.Bot.Types.Message, List<PhotoSessionObject>>.Execute(
        IQueryContext<global::Max.Bot.Types.Message> query, List<PhotoSessionObject> data)
        => Execute(query, data);
}
