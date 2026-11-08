using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Core.Fluent.Routing;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;
using Bot.Commands.Core.Fluent.Builders.Extensions;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;
using System;
using System.Threading.Tasks;
namespace SimpleHandlers.Services.Commands.Routing;

[Command(Name = "routeaddress", Authorized = false)]
public abstract class AddressFlow<TMessage>(ISampleMessageSender sender) : Flow<AddressData, AddressState>(sender)
    where TMessage : class
{
    protected override Task<(AddressData, AddressState)> Enter<T>(IQueryContext<T> query, AddressData data)
    {
        if (data?.RouteId == null) throw new InvalidOperationException("Address must be called by another flow.");
        return Task.FromResult((data, AddressState.Street));
    }
    protected override Task OnReturn<TQuery>(IQueryContext<TQuery> query, AddressData data, AddressState resumeAt, FluentRouteResult result)
    {
        if (!result.Cancelled) data.City = result.Get<string>();
        return Task.CompletedTask;
    }

    protected override IStateMachine<AddressState> StateMachine(IStateMachineBuilder<AddressData, AddressState> b)
        => b.State(AddressState.Street)
            .WithMessage("Улица и дом:", Send)
            .WithCallbacks().Row().NextFromCallback("Отмена адреса", "", AddressState.Cancel, true)
            .Next<TMessage>((q,s,d) => {
                d.Street = q.GetData()?.Trim();
                return Task.FromResult(string.IsNullOrWhiteSpace(d.Street) ? AddressState.Street : AddressState.City);
            }, true)
          .State(AddressState.Review)
            .WithMessage((s,d) => Task.FromResult($"Адрес: {d.Street}; город: {d.City ?? "—"}"), Send)
            .WithCallbacks()
                .Row().NextFromCallback("Город", "", AddressState.City, true)
                .Row().NextFromCallback("Сохранить адрес", "", AddressState.Done, true)
                .Row().NextFromCallback("Отмена адреса", "", AddressState.Cancel, true)
            .Loop(true)
          .Route(AddressState.City, (q, d) =>
                Call<CityFlow<TMessage>, CityData, CityState>(q, d, AddressState.Review, new CityData()))
          .Route(AddressState.Done, (q, d) =>
                string.IsNullOrWhiteSpace(d.City)
                    ? Call<CityFlow<TMessage>, CityData, CityState>(q, d, AddressState.Review, new CityData())
                    : d.ReturnAsync(q, new AddressResult(d.Street, d.City)))
          .CancelRoute(AddressState.Cancel)
          .Build();
}
