using Microsoft.Extensions.DependencyInjection;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;
using Telegram.Commands.Core.Tests.Commands;
using Telegram.Commands.Core.Tests.Environments;
using Telegram.Commands.Core.Tests.Mocks;

namespace Telegram.Commands.Core.Tests.Core;

public static class CompositionRoot
{
    public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<QueryMessageMockCommand>();
        serviceCollection.AddScoped<ITelegramCommandFactory, TelegramCommandFactoryMock>();
        serviceCollection.AddScoped<SessionManager>();
        serviceCollection.AddScoped<ISessionsStore, SessionStoreMock>();
        serviceCollection.AddScoped<IClock, ClockMock>();
        serviceCollection.AddScoped<MessageEnvironment>();
        return serviceCollection;
    }
}