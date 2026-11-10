using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core;
namespace SimpleHandlers.Services.Commands;
// Only metadata is persisted. The resolver selects the concrete command for the current platform and any configured swarms.
internal sealed class SampleCommandResult : ICommandExecutionResult
{
    public object Data { get; init; }
    public ICommandDescriptor NextCommandDescriptor { get; init; }
    public ExecuteResult Result => ExecuteResult.Ahead;
    public long? WaitFromChatId => null;
    public uint? SessionDurationInSec { get; init; }
    public static ICommandExecutionResult Next<TCommand>(object data, uint? duration = 600) => new SampleCommandResult
    {
        Data = data, NextCommandDescriptor = BotCommandExtensions.GetCommandInfo(typeof(TCommand)),
        SessionDurationInSec = duration
    };
}
