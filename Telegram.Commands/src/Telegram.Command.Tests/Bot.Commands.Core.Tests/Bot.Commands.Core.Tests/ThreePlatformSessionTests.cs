using System.Text.Json;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
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

namespace Bot.Commands.Core.Tests;

public class ThreePlatformSessionTests
{
    [Test]
    public async Task InterleavedFluentSessionsPersistUnderDistinctBotNamesWithSameUserAndChat()
    {
        var connectionString = $"Data Source=three-bots-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        var config = new ConfigurationBuilder().AddConfiguration(VkWebhookTests.Config())
            .AddInMemoryCollection(new Dictionary<string, string> { ["ConnectionStrings:Sessions"] = connectionString }).Build();
        var services = new ServiceCollection();
        new Startup(config).ConfigureServices(services);
        services.AddLogging();
        var sent = new List<(string Bot, string Text)>();
        services.AddScoped<ISampleMessageSender>(sp =>
        {
            var name = sp.GetRequiredService<IBotProfile>().BotName;
            var sender = new Mock<ISampleMessageSender>();
            sender.Setup(s => s.Send(It.IsAny<long>(), It.IsAny<IBotMessage>()))
                .Callback<long, IBotMessage>((_, message) => sent.Add((name, message.Text))).Returns(Task.CompletedTask);
            return sender.Object;
        });
        using var provider = services.BuildServiceProvider();
        var accessor = provider.GetRequiredService<IHttpContextAccessor>();
        accessor.HttpContext = new DefaultHttpContext();
        using (var scope = provider.CreateScope())
            await scope.ServiceProvider.GetRequiredService<EfDbContext>().Database.EnsureCreatedAsync();
        var platforms = new[] { SamplePlatform.Telegram, SamplePlatform.Max, SamplePlatform.Vk };
        void Select(SamplePlatform platform) => accessor.HttpContext.Request.Path =
            "/" + BotRuntime.ReadProfile(config, platform).UpdateRoute;

        async Task Send(SamplePlatform platform, string text)
        {
            Select(platform);
            using var scope = provider.CreateScope();
            switch (platform)
            {
                case SamplePlatform.Telegram:
                    await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<global::Telegram.Bot.Types.Update>>().Handle(new()
                    {
                        Message = new global::Telegram.Bot.Types.Message { Text = text,
                            From = new global::Telegram.Bot.Types.User { Id = 2 },
                            Chat = new global::Telegram.Bot.Types.Chat { Id = 1, Type = global::Telegram.Bot.Types.Enums.ChatType.Private } }
                    });
                    break;
                case SamplePlatform.Max:
                    var max = JsonSerializer.Deserialize<global::Max.Bot.Types.Update>(JsonSerializer.Serialize(new
                    {
                        update_type = "message_created",
                        message = new { sender = new { user_id = 2, name = "test" },
                            recipient = new { chat_id = 1, chat_type = "dialog" }, body = new { mid = "mid", text } }
                    }));
                    await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<global::Max.Bot.Types.Update>>().Handle(max);
                    break;
                case SamplePlatform.Vk:
                    await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<VkNet.Model.GroupUpdate>>().Handle(new()
                    {
                        Type = new(VkNet.Enums.StringEnums.GroupUpdateType.MessageNew),
                        Instance = new VkNet.Model.MessageNew { Message = new VkNet.Model.Message { PeerId = 1, FromId = 2, Text = text } }
                    });
                    break;
            }
        }
        foreach (var platform in platforms) await Send(platform, "/myfluent");
        foreach (var platform in platforms) await Send(platform, platform.ToString());
        foreach (var platform in platforms)
        {
            Select(platform);
            using var scope = provider.CreateScope();
            var saved = scope.ServiceProvider.GetRequiredService<ISessionsStore>()
                .GetSessionInfoWithData<FluentObject<MyObject, States>>(DateTime.Now, 1, 2, "/myfluent");
            Assert.That(saved.Data.Object.FirstName, Is.EqualTo(platform.ToString()));
            Assert.That(saved.Data.CurrentStateId.Data, Is.EqualTo(States.Surname));
        }
        // Complete VK first: the other platforms must remain active.
        await Send(SamplePlatform.Vk, "User");
        foreach (var platform in platforms)
        {
            Select(platform);
            using var scope = provider.CreateScope();
            var saved = scope.ServiceProvider.GetRequiredService<ISessionsStore>().GetSessionInfo(DateTime.Now, 1, 2);
            Assert.That(saved is null, Is.EqualTo(platform == SamplePlatform.Vk));
        }
        await Send(SamplePlatform.Telegram, "User");
        await Send(SamplePlatform.Max, "User");
        Assert.That(sent, Does.Contain(("vk_sample", "Your data: Vk User")));
        Assert.That(sent, Does.Contain(("telegram_sample", "Your data: Telegram User")));
        Assert.That(sent, Does.Contain(("max_sample", "Your data: Max User")));
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT BOT_NAME, ID_TELEGRAM_CHAT, ID_TELEGRAM_USER FROM COMMAND_SESSION";
        using var reader = await command.ExecuteReaderAsync();
        var names = new List<string>();
        while (await reader.ReadAsync())
        {
            names.Add(reader.GetString(0));
            Assert.That(reader.GetInt64(1), Is.EqualTo(1));
            Assert.That(reader.GetInt64(2), Is.EqualTo(2));
        }
        Assert.That(names, Is.EquivalentTo(new[] { "telegram_sample", "max_sample", "vk_sample" }));
    }
}
