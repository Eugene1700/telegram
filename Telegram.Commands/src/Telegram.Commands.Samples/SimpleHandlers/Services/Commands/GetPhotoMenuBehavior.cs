using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleHandlers.Services.Commands.Models;
using global::Telegram.Bot.Types;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Exceptions;

namespace SimpleHandlers.Services.Commands
{
    [Command(Swarms = new[] { "telegram" }, Name = "getphotomenu_behavior", Authorized = true)]
    public class GetPhotoMenuBehavior : IBehaviorCommand<List<PhotoSessionObject>>
    {
        public Task<ICommandExecutionResult> DefaultExecute<TQuery>(QueryContext<TQuery> query, List<PhotoSessionObject> sessionObject)
        {
            throw new BotDomainException("You should do action from menu");
        }

        public async Task<ICommandExecutionResult> Execute<TQuery>(IQueryCommand<TQuery> currentCommand,
            QueryContext<TQuery> query, List<PhotoSessionObject> sessionObject)
        {
            if (!(query is QueryContext<CallbackQuery> callbackQuery))
                throw new BotDomainException("You should do action from menu");
            if (currentCommand is CancelCallback cancelCallback)
            {
                return await cancelCallback.Execute(callbackQuery);
            }

            throw new BotDomainException("You should do action from menu");

        }

        public async Task<ICommandExecutionResult> Execute<TQuery>(ISessionCommand<TQuery,
            List<PhotoSessionObject>> currentCommand, QueryContext<TQuery> query, List<PhotoSessionObject> sessionObject)
        {
            if (!(query is QueryContext<CallbackQuery> callbackQuery))
                throw new BotDomainException("You should do action from menu");
            return currentCommand switch
            {
                ShowResultCallback showResultCallback => await showResultCallback.Execute(callbackQuery, sessionObject),
                NextPhotoCallback nextPhotoCallback => await nextPhotoCallback.Execute(callbackQuery, sessionObject),
                _ => throw new BotDomainException("You should do action from menu")
            };
        }
    }
}