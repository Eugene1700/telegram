using Bot.Commands.Abstract.Interfaces;
using EntityStorage;
using IClock = Bot.Commands.Abstract.Interfaces.IClock;
using Microsoft.AspNetCore.Http;
using Moq;
using SimpleHandlers;
using SimpleHandlers.Domain;
using SimpleHandlers.Services;

namespace Bot.Commands.Core.Tests;

public class PlatformSessionStoreTests
{
    [Test]
    public async Task SameIdsHaveIndependentSessionsAndUpdatesTargetTheirPlatform()
    {
        var rows = new List<CommandSession>();
        var storage = new Mock<IEntityStorage>();
        storage.Setup(s => s.Select<CommandSession>()).Returns(() => rows.AsQueryable());
        storage.Setup(s => s.Create(It.IsAny<CommandSession>()))
            .Callback<CommandSession>(rows.Add).ReturnsAsync(1L);
        var now = new DateTime(2026, 11, 4, 12, 0, 0);
        var clock = new Mock<IClock>();
        clock.SetupGet(c => c.Now).Returns(now);
        var telegram = Store(storage.Object, clock.Object, false);
        var max = Store(storage.Object, clock.Object, true);

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
            It.Is<CommandSession>(r => r.Platform == BotPlatform.Max),
            It.IsAny<System.Linq.Expressions.Expression<Func<CommandSession, CommandSession>>>()), Times.Once);
        Assert.That(rows.Single(r => r.Platform == BotPlatform.Telegram).SessionData, Is.EqualTo("\"telegram\""));
    }

    [Test]
    public void DefaultPlatformIsTelegramAndExpiredSessionsAreExcluded()
    {
        var now = new DateTime(2026, 11, 4);
        var row = new CommandSession { ChatId = 1, UserId = 2, CommandQuery = "/ask", ExpiredAt = now.AddHours(1) };
        var storage = new Mock<IEntityStorage>();
        storage.Setup(s => s.Select<CommandSession>()).Returns(new[] { row }.AsQueryable());
        var clock = Mock.Of<IClock>();
        Assert.That(row.Platform, Is.EqualTo(BotPlatform.Telegram));
        Assert.That(Store(storage.Object, clock, false).GetSessionInfo(now, 1, 2), Is.Not.Null);
        Assert.That(Store(storage.Object, clock, true).GetSessionInfo(now, 1, 2), Is.Null);
        Assert.That(Store(storage.Object, clock, false).GetSessionInfo(now.AddHours(1), 1, 2), Is.Null);
    }

    private static SessionStore Store(IEntityStorage storage, IClock clock, bool max)
    {
        var http = new DefaultHttpContext();
        http.Request.Path = "/" + (max ? Settings.MaxRouteToUpdate : Settings.RouteToUpdate);
        return new SessionStore(storage, clock, new BotRuntime(new HttpContextAccessor { HttpContext = http }));
    }
}
