using System.Text.Json;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Fluent;
using EntityStorage.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SimpleHandlers;
using SimpleHandlers.Services;
using SimpleHandlers.Services.Commands;
using Mx = global::Max.Bot.Types;

namespace Bot.Commands.Core.Tests;

public class FluentPersistenceTests
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task MyFluentSessionPersistsAcrossRequestScopes(bool max)
    {
        var connectionString = $"Data Source=fluent-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["BotProfile:Enabled"] = "true",
            ["BotProfile:BotName"] = "telegram-test",
            ["BotProfile:RegisterWebhook"] = "false",
            ["MaxBotProfile:Enabled"] = "true",
            ["MaxBotProfile:BotName"] = "max-test",
            ["MaxBotProfile:RegisterWebhook"] = "false",
            ["ConnectionStrings:Sessions"] = connectionString
        }).Build();
        var services = new ServiceCollection();
        new Startup(config).ConfigureServices(services);
        services.AddLogging();
        var sender = new Mock<ISampleMessageSender>();
        sender.Setup(s => s.Send(It.IsAny<long>(), It.IsAny<Bot.Commands.Abstract.Messages.IBotMessage>()))
            .Returns(Task.CompletedTask);
        services.AddSingleton(sender.Object);
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IHttpContextAccessor>().HttpContext = new DefaultHttpContext();
        provider.GetRequiredService<IHttpContextAccessor>().HttpContext.Request.Path = "/" + (max ? Settings.MaxRouteToUpdate : Settings.RouteToUpdate);
        using (var scope = provider.CreateScope())
            await scope.ServiceProvider.GetRequiredService<EfDbContext>().Database.EnsureCreatedAsync();

        async Task Send(string text)
        {
            using var scope = provider.CreateScope();
            var update = JsonSerializer.Deserialize<Mx.Update>(JsonSerializer.Serialize(new
            {
                update_type = "message_created",
                message = new
                {
                    sender = new { user_id = 2, name = "test" },
                    recipient = new { chat_id = 1, chat_type = "dialog" },
                    body = new { mid = "mid", text }
                }
            }));
            if (max)
                await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<Mx.Update>>().Handle(update);
            else
                await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<global::Telegram.Bot.Types.Update>>().Handle(
                    new global::Telegram.Bot.Types.Update
                    {
                        Message = new global::Telegram.Bot.Types.Message
                        {
                            Text = text,
                            From = new global::Telegram.Bot.Types.User { Id = 2 },
                            Chat = new global::Telegram.Bot.Types.Chat { Id = 1, Type = global::Telegram.Bot.Types.Enums.ChatType.Private }
                        }
                    });
        }
        async Task AssertStoredIdentity()
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT BOT_NAME, ID_TELEGRAM_CHAT, ID_TELEGRAM_USER FROM COMMAND_SESSION";
            using var reader = await command.ExecuteReaderAsync();
            Assert.That(await reader.ReadAsync(), Is.True);
            Assert.That(reader.GetString(0), Is.EqualTo(max ? "max-test" : "telegram-test"));
            Assert.That(reader.GetInt64(1), Is.EqualTo(1));
            Assert.That(reader.GetInt64(2), Is.EqualTo(2));
            Assert.That(await reader.ReadAsync(), Is.False, "The flow must update the same session row.");
        }
        await Send("/myfluent");
        await AssertStoredIdentity();
        using (var scope = provider.CreateScope())
        {
            var session = scope.ServiceProvider.GetRequiredService<ISessionsStore>()
                .GetSessionInfoWithData<FluentObject<MyObject, States>>(DateTime.Now, 1, 2, "/myfluent");
            Assert.That(session, Is.Not.Null, "The session must be committed before the webhook scope ends.");
            Assert.That(session.Data.CurrentStateId.Data, Is.EqualTo(States.Name));
        }
        await Send("Ada");
        await AssertStoredIdentity();
        using (var scope = provider.CreateScope())
        {
            var session = scope.ServiceProvider.GetRequiredService<ISessionsStore>()
                .GetSessionInfoWithData<FluentObject<MyObject, States>>(DateTime.Now, 1, 2, "/myfluent");
            Assert.That(session.Data.Object.FirstName, Is.EqualTo("Ada"));
            Assert.That(session.Data.CurrentStateId.Data, Is.EqualTo(States.Surname));
        }
        await Send("Lovelace");
        await AssertStoredIdentity();
        using (var scope = provider.CreateScope())
            Assert.That(scope.ServiceProvider.GetRequiredService<ISessionsStore>()
                .GetSessionInfo(DateTime.Now, 1, 2), Is.Null);
    }
}
