using Microsoft.Extensions.DependencyInjection;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Services;
using Bot.Commands.Core.Tests.Environments;
using Bot.Commands.Core.Tests.Mocks;

namespace Bot.Commands.Core.Tests.Core;

public static class CompositionRoot
{
    public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<QueryMessageMockCommand>();
        serviceCollection.AddScoped<SessionMessageMockCommand>();
        serviceCollection.AddScoped<BehaviorMockCommand>();
        serviceCollection.AddScoped<ICommandFactory, CommandFactoryMock>();
        serviceCollection.AddScoped<SessionManager>();
        serviceCollection.AddScoped<SessionStoreMock>();
        serviceCollection.AddScoped<ISessionsStore, SessionStoreMock>(x => x.GetRequiredService<SessionStoreMock>());
        serviceCollection.AddScoped<ClockMock>();
        serviceCollection.AddScoped<IClock, ClockMock>(x => x.GetRequiredService<ClockMock>());
        serviceCollection.AddScoped<MessageEnvironment>();
        return serviceCollection;
    }
}