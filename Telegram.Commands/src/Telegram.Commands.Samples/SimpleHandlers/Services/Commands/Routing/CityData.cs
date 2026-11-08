using Bot.Commands.Core.Fluent.Routing;
namespace SimpleHandlers.Services.Commands.Routing;
public sealed class CityData : RoutedData<CityState> { public string City { get; set; } }
