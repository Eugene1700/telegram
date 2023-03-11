using Microsoft.Extensions.DependencyInjection;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Services;
using Telegram.Commands.Core.Tests.Environments;
using Telegram.Commands.Core.Tests.Mocks;

namespace Telegram.Commands.Core.Tests.Core;

public static class CompositionRoot
{
    public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<QueryMessageMockCommand>();
        serviceCollection.AddScoped<SessionMessageMockCommand>();
        serviceCollection.AddScoped<ITelegramCommandFactory, TelegramCommandFactoryMock>();
        serviceCollection.AddScoped<SessionManager>();
        serviceCollection.AddScoped<SessionStoreMock>();
        serviceCollection.AddScoped<ISessionsStore, SessionStoreMock>(x => x.GetRequiredService<SessionStoreMock>());
        serviceCollection.AddScoped<ClockMock>();
        serviceCollection.AddScoped<IClock, ClockMock>(x => x.GetRequiredService<ClockMock>());
        serviceCollection.AddScoped<MessageEnvironment>();
        return serviceCollection;
    }
}