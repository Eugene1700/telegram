using System.Threading.Tasks;
using SimpleHandlers.Services.Commands.Models;
using global::Telegram.Bot.Types;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Exceptions;

namespace SimpleHandlers.Services.Commands
{
    [Command(Swarms = new[] { "telegram" }, Name = "dropdown_behavior")]
    public class DropDownBehavior : IBehaviorCommand<DropDownSessionObject>
    {
        public Task<ICommandExecutionResult> DefaultExecute<TQuery>(QueryContext<TQuery> query, DropDownSessionObject sessionObject)
        {
            throw new BotDomainException("From menu, please");
        }

        public async Task<ICommandExecutionResult> Execute<TQuery>(IQueryCommand<TQuery> currentCommand, QueryContext<TQuery> query, DropDownSessionObject sessionObject)
        {
            return currentCommand switch
            {
                CancelCallback cancelCallback => await cancelCallback.Execute(query as QueryContext<CallbackQuery>),
                _ => await DefaultExecute(query, sessionObject)
            };
        }

        public async Task<ICommandExecutionResult> Execute<TQuery>(ISessionCommand<TQuery, DropDownSessionObject> currentCommand, QueryContext<TQuery> query, DropDownSessionObject sessionObject)
        {
            return currentCommand switch
            {
                SelectDropDownItemCallback selectDropDownItemCallback => await selectDropDownItemCallback.Execute(
                    query as QueryContext<CallbackQuery>, sessionObject),
                ShowDropDownCallback showDropDownCallback => await showDropDownCallback.Execute(query as QueryContext<CallbackQuery>,
                    sessionObject),
                _ => await DefaultExecute(query, sessionObject)
            };
        }
    }
}