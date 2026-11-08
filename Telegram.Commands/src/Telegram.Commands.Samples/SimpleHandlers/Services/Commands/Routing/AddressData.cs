using Bot.Commands.Core.Fluent.Routing;
namespace SimpleHandlers.Services.Commands.Routing;
public sealed class AddressData : RoutedData<AddressState>
{
    public string Street { get; set; }
    public string City { get; set; }
}
