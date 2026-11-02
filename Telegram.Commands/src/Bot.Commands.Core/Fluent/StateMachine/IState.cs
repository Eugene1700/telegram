using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Models;

namespace Bot.Commands.Core.Fluent.StateMachine
{
    internal interface IState<TObj, TStates> : IStateBase<TStates>
    {
        Task SendMessages<TQuery>(QueryContext<TQuery> currentQuery, TObj obj);
        Task<(TStates, bool)> HandleQuery<TQuery>(QueryContext<TQuery> query, TObj obj);
        Task<bool> IsCommandHandle<TQuery>(TObj obj, IQueryCommand<TQuery> currentCommand);
        StateType GetStateType();
        Task<ICommandExecutionResult> Finalize<TQuery>(QueryContext<TQuery> query, TObj sessionObjectObject);
        bool NeedAnswer { get;}
    }
}