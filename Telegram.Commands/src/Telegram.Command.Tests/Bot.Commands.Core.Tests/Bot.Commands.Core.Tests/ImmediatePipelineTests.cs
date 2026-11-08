using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Fluent;
using Bot.Commands.Core.Fluent.Builders.Extensions;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Services;
using Bot.Commands.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Bot.Commands.Core.Tests;

public class ImmediatePipelineTests
{
    public sealed class Trace { public List<string> Entries { get; } = new(); }

    [Command(Name = "immediate_parent", Authorized = false, Area = ChatArea.Group, Swarms = new[] { "immediate-tests" })]
    public sealed class Parent : FluentCommand<string, int>
    {
        protected override Task<(string, int)> Entry<T>(IQueryContext<T> q, string data)
            => Task.FromResult(("saved child data", 0));
        protected override IStateMachine<int> StateMachine(IStateMachineBuilder<string, int> b)
            => b.Exit<object>(0, (_, _, d) => Task.FromResult<ICommandExecutionResult>(
                CommandExecutionResult.AheadFluent<Child, string, int>(d, true))).Build();
    }

    [Command(Name = "immediate_child", Authorized = false, Area = ChatArea.Group, Swarms = new[] { "immediate-tests" })]
    public sealed class Child(Trace trace) : FluentCommand<string, int>
    {
        protected override Task<(string, int)> Entry<T>(IQueryContext<T> q, string data)
        {
            trace.Entries.Add(data);
            return Task.FromResult((data, 0));
        }
        protected override IStateMachine<int> StateMachine(IStateMachineBuilder<string, int> b)
            => b.State(0).WithMessage("Child prompt", (_, _, _, _) => Task.CompletedTask).Loop(true).Build();
    }

    [Command(Name = "immediate_loop", Authorized = false, Area = ChatArea.Group, Swarms = new[] { "immediate-tests" })]
    public sealed class Loop : FluentCommand<string, int>
    {
        protected override Task<(string, int)> Entry<T>(IQueryContext<T> q, string data)
            => Task.FromResult(("loop", 0));
        protected override IStateMachine<int> StateMachine(IStateMachineBuilder<string, int> b)
            => b.Exit<object>(0, (_, _, d) => Task.FromResult<ICommandExecutionResult>(
                CommandExecutionResult.AheadFluent<Loop, string, int>(d, true))).Build();
    }

    private static ServiceProvider Provider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Trace>();
        services.AddSingleton(Mock.Of<IClock>());
        services.AddSingleton(Mock.Of<IAuthProvider>());
        services.AddSingleton(Mock.Of<IBotProfile>(p => p.BotName == "immediate" && p.Swarms == new[] { "immediate-tests" }));
        services.UseInMemoryStorage();
        services.UseBotCommandsServices();
        services.AddScoped<ExecuteFilterTests.Resolver>();
        return services.BuildServiceProvider();
    }

    [Test]
    public async Task FireFromBehaviorCommandExecutesDestinationAndPreservesItsSession()
    {
        using var provider = Provider();
        using (var scope = provider.CreateScope())
            await scope.ServiceProvider.GetRequiredService<ExecuteFilterTests.Resolver>()
                .Handle(new CoreBoundaryTests.CustomContext("/immediate_parent"));
        Assert.That(provider.GetRequiredService<Trace>().Entries, Is.EqualTo(new[] { "saved child data" }));
        using var next = provider.CreateScope();
        var saved = next.ServiceProvider.GetRequiredService<ISessionsStore>()
            .GetSessionInfoWithData<FluentObject<string, int>>(default, 10, 20, "/immediate_child");
        Assert.That(saved.Data.Object, Is.EqualTo("saved child data"));
        Assert.That(saved.Data.CurrentStateId.IsInit, Is.True);
        await next.ServiceProvider.GetRequiredService<ExecuteFilterTests.Resolver>()
            .Handle(new CoreBoundaryTests.CustomContext("answer"));
        Assert.That(provider.GetRequiredService<Trace>().Entries.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task ImmediateLoopIsBoundedAndUpdateTransactionRollsBack()
    {
        using var provider = Provider();
        using var scope = provider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<InMemorySessionsStore>();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => store.ExecuteUpdateAsync("loop", "loop", 10, 20,
            _ => scope.ServiceProvider.GetRequiredService<ExecuteFilterTests.Resolver>()
                .Handle(new CoreBoundaryTests.CustomContext("/immediate_loop"))));
        Assert.That(error.Message, Does.Contain("Too many immediate"));
        Assert.That(store.GetSessionInfo(default, 10, 20), Is.Null);

    }
}
