using System.Threading.Tasks;
using SimpleHandlers.Services.Commands.Models;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Exceptions;

namespace SimpleHandlers.Services.Commands
{
    [Command(Name = "dropdown_behavior")]
    public class DropDownBehavior : IBehaviorTelegramCommand<DropDownSessionObject>
    {
        public Task<ITelegramCommandExecutionResult> DefaultExecute<TQuery>(TQuery query, DropDownSessionObject sessionObject)
        {
            throw new TelegramDomainException("From menu, please");
        }

        public Task<ITelegramCommandExecutionResult> Execute<TQuery>(IQueryTelegramCommand<TQuery> currentCommand, TQuery query, DropDownSessionObject sessionObject)
        {
            return DefaultExecute(query, sessionObject);
        }

        public async Task<ITelegramCommandExecutionResult> Execute<TQuery>(ISessionTelegramCommand<TQuery, DropDownSessionObject> currentCommand, TQuery query, DropDownSessionObject sessionObject)
        {
            return currentCommand switch
            {
                SelectDropDownItemCallback selectDropDownItemCallback => await selectDropDownItemCallback.Execute(
                    query as CallbackQuery, sessionObject),
                ShowDropDownCallback showDropDownCallback => await showDropDownCallback.Execute(query as CallbackQuery,
                    sessionObject),
                _ => await DefaultExecute(query, sessionObject)
            };
        }
    }
}