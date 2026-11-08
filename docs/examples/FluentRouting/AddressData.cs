using Bot.Commands.Core.Fluent.Routing;
namespace SurveyComparison;
public sealed class AddressData : RoutedData<AddressState>
{
    public string Street { get; set; }
    public string City { get; set; }
}
