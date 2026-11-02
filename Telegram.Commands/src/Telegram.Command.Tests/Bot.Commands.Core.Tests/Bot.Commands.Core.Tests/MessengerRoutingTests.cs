using System.Text.Json;
using Bot.Commands.Max.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Tests.Mocks;
using Bot.Commands.DependencyInjection;
using Tg = global::Telegram.Bot.Types;
using MaxTypes = global::Max.Bot.Types;

namespace Bot.Commands.Core.Tests;

public class MessengerRoutingTests
{
    private ServiceProvider _provider;
    private IServiceScope _scope;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IClock>(Mock.Of<IClock>(c => c.Now == DateTime.UtcNow));
        services.AddScoped<ISessionsStore, SessionStoreMock>();
        services.AddSingleton(Mock.Of<IAuthProvider>());
        services.AddSingleton(Mock.Of<IBotProfile>(p => p.BotName == "testbot"));
        services.UseTelegramCommandsServices();
        services.UseMaxCommandsServices();
        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateScope();
    }

    [TearDown]
    public void Cleanup()
    {
        _scope.Dispose();
        _provider.Dispose();
    }

    [Test]
    public async Task SameNameSelectsCommandForItsMessenger()
    {
        await _scope.ServiceProvider.GetRequiredService<IBotCommandResolver<Tg.Update>>()
            .Handle(new Tg.Update { Message = TelegramMessage("/adapter_test") });
        await _scope.ServiceProvider.GetRequiredService<IBotCommandResolver<MaxTypes.Update>>()
            .Handle(MaxUpdate("/adapter_test"));
        Assert.That(_scope.ServiceProvider.GetRequiredService<TelegramTestCommand>().Calls, Is.EqualTo(1));
        Assert.That(_scope.ServiceProvider.GetRequiredService<MaxTestCommand>().Calls, Is.EqualTo(1));
    }

    [TestCase("/adapter_test@testbot argument", 1)]
    [TestCase("/adapter_test@other argument", 0)]
    [TestCase("/adapter_test", 0)]
    public async Task TelegramGroupChecksMentionOnCommandToken(string text, int expected)
    {
        var message = TelegramMessage(text);
        message.Chat.Type = global::Telegram.Bot.Types.Enums.ChatType.Group;
        await _scope.ServiceProvider.GetRequiredService<IBotCommandResolver<Tg.Update>>()
            .Handle(new Tg.Update { Message = message });
        Assert.That(_scope.ServiceProvider.GetRequiredService<TelegramTestCommand>().Calls, Is.EqualTo(expected));
    }

    [Test]
    public async Task MaxGroupDoesNotRequireTelegramMention()
    {
        await _scope.ServiceProvider.GetRequiredService<IBotCommandResolver<MaxTypes.Update>>()
            .Handle(MaxUpdate("/adapter_test argument", "chat"));
        Assert.That(_scope.ServiceProvider.GetRequiredService<MaxTestCommand>().Calls, Is.EqualTo(1));
    }

    [Test]
    public async Task GroupCallbackDoesNotRequireMention()
    {
        var message = TelegramMessage("");
        message.Chat.Type = global::Telegram.Bot.Types.Enums.ChatType.Group;
        await _scope.ServiceProvider.GetRequiredService<IBotCommandResolver<Tg.Update>>()
            .Handle(new Tg.Update { CallbackQuery = new Tg.CallbackQuery
            {
                Id = "callback", From = new Tg.User { Id = 2 }, Message = message,
                Data = "/adapter_test payload"
            }});
        Assert.That(_scope.ServiceProvider.GetRequiredService<TelegramCallbackCommand>().Calls, Is.EqualTo(1));
    }

    [TestCase("dialog", ChatArea.Private, false)]
    [TestCase("chat", ChatArea.Group, true)]
    [TestCase("channel", ChatArea.Channel, false)]
    public void MaxUsesRecipientWhenUpdateHasNoChat(string type, ChatArea area, bool group)
    {
        var update = MaxUpdate("hello", type);
        var context = new MessageCreatedQueryContext(update.Chat, update.Message);
        Assert.Multiple(() =>
        {
            Assert.That(context.GetChatId(), Is.EqualTo(1));
            Assert.That(context.GetFromId(), Is.EqualTo(2));
            Assert.That(context.GetChatType(), Is.EqualTo(area));
            Assert.That(context.IsGroupMessage(), Is.EqualTo(group));
            Assert.That(context.GetMessageId(), Is.EqualTo("mid"));
        });
    }

    [Test]
    public async Task EventWithoutHandlerIsIgnored()
    {
        var message = TelegramMessage(null);
        message.NewChatMembers = new[] { new Tg.User { Id = 3 } };
        await _scope.ServiceProvider.GetRequiredService<IBotCommandResolver<Tg.Update>>()
            .Handle(new Tg.Update { Message = message });
    }

    [Test]
    public void MaxCallbackUsesMessageRecipient()
    {
        var update = JsonSerializer.Deserialize<MaxTypes.Update>("""
            {"update_type":"message_callback","callback":{"callback_id":"cb","user":{"user_id":2,"name":"test"},"payload":"/adapter_test"},
             "message":{"sender":{"user_id":9,"name":"bot"},"recipient":{"chat_id":1,"chat_type":"chat"},"body":{"mid":"mid","text":"menu"}}}
            """);
        var context = new MessageCallbackContext(update.Chat, update.Callback, update.Message);
        Assert.That(context.GetChatId(), Is.EqualTo(1));
        Assert.That(context.GetFromId(), Is.EqualTo(2));
        Assert.That(context.GetChatType(), Is.EqualTo(ChatArea.Group));
        Assert.That(context.GetMessageId(), Is.EqualTo("mid"));
    }

    [Test]
    public void MaxUserAddedUsesTopLevelChatId()
    {
        var update = JsonSerializer.Deserialize<MaxTypes.Update>("""
            {"update_type":"user_added","chat_id":1,"user":{"user_id":2,"name":"test"}}
            """);
        var context = new UserAddedUpdateContext(update.UserAddedUpdate, update.ChatId);
        Assert.That(context.GetChatId(), Is.EqualTo(1));
        Assert.That(context.GetEventType(), Is.EqualTo(EventType.ChatMemberAdded));
    }

    private static Tg.Message TelegramMessage(string text) => new()
    {
        Chat = new Tg.Chat { Id = 1, Type = global::Telegram.Bot.Types.Enums.ChatType.Private },
        From = new Tg.User { Id = 2 }, Text = text
    };

    private static MaxTypes.Update MaxUpdate(string text, string chatType = "dialog") =>
        JsonSerializer.Deserialize<MaxTypes.Update>(JsonSerializer.Serialize(new
        {
            update_type = "message_created",
            message = new
            {
                sender = new { user_id = 2, name = "test" },
                recipient = new { chat_id = 1, chat_type = chatType },
                body = new { mid = "mid", text }
            }
        }));

    [Command(Name = "adapter_test", Authorized = false, Area = ChatArea.Private | ChatArea.Group)]
    public class TelegramTestCommand : IQueryCommand<Tg.Message>
    {
        public int Calls { get; private set; }
        public Task<ICommandExecutionResult> Execute(QueryContext<Tg.Message> query)
        {
            Calls++;
            return Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.Freeze());
        }
    }

    [Command(Name = "adapter_test", Authorized = false, Area = ChatArea.Private | ChatArea.Group)]
    public class MaxTestCommand : IQueryCommand<MaxTypes.Message>
    {
        public int Calls { get; private set; }
        public Task<ICommandExecutionResult> Execute(QueryContext<MaxTypes.Message> query)
        {
            Calls++;
            return Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.Freeze());
        }
    }

    [Command(Name = "adapter_test", Authorized = false, Area = ChatArea.Group)]
    public class TelegramCallbackCommand : IQueryCommand<Tg.CallbackQuery>
    {
        public int Calls { get; private set; }
        public Task<ICommandExecutionResult> Execute(QueryContext<Tg.CallbackQuery> query)
        {
            Calls++;
            return Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.Freeze());
        }
    }
}
