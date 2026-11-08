namespace Bot.Commands.Core.Fluent.Routing;

public abstract class RoutedData<TState> : IRoutedData
{
    public System.Guid? RouteId { get; set; }
    public System.Collections.Generic.Dictionary<System.Guid, FluentRouteFrame> Routes { get; set; } = new();
    public bool IsResuming { get; set; }
    public TState ResumeAt { get; set; }
    public FluentRouteResult RouteResult { get; set; }
    void IRoutedData.Resume(System.Text.Json.JsonElement state, FluentRouteResult result,
        System.Collections.Generic.Dictionary<System.Guid, FluentRouteFrame> routes)
    {
        ResumeAt = System.Text.Json.JsonSerializer.Deserialize<TState>(state);
        RouteResult = result;
        IsResuming = true;
        Routes = routes;
    }
}
