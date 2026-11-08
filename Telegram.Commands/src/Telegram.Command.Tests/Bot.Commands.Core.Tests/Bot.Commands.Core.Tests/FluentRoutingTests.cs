using System.Text.Json;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Fluent;
using Bot.Commands.Core.Fluent.Routing;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Exceptions;
using Bot.Commands.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Bot.Commands.Core.Tests.Mocks;

namespace Bot.Commands.Core.Tests;

public class FluentRoutingTests
{
    public sealed class Data : RoutedData<int> { public string Value { get; set; } }

    [Command(Name = "route_test_target", Authorized = true, Swarms = new[] { "route-tests" })]
    public sealed class Target : RoutedFluentCommand<Data, int>
    {
        public Task<ICommandExecutionResult> Go(Data caller, int state, Data child)
            => Call<Target, Data, int>(new CoreBoundaryTests.CustomContext("input"), caller, state, child);
        protected override Task<(Data, int)> Enter<T>(IQueryContext<T> q, Data d) => throw new Exception("Must not enter");
        protected override IStateMachine<int> StateMachine(IStateMachineBuilder<Data, int> b) => throw new Exception("Must not execute");
    }

    [Test]
    public async Task RouteCollectionIsSerializedOnceAndCallerSnapshotIsDetached()
    {
        var command = new Target();
        var root = new Data { Value = "root" };
        var child = new Data { Value = "child" };
        await command.Go(root, 7, child);
        var grandchild = new Data();
        await command.Go(child, 9, grandchild);
        root.Value = "changed";
        var restored = JsonSerializer.Deserialize<Data>(JsonSerializer.Serialize(grandchild));
        Assert.That(restored.Routes.Count, Is.EqualTo(2));
        var inner = restored.Routes[restored.RouteId.Value];
        Assert.That(inner.ParentRouteId, Is.EqualTo(child.RouteId));
        Assert.That(inner.State.GetInt32(), Is.EqualTo(9));
        Assert.That(restored.Routes[child.RouteId.Value].Data.GetProperty("Value").GetString(), Is.EqualTo("root"));
        foreach (var route in restored.Routes.Values)
            Assert.That(route.Data.TryGetProperty("Routes", out _), Is.False);
        Assert.That(root.Routes, Is.Empty);
        Assert.That(child.Routes.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task InvalidRouteAndConversationAreRejectedWithoutMutatingChild()
    {
        var command = new Target();
        var child = new Data();
        var query = new CoreBoundaryTests.CustomContext("input");
        await command.Go(new Data(), 1, child);
        var result = await child.ReturnAsync(query, "value");
        Assert.That(result.NextCommandDescriptor.Name, Is.EqualTo("route_test_target"));
        Assert.That(child.Routes[child.RouteId.Value].IsRealized, Is.False, "Return must build a new session snapshot");
        await Assert.ThrowsAsync<ArgumentException>(() => command.Go(new Data(), 1, child));
        var shared = new Data();
        await Assert.ThrowsAsync<ArgumentException>(() => command.Go(shared, 1, shared));
        Assert.Throws<InvalidOperationException>(() => new Data().CancelAsync(query));
        child.Routes[child.RouteId.Value].UserId = 99;
        Assert.Throws<InvalidOperationException>(() => child.ReturnAsync(query, "value"));
        child.Routes.Clear();
        Assert.Throws<InvalidOperationException>(() => child.ReturnAsync(query, "value"));
        Assert.Throws<InvalidOperationException>(() => new FluentRouteResult {
            Contract = typeof(string).FullName, Value = JsonSerializer.SerializeToElement("value")
        }.Get<int>());
    }

    [Command(Name = "route_test_start", Authorized = false, Area = ChatArea.Group, Swarms = new[] { "route-tests" })]
    public sealed class Start : IQueryCommand<string>
    {
        public Task<ICommandExecutionResult> Execute(IQueryContext<string> q) =>
            Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.AheadFluent<Target, Data, int>(new Data(), true));
    }

    [Test]
    public async Task ImmediateTransitionChecksTargetAuthorizationAndUpdateBoundaryRollsBackSession()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IClock>());
        services.AddSingleton(Mock.Of<IAuthProvider>());
        services.AddSingleton(Mock.Of<IBotProfile>(p => p.BotName == "route-test" && p.Swarms == new[] { "route-tests" }));
        services.UseInMemoryStorage();
        services.UseBotCommandsServices();
        services.AddScoped<ExecuteFilterTests.Resolver>();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<Bot.Commands.Core.Services.InMemorySessionsStore>();
        await Assert.ThrowsAsync<CommandPermissionException>(() => store.ExecuteUpdateAsync("start", "start", 10, 20,
            _ => scope.ServiceProvider.GetRequiredService<ExecuteFilterTests.Resolver>()
                .Handle(new CoreBoundaryTests.CustomContext("/route_test_start"))));
        Assert.That(store.GetSessionInfo(DateTime.UtcNow, 10, 20), Is.Null);
    }
}
