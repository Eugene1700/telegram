using Bot.Commands.Core.Fluent.Routing;
namespace SurveyComparison;
public sealed class OrderData : RoutedData<OrderState>
{
    public string Name { get; set; }
    public string Address { get; set; }
}
