using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bot.Commands.DependencyInjection;

public static class InMemoryStorageExtensions
{
    /// <summary>Replace the session store with process-local storage partitioned by IBotProfile.BotName.
    /// Register IBotProfile and IClock as usual. State is lost when the service provider/process is disposed.</summary>
    public static IServiceCollection UseInMemoryStorage(this IServiceCollection services)
    {
        services.TryAddSingleton<InMemorySessionStorage>();
        services.TryAddScoped<InMemorySessionsStore>();
        services.Replace(ServiceDescriptor.Scoped<ISessionsStore>(sp => sp.GetRequiredService<InMemorySessionsStore>()));
        return services;
    }
}
