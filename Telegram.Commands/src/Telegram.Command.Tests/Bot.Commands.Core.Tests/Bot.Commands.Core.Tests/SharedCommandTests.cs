using Moq;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Services;
using Bot.Commands.Core.Tests.Mocks;
using Bot.Commands.Telegram.Models;

namespace Bot.Commands.Core.Tests;

public class SharedCommandTests
{
    [Test]
    public async Task OneSessionCommandCanImplementBothMessengersExplicitly()
    {
        var command = new SharedSessionCommand();
        var factory = new Mock<ICommandFactory>();
        factory.Setup(f => f.GetCommand(typeof(SharedSessionCommand))).Returns(command);
        var store = new SessionStoreMock();
        store.SetSessionInfoWithData(new CommandSession { Data = "saved" });
        var executor = new CommandExecutionService(factory.Object, new SessionManager(Mock.Of<IClock>(), store));
        var descriptor = CommandDescriptorComposition.CreateSessionResult(new FullCommandDescriptor(typeof(SharedSessionCommand)));
        await executor.Execute(descriptor, new TelegramMessageQueryContext(new global::Telegram.Bot.Types.Message
        {
            Chat = new global::Telegram.Bot.Types.Chat { Id = 1 }, From = new global::Telegram.Bot.Types.User { Id = 2 }
        }));
        await executor.Execute(descriptor, new global::Bot.Commands.Max.Models.MessageCreatedQueryContext(
            new global::Max.Bot.Types.Chat { ChatId = 1 }, new global::Max.Bot.Types.Message { Sender = new global::Max.Bot.Types.User { Id = 2 } }));
        Assert.That(command.Calls, Is.EqualTo(new[] { "telegram:saved", "max:saved" }));
    }

    [TestCase("/start@bot argument /start", "start@bot", "argument /start")]
    [TestCase("/start\nargument", "start", "argument")]
    [TestCase("/", "", "")]
    public void CommandArgumentsExcludeTheWholeToken(string text, string command, string data)
    {
        var context = new TelegramMessageQueryContext(new global::Telegram.Bot.Types.Message { Text = text });
        Assert.That(BotCommandExtensions.ExtractCommand(text), Is.EqualTo(command));
        Assert.That(context.ExtractData(typeof(SharedSessionCommand)), Is.EqualTo(data));
    }

    [Command(Name = "shared_session_test", Authorized = false)]
    public class SharedSessionCommand : ISessionCommand<global::Telegram.Bot.Types.Message, string>, ISessionCommand<global::Max.Bot.Types.Message, string>
    {
        public List<string> Calls { get; } = new();
        Task<ICommandExecutionResult> ISessionCommand<global::Telegram.Bot.Types.Message, string>.Execute(
            IQueryContext<global::Telegram.Bot.Types.Message> query, string data)
        {
            Calls.Add("telegram:" + data);
            return Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.Break());
        }
        Task<ICommandExecutionResult> ISessionCommand<global::Max.Bot.Types.Message, string>.Execute(
            IQueryContext<global::Max.Bot.Types.Message> query, string data)
        {
            Calls.Add("max:" + data);
            return Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.Break());
        }
    }
}
