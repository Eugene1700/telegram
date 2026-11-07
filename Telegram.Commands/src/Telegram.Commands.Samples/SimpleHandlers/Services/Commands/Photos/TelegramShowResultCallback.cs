using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "showresult_callback", Authorized = true, Swarms = new[] { "telegram" })]
public sealed class TelegramShowResultCallback(ISampleMessageSender sender)
    : ShowResultCallback(sender), ISessionCommand<global::Telegram.Bot.Types.CallbackQuery, List<PhotoSessionObject>>
{
    Task<ICommandExecutionResult> ISessionCommand<global::Telegram.Bot.Types.CallbackQuery, List<PhotoSessionObject>>.Execute(IQueryContext<global::Telegram.Bot.Types.CallbackQuery> query, List<PhotoSessionObject> data)
        => Execute(query, data);
}
