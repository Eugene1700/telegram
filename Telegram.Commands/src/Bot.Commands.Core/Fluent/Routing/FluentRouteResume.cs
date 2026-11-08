using System;
using System.Text.Json;
namespace Bot.Commands.Core.Fluent.Routing;

internal sealed record FluentRouteResume(FluentRouteFrame Frame, FluentRouteResult Result,
    System.Collections.Generic.Dictionary<Guid, FluentRouteFrame> Routes)
{
    public object Restore(Type sessionType)
    {
        if (!sessionType.IsGenericType || sessionType.GetGenericTypeDefinition() != typeof(FluentObject<,>))
            throw new InvalidOperationException("Return destination must be a Fluent session.");
        var data = Frame.Data.Deserialize(sessionType.GetGenericArguments()[0]);
        if (data is not IRoutedData routed)
            throw new InvalidOperationException("Return destination must use routed data.");
        routed.Resume(Frame.State, Result, Routes);
        return Activator.CreateInstance(sessionType, data, FireType.Entry);
    }
}
