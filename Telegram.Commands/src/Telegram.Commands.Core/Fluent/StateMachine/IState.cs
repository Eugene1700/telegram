using System.Threading.Tasks;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Models;

namespace Telegram.Commands.Core.Fluent.StateMachine
{
    internal interface IState<TObj, TStates> : IStateBase<TStates>
    {
        Task SendMessages<TQuery>(QueryContext<TQuery> currentQuery, TObj obj);
        Task<(TStates, bool)> HandleQuery<TQuery>(QueryContext<TQuery> query, TObj obj);
        Task<bool> IsCommandHandle<TQuery>(TObj obj, IQueryTelegramCommand<TQuery> currentCommand);
        StateType GetStateType();
        Task<ITelegramCommandExecutionResult> Finalize<TQuery>(QueryContext<TQuery> query, TObj sessionObjectObject);
        bool NeedAnswer { get;}
    }
}