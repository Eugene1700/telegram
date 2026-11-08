using System;
using System.Text.Json;

namespace Bot.Commands.Core.Fluent.Routing;

/// <summary>Serializable return value. Contract names are compared, never loaded as CLR types.</summary>
public sealed class FluentRouteResult
{
    public Guid RouteId { get; set; }
    public bool Cancelled { get; set; }
    public string Contract { get; set; }
    public JsonElement Value { get; set; }

    public TResult Get<TResult>()
    {
        if (Cancelled) throw new InvalidOperationException("A cancelled route has no result.");
        if (Contract != typeof(TResult).FullName)
            throw new InvalidOperationException($"Expected route result {typeof(TResult).FullName}, received {Contract}.");
        return Value.Deserialize<TResult>();
    }
}
