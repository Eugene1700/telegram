using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Fluent;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Services;
using Bot.Commands.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Moq;
using SimpleHandlers.Services;

namespace Bot.Commands.Core.Tests;

public class InMemoryStorageTests
{
    [Test]
    public void SimpleHandlersCanSelectInMemoryStorageFromConfiguration()
    {
        var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string> { ["BotProfile:BotName"] = "memory-test" })
            .AddInMemoryCollection(new Dictionary<string, string> { ["Sessions:UseInMemory"] = "true" }).Build();
        var services = new ServiceCollection();
        new SimpleHandlers.Startup(config).ConfigureServices(services);
        services.AddLogging();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.That(scope.ServiceProvider.GetRequiredService<ISessionsStore>(),
            Is.SameAs(scope.ServiceProvider.GetRequiredService<InMemorySessionsStore>()));
    }

    private static readonly DateTime Now = new(2026, 11, 8, 0, 0, 0, DateTimeKind.Utc);
    private static CommandSession Session(object data, DateTime? expires = null) => new()
    {
        CommandQuery = "/survey", ChatId = 2, UserId = 3, OpenedAt = Now,
        ExpiredAt = expires, Data = data
    };
    private static ServiceCollection Services(Func<string> name)
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IClock>(c => c.Now == Now));
        services.AddScoped<IBotProfile>(_ => new BotProfile { BotName = name() });
        services.AddSingleton(Mock.Of<ISessionsStore>());
        services.UseInMemoryStorage().UseInMemoryStorage();
        return services;
    }

    [Test]
    public async Task ConfigurationSharesStateAcrossScopesButIsolatesBotsAndProviders()
    {
        var bot = "one";
        var services = Services(() => bot);
        Assert.That(services.Count(d => d.ServiceType == typeof(ISessionsStore)), Is.EqualTo(1));
        Assert.That(services.Count(d => d.ServiceType == typeof(InMemorySessionStorage)), Is.EqualTo(1));
        using var provider = services.BuildServiceProvider();
        using (var scope = provider.CreateScope())
            await scope.ServiceProvider.GetRequiredService<ISessionsStore>().CreateSession(Session(new Payload { Name = "Ada" }));
        using (var scope = provider.CreateScope())
            Assert.That(scope.ServiceProvider.GetRequiredService<ISessionsStore>()
                .GetSessionInfoWithData<Payload>(Now, 2, 3, "/survey").Data.Name, Is.EqualTo("Ada"));
        bot = "two";
        using (var scope = provider.CreateScope())
        {
            var store = scope.ServiceProvider.GetRequiredService<ISessionsStore>();
            Assert.That(store.GetSessionInfo(Now, 2, 3), Is.Null);
            await store.CreateSession(Session(new Payload { Name = "Grace" }));
        }
        bot = "one";
        using (var scope = provider.CreateScope())
            Assert.That(scope.ServiceProvider.GetRequiredService<ISessionsStore>()
                .GetSessionInfoWithData<Payload>(Now, 2, 3, "/survey").Data.Name, Is.EqualTo("Ada"));
        using var restarted = services.BuildServiceProvider();
        using var fresh = restarted.CreateScope();
        Assert.That(fresh.ServiceProvider.GetRequiredService<ISessionsStore>().GetSessionInfo(Now, 2, 3), Is.Null);
    }

    [Test]
    public async Task SnapshotPreservesFluentStateAndDoesNotShareMutableData()
    {
        using var provider = Services(() => "bot").BuildServiceProvider();
        using var scope = provider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<ISessionsStore>();
        var data = new FluentObject<Payload, int>(new Payload { Name = "Ada" }, FireType.HandleCurrent)
        {
            CurrentStateId = new() { Data = 2, IsInit = true }
        };
        await store.CreateSession(Session(data));
        data.Object.Name = "unsaved";
        var restored = store.GetSessionInfoWithData<FluentObject<Payload, int>>(Now, 2, 3, "/survey").Data;
        Assert.That(restored.Object.Name, Is.EqualTo("Ada"));
        Assert.That(restored.CurrentStateId.Data, Is.EqualTo(2));
        Assert.That(restored.CurrentStateId.IsInit, Is.True);
        restored.Object.Name = "also unsaved";
        Assert.That(((FluentObject<Payload, int>)store.GetSessionInfoWithData(Now, 2, 3, "/survey", typeof(FluentObject<Payload, int>)).Data)
            .Object.Name, Is.EqualTo("Ada"));
        Assert.That(store.GetSessionInfoWithData<Payload>(Now, 2, 3, "/other"), Is.Null);
        Assert.That(store.GetSessionInfo(Now, 2, 4), Is.Null);
    }

    [Test]
    public async Task UpdateMovesSessionAndCompletionOrExpiryRemovesIt()
    {
        using var provider = Services(() => "bot").BuildServiceProvider();
        using var scope = provider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<ISessionsStore>();
        var session = Session(new Payload { Name = "Ada" }, Now.AddMinutes(1));
        await store.CreateSession(session);
        session.ChatId = 4;
        await store.UpdateSession(session, 2);
        Assert.That(store.GetSessionInfo(Now, 2, 3), Is.Null);
        Assert.That(store.GetSessionInfo(Now, 4, 3), Is.Not.Null);
        Assert.That(store.GetSessionInfo(Now.AddMinutes(1), 4, 3), Is.Null);
        await store.CreateSession(Session(new Payload()));
        var completed = Session(null, Now);
        await store.UpdateSession(completed, 2);
        Assert.That(store.GetSessionInfo(Now, 2, 3), Is.Null);
    }

    [Test]
    public void MissingBotNameIsRejected()
    {
        using var provider = Services(() => "").BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.Throws<ArgumentException>(() => scope.ServiceProvider.GetRequiredService<ISessionsStore>());
    }

    public sealed class Payload { public string Name { get; set; } }

    [Test]
    public async Task FailedUpdateRollsBackAndCanRetryWithTheSameEventId()
    {
        using var provider = Services(() => "bot").BuildServiceProvider();
        using var scope = provider.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<InMemorySessionsStore>();
        await store.CreateSession(Session(new Payload { Name = "before" }));
        await Assert.ThrowsAsync<InvalidOperationException>(() => store.ExecuteUpdateAsync("event", "content", 2, 3, async _ =>
        {
            await store.UpdateSession(Session(new Payload { Name = "failed" }), 2);
            throw new InvalidOperationException("handler failed");
        }));
        Assert.That(store.GetSessionInfoWithData<Payload>(Now, 2, 3, "/survey").Data.Name, Is.EqualTo("before"));
        Assert.That(await store.ExecuteUpdateAsync("event", "content", 2, 3,
            _ => store.UpdateSession(Session(new Payload { Name = "retry" }), 2)), Is.True);
        Assert.That(store.GetSessionInfoWithData<Payload>(Now, 2, 3, "/survey").Data.Name, Is.EqualTo("retry"));
        Assert.That(await store.ExecuteUpdateAsync("event", "content", 2, 3,
            _ => throw new Exception("Duplicate must not execute")), Is.False);
        await Assert.ThrowsAsync<InvalidOperationException>(() => store.ExecuteUpdateAsync("event", "changed", 2, 3,
            _ => Task.CompletedTask));
    }

    [Test]
    public async Task IndependentProvidersSerializeDuplicateUpdatesOnSharedBackend()
    {
        var backend = new InMemorySessionStorage();
        var firstServices = Services(() => "bot"); firstServices.AddSingleton(backend);
        var secondServices = Services(() => "bot"); secondServices.AddSingleton(backend);
        using var first = firstServices.BuildServiceProvider();
        using var second = secondServices.BuildServiceProvider();
        using var firstScope = first.CreateScope();
        using var secondScope = second.CreateScope();
        var one = firstScope.ServiceProvider.GetRequiredService<InMemorySessionsStore>();
        var two = secondScope.ServiceProvider.GetRequiredService<InMemorySessionsStore>();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var update = one.ExecuteUpdateAsync("event", "content", 2, 3, async _ =>
        {
            await one.CreateSession(Session(new Payload { Name = "committed" }));
            entered.SetResult();
            await release.Task;
        });
        Task<bool> duplicate = null;
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.That(two.GetSessionInfo(Now, 2, 3), Is.Null);
            duplicate = two.ExecuteUpdateAsync("event", "content", 2, 3,
                _ => throw new Exception("Duplicate must not execute"));
            Assert.That(duplicate.IsCompleted, Is.False);
        }
        finally { release.TrySetResult(); }
        Assert.That(await update, Is.True);
        Assert.That(await duplicate, Is.False);
        Assert.That(two.GetSessionInfoWithData<Payload>(Now, 2, 3, "/survey").Data.Name, Is.EqualTo("committed"));
    }
}
