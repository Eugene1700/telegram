using System;
using System.Threading.Tasks;

namespace Bot.Commands.Abstract.Interfaces
{

    /// <summary>Runs before command execution. Throw to reject execution.</summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public abstract class ExecuteFilterAttribute : Attribute
    {
        /// <summary>Lower values run first within a command's filters.</summary>
        public int Order { get; set; }

        public abstract Task Filter<TQuery>(ICommandExecutionContext<TQuery> context);
    }
}
