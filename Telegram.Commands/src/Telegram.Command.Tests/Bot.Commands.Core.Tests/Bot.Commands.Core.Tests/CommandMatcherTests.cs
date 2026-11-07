using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Exceptions;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Services;
using Bot.Commands.Core.Tests.Mocks;
using Bot.Commands.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Bot.Commands.Core.Tests;

public class CommandMatcherTests
{
    private static ServiceProvider Provider(params ICommandMatcher[] matchers)
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IClock>());
        services.AddSingleton(Mock.Of<IAuthProvider>());
        services.AddSingleton(Mock.Of<IBotProfile>(p => p.Swarms == new[] { "matcher-test" }));
        services.AddSingleton<SessionStoreMock>();
        services.AddSingleton<ISessionsStore>(sp => sp.GetRequiredService<SessionStoreMock>());
        foreach (var matcher in matchers) services.AddSingleton(matcher);
        services.UseBotCommandsServices();
        services.AddScoped<Resolver>();
        return services.BuildServiceProvider();
    }

    [Test]
    public async Task ExplicitCommandWinsAndMatcherReceivesOriginalContext()
    {
        var matcher = new Matcher(typeof(Funnel), _ => true);
        using var provider = Provider(matcher);
        using var scope = provider.CreateScope();
        var resolver = scope.ServiceProvider.GetRequiredService<Resolver>();
        await resolver.Handle(new CoreBoundaryTests.CustomContext("/matcher_exact"));
        Assert.That(scope.ServiceProvider.GetRequiredService<Exact>().Calls, Is.EqualTo(1));
        Assert.That(matcher.Calls, Is.Zero);
        var query = new CoreBoundaryTests.CustomContext("interested in your offer");
        await resolver.Handle(query);
        Assert.That(matcher.LastContext, Is.SameAs(query));
        Assert.That(scope.ServiceProvider.GetRequiredService<Funnel>().Calls, Is.EqualTo(1));
    }

    [Test]
    public async Task ActiveSessionKeepsInputAndUnknownSlashDoesNotTriggerMatcher()
    {
        var matcher = new Matcher(typeof(Funnel), _ => true);
        using var provider = Provider(matcher);
        using var scope = provider.CreateScope();
        var resolver = scope.ServiceProvider.GetRequiredService<Resolver>();
        await Assert.ThrowsAsync<CommandExtractionException>(() => resolver.Handle(new CoreBoundaryTests.CustomContext("/missing")));
        Assert.That(matcher.Calls, Is.Zero);
        var store = provider.GetRequiredService<SessionStoreMock>();
        var session = new CommandSession { ChatId = 10, UserId = 20, CommandQuery = "/matcher_session", Data = "stored" };
        store.SetSessionInfo(session);
        store.SetSessionInfoWithData(session);
        await resolver.Handle(new CoreBoundaryTests.CustomContext("interested"));
        Assert.That(scope.ServiceProvider.GetRequiredService<Session>().Calls, Is.EqualTo(1));
        Assert.That(matcher.Calls, Is.Zero);
    }

    [Test]
    public async Task MultipleMatchesAreRejectedAndWrongSwarmIsNotEvaluated()
    {
        var other = new Matcher(typeof(OtherSwarm), _ => true);
        using var provider = Provider(new Matcher(typeof(Exact), _ => true), new Matcher(typeof(Funnel), _ => true), other);
        using var scope = provider.CreateScope();
        await Assert.ThrowsAsync<CommandExtractionException>(() => scope.ServiceProvider.GetRequiredService<Resolver>()
            .Handle(new CoreBoundaryTests.CustomContext("offer")));
        Assert.That(other.Calls, Is.Zero);
        Assert.That(scope.ServiceProvider.GetRequiredService<Exact>().Calls, Is.Zero);
        Assert.That(scope.ServiceProvider.GetRequiredService<Funnel>().Calls, Is.Zero);
    }

    [Test]
    public async Task MatchedCommandStillRequiresAuthorization()
    {
        using var provider = Provider(new Matcher(typeof(Protected), _ => true));
        using var scope = provider.CreateScope();
        await Assert.ThrowsAsync<CommandPermissionException>(() => scope.ServiceProvider.GetRequiredService<Resolver>()
            .Handle(new CoreBoundaryTests.CustomContext("offer")));
    }

    public sealed class Resolver(CommandExecutionService execution, SessionManager sessions, IAuthProvider auth,
        IBotProfile profile, IEnumerable<ICommandMatcher> matchers) : CommandServiceBase(execution, sessions, auth, profile, matchers)
    {
        public Task Handle(CoreBoundaryTests.CustomContext context) => QueryHandler(context);
    }

    private sealed class Matcher(Type commandType, Func<string, bool> match) : ICommandMatcher
    {
        public Type CommandType => commandType;
        public int Calls { get; private set; }
        public object LastContext { get; private set; }
        public bool IsMatch<TQuery>(QueryContext<TQuery> query)
        {
            Calls++;
            LastContext = query;
            return match(query.GetData());
        }
    }

    public abstract class CountingCommand : IQueryCommand<string>
    {
        public int Calls { get; private set; }
        public Task<ICommandExecutionResult> Execute(QueryContext<string> query)
        {
            Calls++;
            return Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.Freeze());
        }
    }
    [Command(Name = "matcher_exact", Authorized = false, Area = ChatArea.Group, Swarms = new[] { "matcher-test" })]
    public sealed class Exact : CountingCommand { }
    [Command(Name = "matcher_funnel", Authorized = false, Area = ChatArea.Group, Swarms = new[] { "matcher-test" })]
    public sealed class Funnel : CountingCommand { }
    [Command(Name = "matcher_other", Authorized = false, Area = ChatArea.Group, Swarms = new[] { "other" })]
    public sealed class OtherSwarm : CountingCommand { }
    [Command(Name = "matcher_protected", Authorized = true, Area = ChatArea.Group)]
    public sealed class Protected : CountingCommand { }
    [Command(Name = "matcher_session", Authorized = false, Area = ChatArea.Group)]
    public sealed class Session : ISessionCommand<string, string>
    {
        public int Calls { get; private set; }
        public Task<ICommandExecutionResult> Execute(QueryContext<string> query, string data)
        {
            Calls++;
            return Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.Freeze());
        }
    }
}
