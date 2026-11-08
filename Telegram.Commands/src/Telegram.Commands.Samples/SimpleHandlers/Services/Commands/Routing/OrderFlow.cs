using Bot.Commands.Core.Fluent.Routing;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;
using Bot.Commands.Core.Fluent.Builders.Extensions;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;
using Bot.Commands.Core.Models;
using System;
using System.Threading.Tasks;
namespace SimpleHandlers.Services.Commands.Routing;

[Command(Name = "routeorder", Authorized = false)]
public abstract class OrderFlow<TMessage>(ISampleMessageSender sender) : Flow<OrderData, OrderState>(sender)
    where TMessage : class
{
    protected override Task<(OrderData, OrderState)> Enter<T>(IQueryContext<T> query, OrderData data)
        => Task.FromResult((new OrderData(), OrderState.Name));
    protected override Task OnReturn<TQuery>(IQueryContext<TQuery> query, OrderData data, OrderState resumeAt, FluentRouteResult result)
    {
        if (!result.Cancelled) {
            var address = result.Get<AddressResult>();
            data.Address = address.Street + ", " + address.City;
        }
        return Task.CompletedTask;
    }

    protected override IStateMachine<OrderState> StateMachine(IStateMachineBuilder<OrderData, OrderState> b)
        => b.State(OrderState.Name)
            .WithMessage("Как вас зовут?", Send)
            .WithCallbacks().Row().NextFromCallback("Отмена", "", OrderState.Cancel, true)
            .Next<TMessage>((q, s, d) => {
                d.Name = q.GetData()?.Trim();
                return Task.FromResult(string.IsNullOrWhiteSpace(d.Name) ? OrderState.Name : OrderState.Address);
            }, true)
          .State(OrderState.Review)
            .WithMessage((s,d) => Task.FromResult($"Заказ: {d.Name}; адрес: {d.Address ?? "—"}"), Send)
            .WithCallbacks()
                .Row().NextFromCallback("Адрес", "", OrderState.Address, true)
                .Row().NextFromCallback("Готово", "", OrderState.Done, true)
                .Row().NextFromCallback("Отмена", "", OrderState.Cancel, true)
            .Loop(true)
          .Route(OrderState.Address, (q, d) =>
                Call<AddressFlow<TMessage>, AddressData, AddressState>(q, d, OrderState.Review, new AddressData()))
          .Exit<object>(OrderState.Done, (q,s,d) => Finish(q, $"Готово: {d.Name}; {d.Address ?? "без адреса"}"))
          .Exit<object>(OrderState.Cancel, (q,s,d) => Finish(q, "Заказ отменён."))
          .Build();
    private async Task<ICommandExecutionResult> Finish<TQuery>(IQueryContext<TQuery> query, string text)
    { await sender.Send(query.GetChatId(), new Bot.Commands.Abstract.Messages.BotMessage(text, null, null, null)); return CommandExecutionResult.Break(); }
}
