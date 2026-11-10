using System.Collections.Generic;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using SimpleHandlers.Services.Commands.Models;
using Bot.Commands.Abstract.Attributes;
namespace SimpleHandlers.Services.Commands;

[Command(Name = "otherfluent", Authorized = true, Platform = "telegram")]
public sealed class TelegramMyFluentInitCommand(ISampleMessageSender sender) : MyFluentInitCommand<global::Telegram.Bot.Types.Message, global::Telegram.Bot.Types.CallbackQuery>(sender), IQueryCommand<global::Telegram.Bot.Types.Message>
{
    Task<ICommandExecutionResult> IQueryCommand<global::Telegram.Bot.Types.Message>.Execute(IQueryContext<global::Telegram.Bot.Types.Message> query)
        => Execute(query);
}
