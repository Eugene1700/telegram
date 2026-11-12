using System.Reflection;
using System.Text.Json;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Exceptions;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Tests.Mocks;
using Bot.Commands.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Tg = global::Telegram.Bot.Types;
using Mx = global::Max.Bot.Types;

namespace Bot.Commands.Core.Tests;

public class CommandAssemblyRegistrationTests
{
    [Test]
    public async Task BothAdaptersUseContainerCatalogAndSelectedAssemblies()
    {
        using var included = Provider(new[] { typeof(AssemblyScopedCommand).Assembly });
        using var excluded = Provider(Array.Empty<Assembly>());
        using var firstScope = included.CreateScope();
        using var secondScope = included.CreateScope();
        Assert.That(firstScope.ServiceProvider.GetRequiredService<CommandCatalog>(),
            Is.SameAs(secondScope.ServiceProvider.GetRequiredService<CommandCatalog>()));
        Assert.That(excluded.GetRequiredService<CommandCatalog>().GetCommandTypes(), Is.Empty);

        var telegram = new Tg.Update { Message = new Tg.Message
        {
            Text = "/assembly_scoped_test", From = new Tg.User { Id = 2 },
            Chat = new Tg.Chat { Id = 1, Type = global::Telegram.Bot.Types.Enums.ChatType.Private }
        }};
        var max = JsonSerializer.Deserialize<Mx.Update>("""
            {"update_type":"message_created","message":{"sender":{"user_id":2,"name":"test"},
             "recipient":{"chat_id":1,"chat_type":"dialog"},"body":{"mid":"mid","text":"/assembly_scoped_test"}}}
            """);
        await firstScope.ServiceProvider.GetRequiredService<IBotCommandResolver<Tg.Update>>().Handle(telegram);
        await firstScope.ServiceProvider.GetRequiredService<IBotCommandResolver<Mx.Update>>().Handle(max);
        Assert.That(firstScope.ServiceProvider.GetRequiredService<AssemblyScopedCommand>().Calls, Is.EqualTo(2));

        using var excludedScope = excluded.CreateScope();
        Assert.That(excludedScope.ServiceProvider.GetService<AssemblyScopedCommand>(), Is.Null);
        await Assert.ThrowsAsync<CommandExtractionException>(() => excludedScope.ServiceProvider
            .GetRequiredService<IBotCommandResolver<Tg.Update>>().Handle(telegram));
        await Assert.ThrowsAsync<CommandExtractionException>(() => excludedScope.ServiceProvider
            .GetRequiredService<IBotCommandResolver<Mx.Update>>().Handle(max));
    }

    [Test]
    public void NoAssemblyArgumentUsesCurrentDomain()
    {
        using var provider = Provider(null);
        using var scope = provider.CreateScope();
        Assert.That(scope.ServiceProvider.GetService<AssemblyScopedCommand>(), Is.Not.Null);
        Assert.That(provider.GetRequiredService<CommandCatalog>().GetCommandTypes(), Does.Contain(typeof(AssemblyScopedCommand)));
    }

    private static ServiceProvider Provider(IEnumerable<Assembly> assemblies)
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IClock>());
        services.AddSingleton(Mock.Of<IAuthProvider>());
        services.AddSingleton(Mock.Of<IBotProfile>());
        services.AddScoped<ISessionsStore, SessionStoreMock>();
        services.UseTelegramCommandsServices(assemblies);
        // A second adapter without an explicit list must preserve the selected catalog.
        services.UseMaxCommandsServices();
        return services.BuildServiceProvider();
    }

    [Command(Name = "assembly_scoped_test", Authorized = false)]
    public sealed class AssemblyScopedCommand : IQueryCommand<Tg.Message>, IQueryCommand<Mx.Message>
    {
        public int Calls { get; private set; }
        public Task<ICommandExecutionResult> Execute(IQueryContext<Tg.Message> query) => Count();
        public Task<ICommandExecutionResult> Execute(IQueryContext<Mx.Message> query) => Count();
        private Task<ICommandExecutionResult> Count()
        {
            Calls++;
            return Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.Freeze());
        }
    }
}
