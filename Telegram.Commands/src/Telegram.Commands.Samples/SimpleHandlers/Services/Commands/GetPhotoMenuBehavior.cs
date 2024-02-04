using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleHandlers.Services.Commands.Models;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Attributes;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Exceptions;

namespace SimpleHandlers.Services.Commands
{
    [Command(Name = "getphotomenu_behavior", Authorized = true)]
    public class GetPhotoMenuBehavior : IBehaviorTelegramCommand<List<PhotoSessionObject>>
    {
        public Task<ITelegramCommandExecutionResult> DefaultExecute<TQuery>(TQuery query, List<PhotoSessionObject> sessionObject)
        {
            throw new TelegramDomainException("You should do action from menu");
        }

        public async Task<ITelegramCommandExecutionResult> Execute<TQuery>(IQueryTelegramCommand<TQuery> currentCommand, 
            TQuery query, List<PhotoSessionObject> sessionObject)
        {
            if (!(query is CallbackQuery callbackQuery))
                throw new TelegramDomainException("You should do action from menu");
            if (currentCommand is CancelCallback cancelCallback)
            {
                return await cancelCallback.Execute(callbackQuery);
            }

            throw new TelegramDomainException("You should do action from menu");

        }

        public async Task<ITelegramCommandExecutionResult> Execute<TQuery>(ISessionTelegramCommand<TQuery, 
            List<PhotoSessionObject>> currentCommand, TQuery query, List<PhotoSessionObject> sessionObject)
        {
            if (!(query is CallbackQuery callbackQuery))
                throw new TelegramDomainException("You should do action from menu");
            return currentCommand switch
            {
                ShowResultCallback showResultCallback => await showResultCallback.Execute(callbackQuery, sessionObject),
                NextPhotoCallback nextPhotoCallback => await nextPhotoCallback.Execute(callbackQuery, sessionObject),
                _ => throw new TelegramDomainException("You should do action from menu")
            };
        }
    }
}