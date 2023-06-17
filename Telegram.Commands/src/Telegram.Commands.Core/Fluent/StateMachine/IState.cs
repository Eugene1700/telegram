using System.Threading.Tasks;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;

namespace Telegram.Commands.Core.Fluent.StateMachine
{
    internal interface IState<in TObj, TStates> : IStateBase<TStates>
    {
        Task SendMessages<TQuery>(TQuery currentQuery, TObj obj);
        Task<(TStates, bool)> HandleQuery<TQuery>(TQuery query, TObj obj);
        Task<bool> IsCommandHandle<TQuery>(TObj obj, IQueryTelegramCommand<TQuery> currentCommand);
        StateType GetStateType();
        Task<ITelegramCommandExecutionResult> Finalize<TQuery>(TQuery query, TObj sessionObjectObject);
        bool NeedAnswer { get;}
    }
}