using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Services;
using Bot.Commands.Core.Tests.Mocks;
using Bot.Commands.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Bot.Commands.Core.Tests;

public class CoreBoundaryTests
{
    [Test]
    public void CommonMessageBuilderDoesNotImposePlatformLengthLimit()
    {
        var text = new string('x', 5000);
        var messages = new PlainBuilder().AppendPlain(text).GetMessages();
        Assert.That(messages, Has.Length.EqualTo(1));
        Assert.That(messages[0].Text, Is.EqualTo(text));
    }

    private sealed class PlainBuilder : MessageBuilderBase
    {
        protected override IMessageTextFormat Format => null;
        protected override string PrepareText(string text) => text;
    }

    [Test]
    public void SharedAssembliesHaveNoMessengerReferencesOrPublicTypeNames()
    {
        var assemblies = new[]
        {
            typeof(IQueryCommand<>).Assembly,
            typeof(CommandExecutionService).Assembly,
            typeof(BotDependencyExtensions).Assembly
        };
        foreach (var assembly in assemblies)
        {
            var references = assembly.GetReferencedAssemblies().Select(a => a.Name).ToArray();
            Assert.That(references.Any(IsMessengerName), Is.False, assembly.FullName);
            Assert.That(assembly.GetExportedTypes().Any(t => IsMessengerName(t.FullName)), Is.False, assembly.FullName);
        }
    }

    [Test]
    public async Task CommonResolverHandlesCustomGroupContextWithoutMentionRules()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IClock>());
        services.AddSingleton(Mock.Of<IAuthProvider>());
        services.AddSingleton(Mock.Of<IBotProfile>());
        services.AddScoped<ISessionsStore, SessionStoreMock>();
        services.UseBotCommandsServices();
        services.AddScoped<CustomResolver>();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.That(services.Any(d => d.ServiceType.IsGenericType &&
            d.ServiceType.GetGenericTypeDefinition() == typeof(IBotCommandResolver<>)), Is.False);

        await scope.ServiceProvider.GetRequiredService<CustomResolver>().Handle(
            new CustomContext("/core_boundary@literal payload"));
        Assert.That(scope.ServiceProvider.GetRequiredService<CustomCommand>().Calls, Is.EqualTo(1));
    }

    private static bool IsMessengerName(string name) => name.Contains("Telegram", StringComparison.OrdinalIgnoreCase) ||
        name.StartsWith("Max.", StringComparison.OrdinalIgnoreCase) || name.StartsWith("Bot.Commands.Max", StringComparison.OrdinalIgnoreCase);

    public sealed class CustomResolver(CommandExecutionService executor, SessionManager sessions,
        IAuthProvider auth, IBotProfile profile) : CommandServiceBase(executor, sessions, auth, profile)
    {
        public Task Handle(CustomContext context) => QueryHandler(context);
    }

    public sealed class CustomContext(string text) : QueryContext<string>(text)
    {
        public override long GetChatId() => 10;
        public override long GetFromId() => 20;
        public override string GetData() => Query;
        public override bool IsGroupMessage() => true;
        public override bool IsEvent() => false;
        public override EventType? GetEventType() => null;
        public override ChatArea GetChatType() => ChatArea.Group;
        public override string GetMessageId() => "native-id";
    }

    [Command(Name = "core_boundary@literal", Authorized = false, Area = ChatArea.Group)]
    public sealed class CustomCommand : IQueryCommand<string>
    {
        public int Calls { get; private set; }
        public Task<ICommandExecutionResult> Execute(QueryContext<string> query)
        {
            Calls++;
            return Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.Freeze());
        }
    }
}
