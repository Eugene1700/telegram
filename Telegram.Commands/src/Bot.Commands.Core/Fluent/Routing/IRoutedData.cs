using System.Text.Json;
namespace Bot.Commands.Core.Fluent.Routing;

internal interface IRoutedData
{
    void Resume(JsonElement state, FluentRouteResult result,
        System.Collections.Generic.Dictionary<System.Guid, FluentRouteFrame> routes);
}
