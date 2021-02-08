using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Models
{
    public abstract class SessionCommandBase<TQuery, TData, TNextCommand, TNextCommandQuery> : ITelegramCommand<TQuery>
        where TNextCommand : ITelegramCommand<TNextCommandQuery>
    {
        private readonly SessionManager _sessionManager;

        protected SessionCommandBase(SessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public async Task Execute(TQuery query)
        {
            var data = await ConcreteExecute(query);
            var chain = GetChain();
            var chatId = query.GetChatId();
            var userId = query.GetFromId();
            switch (chain)
            {
                case CommandChain.StartPoint:
                    await _sessionManager.OpenSession<TNextCommand, TNextCommandQuery, TData>(chatId, userId, data);
                    return;
                case CommandChain.TransitPoint:
                    await _sessionManager.ContinueSession<TNextCommand, TNextCommandQuery, TData>(chatId, userId);
                    return;
                case CommandChain.EndPoint:
                    await _sessionManager.ReleaseSessionIfExists<TData>(chatId, userId);
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected abstract CommandChain GetChain();

        protected abstract Task<TData> ConcreteExecute(TQuery query);
    }
}