using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Bot.Commands.Abstract.Interfaces;
using EntityStorage;
using IClock = Bot.Commands.Abstract.Interfaces.IClock;
using Microsoft.AspNetCore.Http;
using Moq;
using SimpleHandlers;
using SimpleHandlers.Domain;
using SimpleHandlers.Services;

namespace Bot.Commands.Core.Tests;

public class BotSessionStoreTests
{
    [Test]
    public async Task InterleavedChainsOfTwoBotsOnSamePlatformStayWithTheirBot()
    {
        var rows = new List<CommandSession>();
        var storage = new Mock<IEntityStorage>();
        storage.Setup(s => s.Select<CommandSession>()).Returns(() => rows.AsQueryable());
        storage.Setup(s => s.Create(It.IsAny<CommandSession>())).Callback<CommandSession>(rows.Add).ReturnsAsync(1L);
        storage.Setup(s => s.UpdateSingle(It.IsAny<CommandSession>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<CommandSession, CommandSession>>>()))
            .Callback<CommandSession, System.Linq.Expressions.Expression<Func<CommandSession, CommandSession>>>((row, update) =>
            {
                var changed = update.Compile()(row);
                row.ChatId = changed.ChatId;
                row.CommandQuery = changed.CommandQuery;
                row.ExpiredAt = changed.ExpiredAt;
                row.SessionData = changed.SessionData;
            }).Returns(Task.CompletedTask);
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string> { ["BotProfile:BotName"] = "bot-a", ["BotProfile:RegisterWebhook"] = "false" }).Build();
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        new Startup(configuration).ConfigureServices(services);
        services.AddSingleton(storage.Object);
        var botName = "bot-a";
        services.AddScoped<IBotProfile>(_ => new BotProfile { BotName = botName, Swarms = new[] { "telegram" } });
        var sent = new List<(string Bot, string Text)>();
        services.AddScoped<ISampleMessageSender>(sp =>
        {
            var name = sp.GetRequiredService<IBotProfile>().BotName;
            var sender = new Mock<ISampleMessageSender>();
            sender.Setup(s => s.Send(It.IsAny<long>(), It.IsAny<Bot.Commands.Abstract.Messages.IBotMessage>()))
                .Callback<long, Bot.Commands.Abstract.Messages.IBotMessage>((_, m) => sent.Add((name, m.Text))).Returns(Task.CompletedTask);
            return sender.Object;
        });
        using var provider = services.BuildServiceProvider();
        async Task Send(string bot, string text)
        {
            botName = bot;
            using var scope = provider.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<global::Telegram.Bot.Types.Update>>()
                .Handle(new global::Telegram.Bot.Types.Update { Message = new global::Telegram.Bot.Types.Message
                {
                    Chat = new global::Telegram.Bot.Types.Chat { Id = 1, Type = global::Telegram.Bot.Types.Enums.ChatType.Private },
                    From = new global::Telegram.Bot.Types.User { Id = 2 }, Text = text
                }});
        }
        await Send("bot-a", "/ask");
        await Send("bot-b", "/profile");
        Assert.That(rows.Select(r => r.BotName), Is.EquivalentTo(new[] { "bot-a", "bot-b" }));
        await Send("bot-a", "Ada");
        Assert.That(sent.Last(), Is.EqualTo(("bot-a", "Nice to meet you, Ada!")));
        Assert.That(rows.Single(r => r.BotName == "bot-b").ExpiredAt, Is.Null);
        await Send("bot-b", "Grace");
        Assert.That(sent.Last(), Is.EqualTo(("bot-b", "Profile saved: Grace")));
        Assert.That(rows.All(r => r.ExpiredAt <= DateTime.Now), Is.True);
    }

    [Test]
    public async Task SameIdsHaveIndependentSessionsAndUpdatesTargetTheirBot()
    {
        var rows = new List<CommandSession>();
        var storage = new Mock<IEntityStorage>();
        storage.Setup(s => s.Select<CommandSession>()).Returns(() => rows.AsQueryable());
        storage.Setup(s => s.Create(It.IsAny<CommandSession>()))
            .Callback<CommandSession>(rows.Add).ReturnsAsync(1L);
        var now = new DateTime(2026, 11, 4, 12, 0, 0);
        var clock = new Mock<IClock>();
        clock.SetupGet(c => c.Now).Returns(now);
        var telegram = Store(storage.Object, clock.Object, "telegram-one");
        var max = Store(storage.Object, clock.Object, "telegram-two");

        CourseSessionDescriptor<string> Session(string value) => new()
        {
            ChatId = 1, UserId = 2, CommandQuery = "/profile",
            OpenedAt = now, ExpiredAt = now.AddHours(1), Data = value
        };
        await telegram.CreateSession(Session("telegram"));
        await max.CreateSession(Session("max"));
        Assert.That(telegram.GetSessionInfoWithData<string>(now, 1, 2, "/profile").Data, Is.EqualTo("telegram"));
        Assert.That(max.GetSessionInfoWithData(now, 1, 2, "/profile", typeof(string)).Data, Is.EqualTo("max"));
        Assert.That(max.GetSessionInfoWithData<string>(now, 1, 2, "/other"), Is.Null);
        await max.UpdateSession(Session("updated"), 1);
        storage.Verify(s => s.UpdateSingle(
            It.Is<CommandSession>(r => r.BotName == "telegram-two"),
            It.IsAny<System.Linq.Expressions.Expression<Func<CommandSession, CommandSession>>>()), Times.Once);
        Assert.That(rows.Single(r => r.BotName == "telegram-one").SessionData, Is.EqualTo("\"telegram\""));
    }

    [Test]
    public void OtherBotsAndExpiredSessionsAreExcluded()
    {
        var now = new DateTime(2026, 11, 4);
        var row = new CommandSession { BotName = "telegram-one", ChatId = 1, UserId = 2, CommandQuery = "/ask", ExpiredAt = now.AddHours(1) };
        var storage = new Mock<IEntityStorage>();
        storage.Setup(s => s.Select<CommandSession>()).Returns(new[] { row }.AsQueryable());
        var clock = Mock.Of<IClock>();

        Assert.That(Store(storage.Object, clock, "telegram-one").GetSessionInfo(now, 1, 2), Is.Not.Null);
        Assert.That(Store(storage.Object, clock, "telegram-two").GetSessionInfo(now, 1, 2), Is.Null);
        Assert.That(Store(storage.Object, clock, "telegram-one").GetSessionInfo(now.AddHours(1), 1, 2), Is.Null);
    }

    private static SessionStore Store(IEntityStorage storage, IClock clock, string name)
        => new SessionStore(storage, clock, new BotProfile { BotName = name });
}
