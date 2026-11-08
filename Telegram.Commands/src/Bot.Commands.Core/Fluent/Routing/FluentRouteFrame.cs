using System;
using System.Text.Json;
namespace Bot.Commands.Core.Fluent.Routing;

/// <summary>Runtime route record persisted inside session data. The host isolates sessions by bot.</summary>
public sealed class FluentRouteFrame
{
    public Guid RouteId { get; set; }
    public Guid? ParentRouteId { get; set; }
    public long ChatId { get; set; }
    public long UserId { get; set; }
    public string CallerCommand { get; set; }
    public string DestinationCommand { get; set; }
    public JsonElement Data { get; set; }
    public JsonElement State { get; set; }
    public bool IsRealized { get; set; }
    public FluentRouteResult Result { get; set; }
}
