using Bot.Commands.Abstract.Interfaces;
namespace SurveyComparison;
public sealed class DemoAuth : IAuthProvider
{
    public Task<bool> AuthUser<T>(long userId, ICommandExecutionContext<T> context) => Task.FromResult(true);
}
