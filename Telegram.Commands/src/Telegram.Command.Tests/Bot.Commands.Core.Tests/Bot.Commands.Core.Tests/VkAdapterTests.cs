using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Services;
using Bot.Commands.Core.Tests.Mocks;
using Bot.Commands.DependencyInjection;
using Bot.Commands.Vk;
using Bot.Commands.Vk.Models;
using Bot.Commands.Vk.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VkNet.Enums.StringEnums;
using VkNet.Model;
using CommandSession = Bot.Commands.Core.Models.CommandSession;

namespace Bot.Commands.Core.Tests;

public class VkAdapterTests
{
    private ServiceProvider _provider;
    private IServiceScope _scope;
    private IBotCommandResolver<GroupUpdate> Resolver => _scope.ServiceProvider.GetRequiredService<IBotCommandResolver<GroupUpdate>>();

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IClock>(c => c.Now == DateTime.UtcNow));
        services.AddSingleton(Mock.Of<IAuthProvider>());
        services.AddSingleton(Mock.Of<IBotProfile>(p => p.BotName == "vk-test"));
        services.AddScoped<SessionStoreMock>();
        services.AddScoped<ISessionsStore>(sp => sp.GetRequiredService<SessionStoreMock>());
        services.AddSingleton<ICommandMatcher<Message>, GreetingMatcher>();
        services.UseVkCommandsServices();
        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateScope();
    }

    [TearDown]
    public void Cleanup()
    {
        _scope.Dispose();
        _provider.Dispose();
    }

    [TestCase(42, ChatArea.Private)]
    [TestCase(2000000001, ChatArea.Group)]
    public async Task RealMessageNewJsonRoutesWithoutTelegramMention(long peer, ChatArea area)
    {
        var update = Incoming("/vk_adapter_test argument", peer);
        await Resolver.Handle(update);
        var query = _scope.ServiceProvider.GetRequiredService<TextCommand>().Last;
        Assert.That(query.Query, Is.SameAs(((MessageNew)update.Instance).Message));
        Assert.That(query.GetChatId(), Is.EqualTo(peer));
        Assert.That(query.GetFromId(), Is.EqualTo(42));
        Assert.That(query.GetChatType(), Is.EqualTo(area));
        Assert.That(query.IsGroupMessage(), Is.EqualTo(area == ChatArea.Group));
        Assert.That(query.GetMessageId(), Is.EqualTo("7"));
        Assert.That(query.GetData(), Is.EqualTo("/vk_adapter_test argument"));
    }

    [Test]
    public async Task LegacyMessageShapeAndMatcherAreSupported()
    {
        var update = JsonConvert.DeserializeObject<GroupUpdate>("""
            {"type":"message_new","group_id":1,"object":{"peer_id":42,"from_id":42,"text":"hello"}}
            """);
        await Resolver.Handle(update);
        Assert.That(_scope.ServiceProvider.GetRequiredService<TextCommand>().Last.GetData(), Is.EqualTo("hello"));
    }

    [Test]
    public async Task KeyboardPayloadSurvivesWireFormatAndRoutesTypedCallback()
    {
        var keyboard = new VkInlineMarkupQueryBuilder()
            .AddInlineKeyboardButton<CallbackCommand>(new CallbackData { Text = "Go", CallbackText = "quoted \"value\" / имя" })
            .GetResult();
        var action = JObject.Parse(JsonConvert.SerializeObject(keyboard))["buttons"][0][0]["action"];
        Assert.That(action["type"].Value<string>(), Is.EqualTo("callback"));
        var update = Callback(action["payload"].Value<string>());
        await Resolver.Handle(update);
        var query = _scope.ServiceProvider.GetRequiredService<CallbackCommand>().Last;
        Assert.That(query.Query, Is.SameAs(update.Instance));
        Assert.That(query.GetData(), Is.EqualTo("/vk_adapter_test quoted \"value\" / имя"));
        Assert.That(query.GetChatId(), Is.EqualTo(2000000001));
        Assert.That(query.GetFromId(), Is.EqualTo(42));
        Assert.That(query.GetMessageId(), Is.EqualTo("7"));
        Assert.That(query.GetChatType(), Is.EqualTo(ChatArea.Group));
        Assert.That(query.IsEvent(), Is.False);
    }

    [TestCase("message_edit")]
    [TestCase("message_reply")]
    [TestCase("confirmation")]
    public async Task NonIncomingUpdatesDoNotExecuteCommands(string type)
    {
        var update = Incoming("/vk_adapter_test");
        update.Type = new UpdateType(type == "message_edit" ? GroupUpdateType.MessageEdit :
            type == "message_reply" ? GroupUpdateType.MessageReply : GroupUpdateType.Confirmation);
        await Resolver.Handle(update);
        Assert.That(_scope.ServiceProvider.GetRequiredService<TextCommand>().Last, Is.Null);
    }

    [Test]
    public async Task ActiveSessionReceivesTextAndServiceActionsAreIgnored()
    {
        var store = _scope.ServiceProvider.GetRequiredService<SessionStoreMock>();
        var session = new CommandSession { ChatId = 42, UserId = 42, CommandQuery = "/vk_session_test", Data = "stored" };
        store.SetSessionInfo(session);
        store.SetSessionInfoWithData(session);
        var update = Incoming("reply");
        ((MessageNew)update.Instance).Message.Action = new MessageActionObject();
        await Resolver.Handle(update);
        Assert.That(_scope.ServiceProvider.GetRequiredService<SessionCommand>().Received, Is.Null);
        ((MessageNew)update.Instance).Message.Action = null;
        await Resolver.Handle(update);
        Assert.That(_scope.ServiceProvider.GetRequiredService<SessionCommand>().Received, Is.EqualTo("stored:reply"));
    }

    [Test]
    public void CommonKeyboardPreservesRowsLinksAndRejectsUnsupportedModes()
    {
        var builder = new InlineMarkupQueryBuilder();
        builder.InlineKeyboardButtonsRow(new[] { Button("a"), Button("b") });
        builder.InlineKeyboardButtonsRow(new[] { Button("site") with { CallbackMode = CallbackMode.Url, CallbackText = "https://example.com/" } });
        var keyboard = new BotMessage("menu", null, null, builder.GetCallbacks()).ToInlineKeyboard();
        Assert.That(keyboard.Inline, Is.True);
        Assert.That(keyboard.OneTime, Is.False);
        Assert.That(keyboard.Buttons.Select(row => row.Count()), Is.EqualTo(new[] { 2, 1 }));
        Assert.That(keyboard.Buttons.Last().Single().Action.Link.AbsoluteUri, Is.EqualTo("https://example.com/"));
        Assert.That(keyboard.Buttons.Last().Single().Action.Type, Is.EqualTo(KeyboardButtonActionType.OpenLink));
        foreach (var mode in new[] { CallbackMode.Inline, CallbackMode.InlineCurrent, CallbackMode.Payment })
            Assert.Throws<NotSupportedException>(() => new VkInlineMarkupQueryBuilder().AddInlineKeyboardButton(Button("x") with { CallbackMode = mode }));
        Assert.That(new BotMessage("text", null, null, null).ToInlineKeyboard(), Is.Null);
    }

    [Test]
    public void PayloadOnlyOverridesTextForAdapterEnvelope()
    {
        var message = new Message { Text = "visible", Payload = "{\"other\":1}" };
        Assert.That(new VkMessageQueryContext(message).GetData(), Is.EqualTo("visible"));
        message.Payload = VkCallbackPayload.Encode("/vk_adapter_test");
        Assert.That(new VkMessageQueryContext(message).GetData(), Is.EqualTo("/vk_adapter_test"));
        var callback = new MessageEvent { Payload = "{\"custom\":1}" };
        Assert.That(new VkMessageEventContext(callback).GetData(), Is.EqualTo(callback.Payload));
    }

    [Test]
    public void MissingIdsFailAndMessageIdsAreNotConfusedWithGlobalIds()
    {
        var context = new VkMessageQueryContext(new Message { Id = 123 });
        Assert.Throws<InvalidOperationException>(() => context.GetChatId());
        Assert.Throws<InvalidOperationException>(() => context.GetFromId());
        Assert.That(context.GetMessageId(), Is.Null);
        var callback = new VkMessageEventContext(new MessageEvent());
        Assert.Throws<InvalidOperationException>(() => callback.GetChatId());
        Assert.Throws<InvalidOperationException>(() => callback.GetFromId());
    }

    [Test]
    public void PlainTextBuilderPreservesTextAndRejectsForeignFormats()
    {
        var message = new VkMessageBuilder().AppendPlain("a_b <b> *text*").GetMessages().Single();
        Assert.That(message.ToVkText(), Is.EqualTo("a_b <b> *text*"));
        Assert.Throws<NotSupportedException>(() => new BotMessage("text", null,
            new Bot.Commands.Max.MaxTextFormat(global::Max.Bot.Types.Enums.TextFormat.Markdown), null).ToVkText());
    }

    [TestCase("")]
    [TestCase("a&fid=foreign&fud=wrong")]
    [TestCase("100% + имя \"quoted\"")]
    public async Task FluentCallbackRoundTripThroughVkJson(string payload)
    {
        IBotMessage sent = null;
        string received = null;
        var builder = new StateMachineBuilder<object, int>();
        builder.State(0)
            .WithMessage<Message>((_, _) => Task.FromResult<ITextMessage>(new BotMessage("choose", null, null, null)),
                (_, _, _, message) => { sent = message; return Task.CompletedTask; })
            .WithCallbacks().Row().OnCallback<MessageEvent>(
                (_, _) => new CallbackData { Text = "Choose", CallbackText = payload },
                (_, _, _, value) => { received = value; return Task.FromResult(1); }, false);
        builder.Exit<MessageEvent>(1, (_, _, _) => Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.Break()));
        var machine = (StateMachine<object, int>)builder.Build();
        await machine.GetStateInternal(0).SendMessages(new VkMessageQueryContext(new Message { PeerId = 42, FromId = 42 }), null);
        var encoded = sent.ToInlineKeyboard().Buttons.Single().Single().Action.Payload;
        var context = new VkMessageEventContext((MessageEvent)Callback(encoded).Instance);
        var (next, _) = await machine.GetStateInternal(0).HandleQuery(context, null);
        Assert.That(next, Is.EqualTo(1));
        Assert.That(received, Is.EqualTo(payload));
    }

    private static CallbackDataWithCommand Button(string text) => new() { Text = text, CallbackText = text };
    private static GroupUpdate Incoming(string text, long peer = 42) =>
        JsonConvert.DeserializeObject<GroupUpdate>(JsonConvert.SerializeObject(new
        {
            type = "message_new", group_id = 1,
            @object = new { message = new { peer_id = peer, from_id = 42, id = 999, conversation_message_id = 7, text }, client_info = new { keyboard = true } }
        }));
    private static GroupUpdate Callback(string payload) =>
        JsonConvert.DeserializeObject<GroupUpdate>(JsonConvert.SerializeObject(new
        {
            type = "message_event", group_id = 1,
            @object = new { peer_id = 2000000001, user_id = 42, event_id = "event", conversation_message_id = 7, payload = JObject.Parse(payload) }
        }));

    private sealed class GreetingMatcher : ICommandMatcher<Message>
    {
        public Type CommandType => typeof(TextCommand);
        public bool IsMatch(IQueryContext<Message> query) => query.GetData() == "hello";
    }

    [Command(Name = "vk_adapter_test", Authorized = false, Area = ChatArea.Private | ChatArea.Group)]
    public sealed class TextCommand : IQueryCommand<Message>
    {
        public IQueryContext<Message> Last { get; private set; }
        public Task<ICommandExecutionResult> Execute(IQueryContext<Message> query)
        {
            Last = query;
            return Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.Freeze());
        }
    }

    [Command(Name = "vk_adapter_test", Authorized = false, Area = ChatArea.Private | ChatArea.Group)]
    public sealed class CallbackCommand : IQueryCommand<MessageEvent>
    {
        public IQueryContext<MessageEvent> Last { get; private set; }
        public Task<ICommandExecutionResult> Execute(IQueryContext<MessageEvent> query)
        {
            Last = query;
            return Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.Freeze());
        }
    }

    [Command(Name = "vk_session_test", Authorized = false)]
    public sealed class SessionCommand : ISessionCommand<Message, string>
    {
        public string Received { get; private set; }
        public Task<ICommandExecutionResult> Execute(IQueryContext<Message> query, string data)
        {
            Received = data + ":" + query.GetData();
            return Task.FromResult<ICommandExecutionResult>(CommandExecutionResult.Freeze());
        }
    }
}
