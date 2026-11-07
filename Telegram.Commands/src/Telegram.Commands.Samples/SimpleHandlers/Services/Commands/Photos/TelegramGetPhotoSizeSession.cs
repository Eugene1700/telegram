using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "getphotosize_session", Authorized = true, Swarms = new[] { "telegram" })]
public sealed class TelegramGetPhotoSizeSession(ISampleMessageSender sender)
    : GetPhotoSizeSession(sender), ISessionCommand<global::Telegram.Bot.Types.Message, List<PhotoSessionObject>>
{
    Task<ICommandExecutionResult> ISessionCommand<global::Telegram.Bot.Types.Message, List<PhotoSessionObject>>.Execute(
        IQueryContext<global::Telegram.Bot.Types.Message> query, List<PhotoSessionObject> data)
        => Execute(query, data);
}
