using System;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;
using Bot.Commands.Core.Models;

namespace Bot.Commands.Core.Fluent
{

    public class InterceptResult<TQuery, TObject>
    {
        public bool MustIntercept { get; set; }
        public bool MustInterceptWithoutExecute { get; set; }
        public bool IsTerminalResult { get; set; }
        public Func<IQueryContext<TQuery>, TObject, ICommandExecutionResult, Task> ResultCallback { get; set; }
    }
}
