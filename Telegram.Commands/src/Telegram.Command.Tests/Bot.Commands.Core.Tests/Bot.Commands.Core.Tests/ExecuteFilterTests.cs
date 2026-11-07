using System.Text.Json;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Exceptions;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Services;
using Bot.Commands.Core.Tests.Mocks;
using Bot.Commands.DependencyInjection;
using Bot.Commands.Telegram.Services;
using Bot.Commands.Max.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Tg = global::Telegram.Bot.Types;
using Mx = global::Max.Bot.Types;

namespace Bot.Commands.Core.Tests;

public class ExecuteFilterTests
{
    private static readonly AsyncLocal<List<(string Label, Type Command, object Query, long Chat)>> Audit = new();

    private static ServiceProvider Provider()
    {
        Audit.Value = new();
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IClock>());
        services.AddSingleton(Mock.Of<IAuthProvider>());
        services.AddSingleton(Mock.Of<IBotProfile>(p => p.Swarms == new[] { "filter-tests" }));
        services.AddSingleton<SessionStoreMock>();
        services.AddSingleton<ISessionsStore>(sp => sp.GetRequiredService<SessionStoreMock>());
        services.UseBotCommandsServices();
        services.AddScoped<Resolver>();
        services.AddScoped<TelegramCommandService>();
        services.AddScoped<MaxCommandService>();
        return services.BuildServiceProvider();
    }

    [Test]
    public async Task InheritedFiltersAreAwaitedInOrderWithConcreteCommandContext()
    {
        using var provider = Provider();
        using var scope = provider.CreateScope();
        var query = new CoreBoundaryTests.CustomContext("/filter_ordered");
        await scope.ServiceProvider.GetRequiredService<Resolver>().Handle(query);
        Assert.That(Audit.Value.Select(x => x.Label), Is.EqualTo(new[] { "base", "first", "last", "execute" }));
        foreach (var entry in Audit.Value.Take(3))
        {
            Assert.That(entry.Command, Is.EqualTo(typeof(Ordered)));
            Assert.That(entry.Query, Is.SameAs(query.Query));
            Assert.That(entry.Chat, Is.EqualTo(10));
        }
    }

    [Test]
    public async Task RejectionStopsRemainingFiltersExecutionAndSessionMutation()
    {
        using var provider = Provider();
        using var scope = provider.CreateScope();
        var store = provider.GetRequiredService<SessionStoreMock>();
        var changed = false;
        store.SetCreateSessionCallback(_ => changed = true);
        store.SetUpdateSessionCallback((_, _) => changed = true);
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            scope.ServiceProvider.GetRequiredService<Resolver>().Handle(new CoreBoundaryTests.CustomContext("/filter_denied")));
        Assert.That(error.Message, Is.EqualTo("rejected"));
        Assert.That(Audit.Value.Select(x => x.Label), Is.EqualTo(new[] { "base", "reject" }));
        Assert.That(changed, Is.False);
    }

    [TestCase("/filter_auth", true)]
    [TestCase("/filter_private", false)]
    public async Task AuthorizationAndChatAreaAreCheckedBeforeFilters(string command, bool authorization)
    {
        using var provider = Provider();
        using var scope = provider.CreateScope();
        var action = () => scope.ServiceProvider.GetRequiredService<Resolver>().Handle(new CoreBoundaryTests.CustomContext(command));
        if (authorization) await Assert.ThrowsAsync<CommandPermissionException>(async () => await action());
        else await Assert.ThrowsAsync<CommandChatAreaException>(async () => await action());
        Assert.That(Audit.Value, Is.Empty);
    }

    [Test]
    public async Task ActiveSessionRunsItsFilterBeforeSessionHandler()
    {
        using var provider = Provider();
        var saved = new CommandSession { ChatId = 10, UserId = 20, CommandQuery = "/filter_session", Data = "saved" };
        var store = provider.GetRequiredService<SessionStoreMock>();
        store.SetSessionInfo(saved);
        store.SetSessionInfoWithData(saved);
        using var scope = provider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<Resolver>().Handle(new CoreBoundaryTests.CustomContext("answer"));
        Assert.That(Audit.Value.Select(x => x.Label), Is.EqualTo(new[] { "session", "session-execute" }));
        Assert.That(Audit.Value[0].Command, Is.EqualTo(typeof(Session)));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task BothAdaptersApplyTheCommonFilterPipeline(bool max)
    {
        using var provider = Provider();
        using var scope = provider.CreateScope();
        if (max)
        {
            var update = JsonSerializer.Deserialize<Mx.Update>(
                """{"update_type":"message_created","message":{"sender":{"user_id":20,"name":"test"},"recipient":{"chat_id":10,"chat_type":"dialog"},"body":{"mid":"mid","text":"/filter_ordered"}}}""");
            await scope.ServiceProvider.GetRequiredService<MaxCommandService>().Handle(update);
            Assert.That(Audit.Value[0].Query, Is.SameAs(update.Message));
        }
        else
        {
            var update = new Tg.Update { Message = new Tg.Message { Text = "/filter_ordered",
                From = new Tg.User { Id = 20 }, Chat = new Tg.Chat { Id = 10, Type = global::Telegram.Bot.Types.Enums.ChatType.Private } } };
            await scope.ServiceProvider.GetRequiredService<TelegramCommandService>().Handle(update);
            Assert.That(Audit.Value[0].Query, Is.SameAs(update.Message));
        }
        Assert.That(Audit.Value.Select(x => x.Label), Is.EqualTo(new[] { "base", "first", "last", "execute" }));
    }

    public sealed class Resolver(CommandExecutionService execution, SessionManager sessions, IAuthProvider auth, IBotProfile profile)
        : CommandServiceBase(execution, sessions, auth, profile)
    {
        public Task Handle(IQueryContext<string> context) => QueryHandler(context);
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task BehaviorAndSelectedCommandFiltersRunBeforeAnyHandler(bool reject)
    {
        using var provider = Provider();
        var saved = new CommandSession { ChatId = 10, UserId = 20,
            CommandQuery = reject ? "/filter_behavior_denied" : "/filter_behavior", Data = "saved" };
        var store = provider.GetRequiredService<SessionStoreMock>();
        store.SetSessionInfo(saved);
        store.SetSessionInfoWithData(saved);
        var changed = false;
        store.SetUpdateSessionCallback((_, _) => changed = true);
        using var scope = provider.CreateScope();
        Task Run() => scope.ServiceProvider.GetRequiredService<Resolver>()
            .Handle(new CoreBoundaryTests.CustomContext("/filter_ordered"));
        if (reject)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(Run);
            Assert.That(Audit.Value.Select(x => x.Label), Is.EqualTo(new[] { "reject" }));
        }
        else
        {
            await Run();
            Assert.That(Audit.Value.Select(x => x.Label),
                Is.EqualTo(new[] { "behavior", "base", "first", "last", "behavior-execute", "execute" }));
            Assert.That(Audit.Value[0].Command, Is.EqualTo(typeof(Behavior)));
            Assert.That(Audit.Value[1].Command, Is.EqualTo(typeof(Ordered)));
        }
        Assert.That(changed, Is.False);
    }

    [Command(Name = "filter_behavior", Authorized = false, Area = ChatArea.Group, Swarms = new[] { "filter-tests" })]
    [Record("behavior")]
    public class Behavior : IBehaviorCommand<string>
    {
        public Task<ICommandExecutionResult> DefaultExecute<TQuery>(IQueryContext<TQuery> query, string data)
            => Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.Freeze());
        public Task<ICommandExecutionResult> Execute<TQuery>(IQueryCommand<TQuery> command, IQueryContext<TQuery> query, string data)
        {
            Audit.Value.Add(("behavior-execute", GetType(), query.Query, query.GetChatId()));
            return command.Execute(query);
        }
        public Task<ICommandExecutionResult> Execute<TQuery>(ISessionCommand<TQuery, string> command, IQueryContext<TQuery> query, string data)
            => command.Execute(query, data);
    }

    [Command(Name = "filter_behavior_denied", Authorized = false, Area = ChatArea.Group, Swarms = new[] { "filter-tests" })]
    [Record("reject", Order = -10)]
    public sealed class DeniedBehavior : Behavior { }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class RecordAttribute(string label) : ExecuteFilterAttribute
    {
        public override async Task Filter<TQuery>(ICommandExecutionContext<TQuery> context)
        {
            await Task.Yield();
            Audit.Value.Add((label, context.CommandType, context.Query, context.ChatId));
            Assert.That(context.CommandDescriptor, Is.Not.Null);
            if (label == "reject") throw new InvalidOperationException("rejected");
        }
    }

    [Record("base", Order = -10)]
    public abstract class QueryCommand : IQueryCommand<string>, IQueryCommand<Tg.Message>, IQueryCommand<Mx.Message>
    {
        public Task<ICommandExecutionResult> Execute(IQueryContext<string> query) => Run();
        public Task<ICommandExecutionResult> Execute(IQueryContext<Tg.Message> query) => Run();
        public Task<ICommandExecutionResult> Execute(IQueryContext<Mx.Message> query) => Run();
        private Task<ICommandExecutionResult> Run()
        {
            Audit.Value.Add(("execute", GetType(), null, 0));
            return Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.Freeze());
        }
    }

    [Command(Name = "filter_ordered", Authorized = false, Area = ChatArea.Private | ChatArea.Group, Swarms = new[] { "filter-tests" })]
    [Record("last", Order = 20), Record("first", Order = 10)]
    public sealed class Ordered : QueryCommand { }

    [Command(Name = "filter_denied", Authorized = false, Area = ChatArea.Group, Swarms = new[] { "filter-tests" })]
    [Record("reject", Order = 0), Record("never", Order = 10)]
    public sealed class Denied : QueryCommand { }

    [Command(Name = "filter_auth", Authorized = true, Area = ChatArea.Group, Swarms = new[] { "filter-tests" })]
    public sealed class Authorized : QueryCommand { }

    [Command(Name = "filter_private", Authorized = false, Area = ChatArea.Private, Swarms = new[] { "filter-tests" })]
    public sealed class Private : QueryCommand { }

    [Command(Name = "filter_session", Authorized = false, Area = ChatArea.Group, Swarms = new[] { "filter-tests" })]
    [Record("session")]
    public sealed class Session : ISessionCommand<string, string>
    {
        public Task<ICommandExecutionResult> Execute(IQueryContext<string> query, string sessionObject)
        {
            Assert.That(sessionObject, Is.EqualTo("saved"));
            Audit.Value.Add(("session-execute", GetType(), query.Query, query.GetChatId()));
            return Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.Freeze());
        }
    }
}
