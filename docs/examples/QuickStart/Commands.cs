using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Fluent;
using Bot.Commands.Core.Fluent.Builders.Extensions;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;
using Bot.Commands.Core.Models;
using Tg = global::Telegram.Bot.Types;
using MaxTypes = global::Max.Bot.Types;

namespace QuickStart;

[Command(Name = "hello", Authorized = false)]
public sealed class HelloCommand : IQueryCommand<Tg.Message>, IQueryCommand<MaxTypes.Message>
{
    public Task<ICommandExecutionResult> Execute(QueryContext<Tg.Message> query) => Reply();
    public Task<ICommandExecutionResult> Execute(QueryContext<MaxTypes.Message> query) => Reply();

    private static Task<ICommandExecutionResult> Reply()
    {
        Console.WriteLine("bot: Hello! Try /ask or /profile.");
        return Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.Break());
    }
}

public sealed class NameData
{
    public string Prompt { get; set; } = "What should I call you?";
}

[Command(Name = "ask", Authorized = false)]
public sealed class AskCommand : IQueryCommand<Tg.Message>, IQueryCommand<MaxTypes.Message>
{
    public Task<ICommandExecutionResult> Execute(QueryContext<Tg.Message> query)
    {
        var data = Prompt();
        return Task.FromResult<ICommandExecutionResult>(
            CommandExecutionResult.Ahead<NameReply, Tg.Message, NameData>(data));
    }

    public Task<ICommandExecutionResult> Execute(QueryContext<MaxTypes.Message> query)
    {
        var data = Prompt();
        return Task.FromResult<ICommandExecutionResult>(
            CommandExecutionResult.Ahead<NameReply, MaxTypes.Message, NameData>(data));
    }

    private static NameData Prompt()
    {
        var data = new NameData();
        Console.WriteLine($"bot: {data.Prompt}");
        return data;
    }
}

[Command(Name = "name_reply", Authorized = false)]
public sealed class NameReply : ISessionCommand<Tg.Message, NameData>, ISessionCommand<MaxTypes.Message, NameData>
{
    public Task<ICommandExecutionResult> Execute(QueryContext<Tg.Message> query, NameData data)
        => Reply(query.GetData(), data);
    public Task<ICommandExecutionResult> Execute(QueryContext<MaxTypes.Message> query, NameData data)
        => Reply(query.GetData(), data);

    private static Task<ICommandExecutionResult> Reply(string text, NameData data)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            Console.WriteLine($"bot: Please send text. {data.Prompt}");
            return Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.Freeze());
        }
        Console.WriteLine($"bot: Nice to meet you, {text}.");
        return Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.Break());
    }
}

public enum ProfileStep { Name, Done }
public sealed class ProfileData
{
    public string Name { get; set; }
}

[Command(Name = "profile", Authorized = false)]
public sealed class ProfileCommand : FluentCommand<ProfileData, ProfileStep>
{
    protected override Task<(ProfileData, ProfileStep)> Entry<TQuery>(QueryContext<TQuery> query, ProfileData currentObject)
        => Task.FromResult((currentObject ?? new ProfileData(), ProfileStep.Name));

    protected override IStateMachine<ProfileStep> StateMachine(IStateMachineBuilder<ProfileData, ProfileStep> builder)
    {
        return builder
            .State(ProfileStep.Name)
            .WithMessage("What is your name?", Send)
            .Next<object>((query, state, data) =>
            {
                data.Name = ReadText(query);
                return Task.FromResult(string.IsNullOrWhiteSpace(data.Name) ? ProfileStep.Name : ProfileStep.Done);
            }, force: true)
            .Exit<object>(ProfileStep.Done, Finish)
            .Build();
    }

    private static Task Send(IQueryContext<object> context, ProfileStep state, ProfileData data, IBotMessage message)
    {
        // An untyped sender receives QueryContext<T>, not the native SDK object.
        // A real sender can select a messenger client by matching this context.
        Console.WriteLine($"bot: {message.Text}");
        return Task.CompletedTask;
    }

    private static string ReadText(IQueryContext<object> query) => query.GetData();

    private static Task<ICommandExecutionResult> Finish(IQueryContext<object> query, ProfileStep state, ProfileData data)
    {
        Console.WriteLine($"bot: Profile saved for {data.Name}.");
        return Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.Break());
    }
}
