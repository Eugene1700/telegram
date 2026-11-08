using Bot.Commands.Core.Fluent.Routing;
namespace SimpleHandlers.Services.Commands.Routing;
public sealed class OrderData : RoutedData<OrderState>
{
    public string Name { get; set; }
    public string Address { get; set; }
}
