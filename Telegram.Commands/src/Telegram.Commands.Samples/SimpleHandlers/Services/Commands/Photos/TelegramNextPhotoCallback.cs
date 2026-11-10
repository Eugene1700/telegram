using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "nextphoto_callback", Authorized = true, Platform = "telegram")]
public sealed class TelegramNextPhotoCallback(ISampleMessageSender sender)
    : NextPhotoCallback(sender), ISessionCommand<global::Telegram.Bot.Types.CallbackQuery, List<PhotoSessionObject>>
{
    Task<ICommandExecutionResult> ISessionCommand<global::Telegram.Bot.Types.CallbackQuery, List<PhotoSessionObject>>.Execute(IQueryContext<global::Telegram.Bot.Types.CallbackQuery> query, List<PhotoSessionObject> data)
        => Execute(query, data);
}
