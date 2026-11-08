using System.Text.Json;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Fluent;
using Bot.Commands.Core.Services;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Core.Fluent.Routing;
using Bot.Commands.Core.Fluent.Builders.Extensions;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;
using Bot.Commands.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SimpleHandlers.Services;
using SimpleHandlers.Services.Commands.Routing;
using Tg = global::Telegram.Bot.Types;
using Mx = global::Max.Bot.Types;

namespace Bot.Commands.Core.Tests;

// These tests execute the actual SimpleHandlers flow via the actual adapters.
// Two independent containers share a memory backend, not mocks of the command engine.
public class InMemoryRoutingBehaviorTests
{
    public enum ParentState { Call, Review, AlternateReview }
    public sealed class ParentData : RoutedData<ParentState>
    {
        public string City { get; set; }
        public Guid CompletedRoute { get; set; }
    }
    public abstract class DynamicParent(ISampleMessageSender sender) : RoutedFluentCommand<ParentData, ParentState>
    {
        protected abstract ParentState ReturnState { get; }
        protected override Task<(ParentData, ParentState)> Enter<T>(IQueryContext<T> q, ParentData data)
            => Task.FromResult((new ParentData(), ParentState.Call));
        protected override Task OnReturn<T>(IQueryContext<T> q, ParentData data, ParentState state, FluentRouteResult result)
        {
            data.City = result.Get<string>();
            data.CompletedRoute = result.RouteId;
            return Task.CompletedTask;
        }
        protected override IStateMachine<ParentState> StateMachine(IStateMachineBuilder<ParentData, ParentState> b)
            => b.Route(ParentState.Call, (q, d) => Call<TelegramCityFlow, CityData, CityState>(q, d, ReturnState, new CityData()))
                .State(ParentState.Review).WithMessage("First parent resumed", Send).Loop(true)
                .State(ParentState.AlternateReview).WithMessage("Second parent resumed", Send).Loop(true).Build();
        private Task Send(IQueryContext<object> q, ParentState state, ParentData d, IBotMessage message)
            => sender.Send(q.GetChatId(), message);
    }
    [Command(Name = "dynamic_first", Authorized = false, Swarms = new[] { "telegram" })]
    public sealed class FirstParent(ISampleMessageSender sender) : DynamicParent(sender)
    { protected override ParentState ReturnState => ParentState.Review; }
    [Command(Name = "dynamic_second", Authorized = false, Swarms = new[] { "telegram" })]
    public sealed class SecondParent(ISampleMessageSender sender) : DynamicParent(sender)
    { protected override ParentState ReturnState => ParentState.AlternateReview; }

    [Test]
    public async Task SameChildReturnsToDifferentRuntimeCallersWithoutRouteRegistration()
    {
        var backend = new InMemorySessionStorage();
        using var firstInstance = new Instance(backend, false);
        await firstInstance.Send("first-start", "/dynamic_first");
        await firstInstance.Send("second-start", "/dynamic_second", user: 4);
        var firstChild = firstInstance.Read<CityData, CityState>("/routecity").Object;
        var firstRoute = firstChild.Routes[firstChild.RouteId.Value];
        var secondChild = firstInstance.Read<CityData, CityState>("/routecity", user: 4).Object;
        var secondRoute = secondChild.Routes[secondChild.RouteId.Value];
        Assert.That(firstRoute.RouteId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(secondRoute.RouteId, Is.Not.EqualTo(firstRoute.RouteId));
        Assert.That(firstRoute.CallerCommand, Is.EqualTo("dynamic_first"));
        Assert.That(secondRoute.CallerCommand, Is.EqualTo("dynamic_second"));
        // New command instances; only serialized session snapshots are shared.
        using var restarted = new Instance(backend, false);
        Assert.That((await restarted.Send("first-city", "Moscow")).Message.Text, Is.EqualTo("First parent resumed"));
        Assert.That((await restarted.Send("second-city", "Berlin", user: 4)).Message.Text, Is.EqualTo("Second parent resumed"));
        var first = restarted.Read<ParentData, ParentState>("/dynamic_first");
        var second = restarted.Read<ParentData, ParentState>("/dynamic_second", user: 4);
        Assert.That(first.Object.City, Is.EqualTo("Moscow"));
        Assert.That(second.Object.City, Is.EqualTo("Berlin"));
        Assert.That(first.Object.CompletedRoute, Is.EqualTo(firstRoute.RouteId));
        Assert.That(second.Object.CompletedRoute, Is.EqualTo(secondRoute.RouteId));
        Assert.That(first.CurrentStateId.Data, Is.EqualTo(ParentState.Review));
        Assert.That(second.CurrentStateId.Data, Is.EqualTo(ParentState.AlternateReview));
    }

    private sealed class Sender : ISampleMessageSender
    {
        public IBotMessage Last;
        public Func<IBotMessage, Task> OnSend;
        public async Task Send(long chatId, IBotMessage message)
        { Last = message; if (OnSend != null) await OnSend(message); }
        public Task Edit(long chatId, string messageId, IBotMessage message) => Send(chatId, message);
    }

    private sealed class Instance : IDisposable
    {
        public readonly ServiceProvider Provider;
        private readonly bool _max;
        public Instance(InMemorySessionStorage storage, bool max, string bot = "routing-test")
        {
            _max = max;
            var services = new ServiceCollection();
            services.AddSingleton(storage);
            services.AddSingleton(Mock.Of<IBotProfile>(p => p.BotName == bot && p.Swarms == new[] { max ? "max" : "telegram" }));
            var clock = new Mock<IClock>(); clock.SetupGet(x => x.Now).Returns(() => DateTime.UtcNow);
            services.AddSingleton(clock.Object);
            services.AddSingleton(Mock.Of<IAuthProvider>());
            services.AddScoped<Sender>();
            services.AddScoped<ISampleMessageSender>(sp => sp.GetRequiredService<Sender>());
            services.UseTelegramCommandsServices(); services.UseMaxCommandsServices(); services.UseInMemoryStorage();
            Provider = services.BuildServiceProvider();
        }
        public async Task<(bool Processed, IBotMessage Message)> Send(string id, string text = null, string callback = null,
            Func<IBotMessage, Task> onSend = null, bool failAfterSave = false, long user = 3, CancellationToken token = default)
        {
            using var scope = Provider.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<Sender>(); sender.OnSend = onSend;
            var store = scope.ServiceProvider.GetRequiredService<InMemorySessionsStore>();
            var processed = await store.ExecuteUpdateAsync(id, JsonSerializer.Serialize(new { text, callback }), 2, user, async _ => {
                if (_max)
                {
                    var message = new { sender = new { user_id = user, name = "test" },
                        recipient = new { chat_id = 2, chat_type = "dialog" }, body = new { mid = "msg", text } };
                    var json = callback == null
                        ? JsonSerializer.Serialize(new { update_type = "message_created", message })
                        : JsonSerializer.Serialize(new { update_type = "message_callback", message,
                            callback = new { callback_id = id, payload = callback, user = new { user_id = user, name = "test" } } });
                    await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<Mx.Update>>()
                        .Handle(JsonSerializer.Deserialize<Mx.Update>(json));
                }
                else
                {
                    var message = new Tg.Message { Id = 1, Text = text, From = new Tg.User { Id = user },
                        Chat = new Tg.Chat { Id = 2, Type = global::Telegram.Bot.Types.Enums.ChatType.Private } };
                    var update = callback == null ? new Tg.Update { Message = message } : new Tg.Update {
                        CallbackQuery = new Tg.CallbackQuery { Id = id, Data = callback, Message = message, From = message.From } };
                    await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<Tg.Update>>().Handle(update);
                }
                if (failAfterSave) throw new InvalidOperationException("Failure after resolver wrote the session.");
            }, token);
            return (processed, sender.Last);
        }
        public FluentObject<T, TState> Read<T, TState>(string command, long user = 3)
        {
            using var scope = Provider.CreateScope();
            return scope.ServiceProvider.GetRequiredService<ISessionsStore>()
                .GetSessionInfoWithData<FluentObject<T, TState>>(DateTime.UtcNow, 2, user, command)?.Data;
        }
        public void Dispose() => Provider.Dispose();
    }
    private static string Click(IBotMessage message, string title)
    {
        var button = message.Callbacks.Single(c => c.Text == title);
        return button.CommandDescriptor == null ? button.CallbackText : $"/{button.CommandDescriptor.Name} {button.CallbackText}";
    }
    private static async Task AtCity(Instance one, Instance two)
    {
        Assert.That((await one.Send("start", "/routeorder")).Message.Text, Is.EqualTo("Как вас зовут?"));
        Assert.That((await two.Send("name", "Ada")).Message.Text, Is.EqualTo("Улица и дом:"));
        Assert.That((await one.Send("street", "Main 1")).Message.Text, Is.EqualTo("Город:"));
    }

    [TestCase(false)] [TestCase(true)]
    public async Task NestedReturnAndCancellationUseTheRealSampleAcrossInstances(bool max)
    {
        var backend = new InMemorySessionStorage();
        using var one = new Instance(backend, max); using var two = new Instance(backend, max);
        await AtCity(one, two);
        var city = await two.Send("city", "Moscow");
        var address = two.Read<AddressData, AddressState>("/routeaddress");
        Assert.That(address.Object.City, Is.EqualTo("Moscow"));
        Assert.That(address.CurrentStateId.Data, Is.EqualTo(AddressState.Review));
        Assert.That(address.Object.Routes.Count, Is.EqualTo(2));
        var completedCity = address.Object.Routes.Values.Single(r => r.IsRealized);
        Assert.That(completedCity.ParentRouteId, Is.EqualTo(address.Object.RouteId));
        Assert.That(completedCity.Result.Get<string>(), Is.EqualTo("Moscow"));
        var order = await one.Send("save", callback: Click(city.Message, "Сохранить адрес"));
        Assert.That(order.Message.Text, Is.EqualTo("Заказ: Ada; адрес: Main 1, Moscow"));
        var savedOrder = two.Read<OrderData, OrderState>("/routeorder").Object;
        Assert.That(savedOrder.RouteId, Is.Null);
        Assert.That(savedOrder.Routes.Count, Is.EqualTo(2));
        Assert.That(savedOrder.Routes.Values.All(r => r.IsRealized && !r.Result.Cancelled), Is.True);
        await two.Send("edit", callback: Click(order.Message, "Адрес"));
        var street = await one.Send("new-street", "New 2");
        var cancelledCity = await two.Send("cancel-city", callback: Click(street.Message, "Отмена города"));
        var cancelledAddress = await one.Send("cancel-address", callback: Click(cancelledCity.Message, "Отмена адреса"));
        Assert.That(cancelledAddress.Message.Text, Is.EqualTo("Заказ: Ada; адрес: Main 1, Moscow"));
        var cancelledRoutes = two.Read<OrderData, OrderState>("/routeorder").Object.Routes.Values;
        Assert.That(cancelledRoutes.Count, Is.EqualTo(4));
        Assert.That(cancelledRoutes.Count(r => r.IsRealized && r.Result.Cancelled), Is.EqualTo(2));
        await two.Send("done", callback: Click(cancelledAddress.Message, "Готово"));
        Assert.That(one.Read<OrderData, OrderState>("/routeorder"), Is.Null);
        Assert.That((await one.Send("done", callback: Click(cancelledAddress.Message, "Готово"))).Processed, Is.False);
    }

    [TestCase(false)] [TestCase(true)]
    public async Task ConcurrentDuplicateReturnCannotRunAgainInTheParent(bool max)
    {
        var backend = new InMemorySessionStorage();
        using var one = new Instance(backend, max); using var two = new Instance(backend, max);
        await AtCity(one, two);
        var city = await one.Send("city", "Moscow");
        var callback = Click(city.Message, "Сохранить адрес");
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var first = one.Send("return", callback: callback, onSend: async _ => { entered.SetResult(); await release.Task; });
        Task<(bool Processed, IBotMessage Message)> duplicate = null;
        try
        {
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            duplicate = two.Send("return", callback: callback);
            Assert.That(duplicate.IsCompleted, Is.False);
            // Neither another user nor another bot must wait for this return.
            Assert.That((await two.Send("other-user", "/routeorder", user: 4).WaitAsync(TimeSpan.FromSeconds(5))).Processed, Is.True);
            using var otherBot = new Instance(backend, max, "other-bot");
            Assert.That((await otherBot.Send("return", "/routeorder").WaitAsync(TimeSpan.FromSeconds(5))).Processed, Is.True);
        }
        finally { release.TrySetResult(); }
        Assert.That((await first).Processed, Is.True);
        Assert.That((await duplicate).Processed, Is.False);
        Assert.That((await duplicate).Message, Is.Null);
        var parent = two.Read<OrderData, OrderState>("/routeorder");
        Assert.That(parent.Object.Address, Is.EqualTo("Main 1, Moscow"));
        Assert.That(parent.CurrentStateId.Data, Is.EqualTo(OrderState.Review));
        // Lost acknowledgement: another container with the same backend must not re-execute.
        using var restarted = new Instance(backend, max);
        Assert.That((await restarted.Send("return", callback: callback)).Processed, Is.False);
        await Assert.ThrowsAsync<InvalidOperationException>(() => restarted.Send("return", "different payload"));
    }

    [TestCase(false, false)] [TestCase(true, false)]
    [TestCase(false, true)] [TestCase(true, true)]
    public async Task FailureDuringReturnOrAfterSessionWritePreservesTheNestedCallerChain(bool max, bool afterSave)
    {
        var backend = new InMemorySessionStorage();
        using var one = new Instance(backend, max); using var two = new Instance(backend, max);
        await AtCity(one, two);
        await Assert.ThrowsAsync<InvalidOperationException>(() => one.Send("city", "Moscow",
            onSend: afterSave ? null : _ => throw new InvalidOperationException("Parent prompt failed"), failAfterSave: afterSave));
        var child = two.Read<CityData, CityState>("/routecity");
        Assert.That(child.CurrentStateId.Data, Is.EqualTo(CityState.Input));
        Assert.That(child.Object.City, Is.Null);
        Assert.That(child.Object.RouteId, Is.Not.Null);
        Assert.That(child.Object.Routes.Count, Is.EqualTo(2));
        Assert.That(child.Object.Routes.Values.All(r => !r.IsRealized && r.Result == null), Is.True);
        var retry = await two.Send("city", "Moscow");
        Assert.That(retry.Message.Text, Is.EqualTo("Адрес: Main 1; город: Moscow"));
        var result = await one.Send("save", callback: Click(retry.Message, "Сохранить адрес"));
        Assert.That(result.Message.Text, Is.EqualTo("Заказ: Ada; адрес: Main 1, Moscow"));
    }

    [TestCase(false)] [TestCase(true)]
    public async Task ConcurrentDistinctAnswersObserveCommittedStateAndCancelledWaitCanRetry(bool max)
    {
        var backend = new InMemorySessionStorage();
        using var one = new Instance(backend, max); using var two = new Instance(backend, max);
        await AtCity(one, two);
        var entered = new TaskCompletionSource<IBotMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var city = one.Send("city", "Moscow", onSend: async message => { entered.SetResult(message); await release.Task; });
        Task<(bool Processed, IBotMessage Message)> save = null;
        string callback = null;
        try
        {
            callback = Click(await entered.Task.WaitAsync(TimeSpan.FromSeconds(5)), "Сохранить адрес");
            using var cancel = new CancellationTokenSource();
            var waiting = two.Send("save", callback: callback, token: cancel.Token);
            cancel.Cancel();
            Assert.That(async () => await waiting, Throws.InstanceOf<OperationCanceledException>());
            save = two.Send("save", callback: callback);
            Assert.That(save.IsCompleted, Is.False);
        }
        finally { release.TrySetResult(); }
        await city;
        Assert.That((await save).Message.Text, Is.EqualTo("Заказ: Ada; адрес: Main 1, Moscow"));
        using var empty = new Instance(new InMemorySessionStorage(), max);
        Assert.That(empty.Read<OrderData, OrderState>("/routeorder"), Is.Null);
    }
}
