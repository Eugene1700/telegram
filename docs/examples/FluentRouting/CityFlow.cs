using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Core.Fluent.Routing;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;
using Bot.Commands.Core.Fluent.Builders.Extensions;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;
using Telegram.Bot.Types;
namespace SurveyComparison;

[Command(Name = "city", Authorized = false)]
public sealed class CityFlow(Output output) : Flow<CityData, CityState>(output)
{
    protected override Task<(CityData, CityState)> Enter<T>(IQueryContext<T> query, CityData data)
    {
        if (data?.RouteId == null) throw new InvalidOperationException("City must be called by another flow.");
        return Task.FromResult((data, CityState.Input));
    }
    protected override IStateMachine<CityState> StateMachine(IStateMachineBuilder<CityData, CityState> b)
        => b.State(CityState.Input)
            .WithMessage("Город:", Send)
            .WithCallbacks().Row().NextFromCallback("Отмена города", "", CityState.Cancel, true)
            .Next<Message>((q,s,d) => {
                d.City = q.GetData()?.Trim();
                return Task.FromResult(string.IsNullOrWhiteSpace(d.City) ? CityState.Input : CityState.Done);
            }, true)
          .ReturnRoute(CityState.Done, d => d.City)
          .CancelRoute(CityState.Cancel)
          .Build();
}
