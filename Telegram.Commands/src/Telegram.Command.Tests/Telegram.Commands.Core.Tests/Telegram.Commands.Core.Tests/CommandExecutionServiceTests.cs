using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Core.Models;
using Telegram.Commands.Core.Services;
using Telegram.Commands.Core.Tests.Commands;
using Telegram.Commands.Core.Tests.Core;
using Telegram.Commands.Core.Tests.Environments;

namespace Telegram.Commands.Core.Tests;

public class Tests : TestsBase
{
    [ScopedInjection] private QueryMessageMockCommand _queryMessageMockCommand;
    [ScopedInjection] private ITelegramCommandFactory _telegramCommandFactory;
    [ScopedInjection] private CommandExecutionService _commandExecutionService;
    [ScopedInjection] private MessageEnvironment _messageEnvironment;

    [SetUp]
    public void Setup()
    {
        base.SetUp();
    }

    [Test]
    public async Task QueryCommandExecution()
    {
        var result = TelegramCommandExecutionResult.Break();

        var newMessage = _messageEnvironment.CreateMessage();
        var queryCommandDescriptor = new FullCommandDescriptor(_queryMessageMockCommand.GetType());
        _queryMessageMockCommand.SetResult(result, m =>
        {
            Assert.That(m.Text, Is.EqualTo(newMessage.Text));
            Assert.That(m.GetChatId(), Is.EqualTo(newMessage.Chat.Id));
        });
        var res = await _commandExecutionService.Execute(CommandDescriptorComposition.CreateQueryResult(queryCommandDescriptor),
            newMessage);

        Assert.That(res.Result, Is.EqualTo(ExecuteResult.Break));
    }

    protected override void RegisterServices(ServiceCollection serviceCollection)
    {
        serviceCollection.AddServices();
        serviceCollection.AddScoped<CommandExecutionService>();
    }
}