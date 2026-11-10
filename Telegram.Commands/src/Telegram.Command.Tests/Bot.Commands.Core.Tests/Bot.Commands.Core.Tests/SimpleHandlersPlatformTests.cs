using System.Text.Json;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Tests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleHandlers;
using SimpleHandlers.Services;
using Tg = global::Telegram.Bot.Types;
using Mx = global::Max.Bot.Types;

namespace Bot.Commands.Core.Tests;

public class SimpleHandlersPlatformTests
{
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public async Task SharedCommandsHandleMessagesSessionsAndCallbacks(bool max, bool useMatchers)
    {
        var configuration = Configuration();
        var services = new ServiceCollection();
        new Startup(configuration).ConfigureServices(services);
        if (!useMatchers)
        {
            services.RemoveAll<ICommandMatcher<Tg.Message>>();
            services.RemoveAll<ICommandMatcher<Mx.Message>>();
        }
        services.AddLogging();
        var sent = new RecordingSender();
        services.AddSingleton<ISampleMessageSender>(sent);
        var executed = new List<Type>();
        services.AddScoped<ICommandFactory>(sp =>
        {
            var factory = new Moq.Mock<ICommandFactory>();
            factory.Setup(f => f.GetCommand(Moq.It.IsAny<Type>())).Returns((Type type) =>
            {
                executed.Add(type);
                return sp.GetRequiredService(type);
            });
            return factory.Object;
        });
        {
            ISessionInfoWithData saved = null;
            var store = new SessionStoreMock();
            store.SetCreateSessionCallback(s => saved = s);
            store.SetUpdateSessionCallback((s, _) => saved = s);
            store.SetSessionInfoCallback((now, _, _) => saved?.ExpiredAt <= now ? null : saved);
            store.SetSessionInfoWithDataCallback((now, _, _, _, _) => saved?.ExpiredAt <= now ? null : saved);
            services.AddSingleton<ISessionsStore>(store);
        }
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IHttpContextAccessor>().HttpContext = new DefaultHttpContext();
        provider.GetRequiredService<IHttpContextAccessor>().HttpContext.Request.Path =
            "/" + (max ? Settings.MaxRouteToUpdate : Settings.RouteToUpdate);

        async Task Send(string text, bool callback = false, bool photo = false)
        {
            // Each update gets a fresh DI scope, just like a webhook request.
            using var scope = provider.CreateScope();
            Assert.That(scope.ServiceProvider.GetRequiredService<IBotProfile>().Swarms, Is.Null.Or.Empty);
            Assert.That(scope.ServiceProvider.GetRequiredService<IBotProfile>().Platform, Is.EqualTo(max ? "max" : "telegram"));
            if (max)
            {
                var json = callback
                    ? JsonSerializer.Serialize(new { update_type = "message_callback",
                        callback = new { callback_id = "cb", user = new { user_id = 2, name = "test" }, payload = text },
                        message = new { recipient = new { chat_id = 1, chat_type = "dialog" },
                            body = new { mid = "mid", text = "menu" } } })
                    : JsonSerializer.Serialize(new { update_type = "message_created",
                        message = new { sender = new { user_id = 2, name = "test" },
                            recipient = new { chat_id = 1, chat_type = "dialog" }, body = new { mid = "mid", text } } });
                if (photo) json = """
                    {"update_type":"message_created","message":{"sender":{"user_id":2,"name":"test"},
                    "recipient":{"chat_id":1,"chat_type":"dialog"},"body":{"mid":"photo-mid","attachments":[
                    {"type":"image","payload":{"photo_id":12,"url":"https://example.com/photo.jpg"}}]}}}
                    """;
                await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<Mx.Update>>().Handle(JsonSerializer.Deserialize<Mx.Update>(json));
            }
            else
            {
                var message = new Tg.Message { Chat = new Tg.Chat { Id = 1, Type = global::Telegram.Bot.Types.Enums.ChatType.Private }, From = new Tg.User { Id = 2 }, Text = text };
                if (photo) message.Photo = new[] { new Tg.PhotoSize { FileId = "photo", FileUniqueId = "photo", Width = 10, Height = 10, FileSize = 100 } };
                var update = callback
                    ? new Tg.Update { CallbackQuery = new Tg.CallbackQuery { Id = "cb", From = message.From, Message = message, Data = text } }
                    : new Tg.Update { Message = message };
                await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<Tg.Update>>().Handle(update);
            }
        }

        if (useMatchers)
        {
            await Assert.ThrowsAsync<Bot.Commands.Core.Exceptions.CommandExtractionException>(() => Send("/start"));
            await Send("register");
            await Send("Funnel User");
            Assert.That(sent.Messages.Last().Text, Is.EqualTo("Profile saved: Funnel User"));
            await Send("register");
            var callbackPayload = sent.Messages.Last().Callbacks.SelectMany(row => row).Single().CallbackText;
            await Send(callbackPayload, callback: true);
            Assert.That(sent.Messages.Last().Text, Is.EqualTo("Profile saved: Alex"));
            await Assert.ThrowsAsync<Bot.Commands.Core.Exceptions.CommandExtractionException>(() => Send("register", callback: true));
            Assert.That(executed.All(t => t.Name.StartsWith(max ? "Max" : "Telegram", StringComparison.Ordinal)), Is.True);
            return;
        }

        await Send("/start");
        Assert.That(sent.Messages.Last().Text, Does.Contain("/profile"));
        await Send("/ask");
        await Send("Ada");
        Assert.That(sent.Messages.Last().Text, Is.EqualTo("Nice to meet you, Ada!"));
        await Send("/profile");
        await Send("Grace");
        Assert.That(sent.Messages.Last().Text, Is.EqualTo("Profile saved: Grace"));
        await Send("/profile");
        var payload = sent.Messages.Last().Callbacks.SelectMany(row => row).Single().CallbackText;
        await Send(payload, callback: true);
        Assert.That(sent.Messages.Last().Text, Is.EqualTo("Profile saved: Alex"));
        async Task Click(string label)
        {
            var button = sent.Messages.AsEnumerable().Reverse().SelectMany(m => m.Callbacks?.SelectMany(row => row) ?? Enumerable.Empty<CallbackDataWithCommand>())
                .First(c => c.Text == label);
            await Send(button.CommandDescriptor == null ? button.CallbackText : $"/{button.CommandDescriptor.Name} {button.CallbackText}", callback: true);
        }
        foreach (var command in new[] { "/dropdown", "/dropdownmultiple" })
        {
            await Send(command);
            var dropdown = sent.Messages.Last().Callbacks.SelectMany(row => row).First();
            await Send($"/{dropdown.CommandDescriptor.Name} {dropdown.CallbackText}", callback: true);
            await Click("Item1");
            Assert.That(sent.Messages.Last().Callbacks.SelectMany(row => row).Any(c => c.Text == "✓ Item1"), Is.True);
            await Click("Cancel");
        }
        await Send("/sendphoto");
        await Send(null, photo: true);
        await Click("Next photo");
        await Send(null, photo: true);
        await Click("Show result");
        Assert.That(sent.Messages.Last().Text.Split("MessageId:").Length, Is.EqualTo(3));
        await Click("Cancel");
        foreach (var command in new[] { "/myfluent", "/otherfluent" })
        {
            await Send(command);
            await Send("Ada");
            await Send("Lovelace");
            Assert.That(sent.Messages.Last().Text, Is.EqualTo("Your data: Ada Lovelace"));
        }
        await Send("/myfluent");
        await Click("Default Name (Jack)");
        await Send("Smith");
        Assert.That(sent.Messages.Last().Text, Is.EqualTo("Your data: Jack Smith"));
        await Send("/myfluent");
        await Click("Cancel");
        Assert.That(sent.Messages.Last().Text, Is.EqualTo("Cancelled. Try /sendphoto or /dropdown."));
        using (var cancelled = provider.CreateScope())
            Assert.That(cancelled.ServiceProvider.GetRequiredService<ISessionsStore>()
                .GetSessionInfo(DateTime.Now, 1, 2), Is.Null);
        await Send("/global");
        await Send("toggle");
        await Send("intercept me");
        Assert.That(sent.Messages.Last().Text, Is.EqualTo("Intercepted message"));
        await Click("Stop");
        // A message-only funnel must not consume a callback with the same text.
        await Assert.ThrowsAsync<Bot.Commands.Core.Exceptions.CommandExtractionException>(
            () => Send("register", callback: true));
        Assert.That(executed.All(t => t.Name.StartsWith(max ? "Max" : "Telegram", StringComparison.Ordinal)), Is.True);
        using var check = provider.CreateScope();
        Assert.That(check.ServiceProvider.GetRequiredService<ISessionsStore>().GetSessionInfo(DateTime.Now, 1, 2), Is.Null);

    }

    [TestCase(false, false, 404)]
    [TestCase(true, false, 401)]
    [TestCase(true, true, 0)]
    public async Task MaxEndpointRejectsDisabledOrUnauthenticatedRequestsBeforeResolvingClient(bool enabled, bool validSecret, int status)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["MaxBotProfile:Enabled"] = enabled.ToString(), ["MaxBotProfile:WebhookSecret"] = "test-secret"
        }).Build();
        var http = new DefaultHttpContext();
        http.Request.Path = "/" + Settings.MaxRouteToUpdate;
        if (validSecret) http.Request.Headers["X-Max-Bot-Api-Secret"] = "test-secret";
        var filter = new WebhookAccessFilter(configuration, new BotRuntime(new HttpContextAccessor { HttpContext = http }));
        var action = new Microsoft.AspNetCore.Mvc.ActionContext(http, new Microsoft.AspNetCore.Routing.RouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
        var filters = new List<Microsoft.AspNetCore.Mvc.Filters.IFilterMetadata>();
        var context = new Microsoft.AspNetCore.Mvc.Filters.ResourceExecutingContext(action, filters,
            new List<Microsoft.AspNetCore.Mvc.ModelBinding.IValueProviderFactory>());
        var called = false;
        await filter.OnResourceExecutionAsync(context, () =>
        {
            called = true;
            return Task.FromResult(new Microsoft.AspNetCore.Mvc.Filters.ResourceExecutedContext(action, filters));
        });
        Assert.That(called, Is.EqualTo(status == 0));
        Assert.That((context.Result as Microsoft.AspNetCore.Mvc.StatusCodeResult)?.StatusCode ?? 0, Is.EqualTo(status));
    }

    [Test]
    public void BothPlatformsUseTheSameSessionStoreImplementation()
    {
        var services = new ServiceCollection();
        new Startup(Configuration()).ConfigureServices(services);
        using var provider = services.BuildServiceProvider();
        var accessor = provider.GetRequiredService<IHttpContextAccessor>();
        accessor.HttpContext = new DefaultHttpContext();
        accessor.HttpContext.Request.Path = "/" + Settings.MaxRouteToUpdate;
        using var scope = provider.CreateScope();
        var profile = scope.ServiceProvider.GetRequiredService<IBotProfile>();
        Assert.That(profile.BotName, Is.EqualTo("max_sample"));
        Assert.That(profile.UpdateRoute, Is.EqualTo(Settings.MaxRouteToUpdate));
        Assert.That(services.Single(s => s.ServiceType == typeof(ISessionsStore)).ImplementationType, Is.EqualTo(typeof(SessionStore)));
        accessor.HttpContext.Request.Path = "/" + Settings.RouteToUpdate;
        using var telegram = provider.CreateScope();
        Assert.That(telegram.ServiceProvider.GetRequiredService<IBotProfile>().BotName, Is.EqualTo("telegram_sample"));
    }

    private static IConfiguration Configuration() => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
    {
        ["BotProfile:Enabled"] = "true", ["BotProfile:RegisterWebhook"] = "false",
        ["BotProfile:BotName"] = "telegram_sample",
        ["MaxBotProfile:Enabled"] = "true", ["MaxBotProfile:RegisterWebhook"] = "false",
        ["MaxBotProfile:BotName"] = "max_sample"
    }).Build();

    private sealed class RecordingSender : ISampleMessageSender
    {
        public List<IBotMessage> Messages { get; } = new();
        public Task Edit(long chatId, string messageId, IBotMessage message) => Send(chatId, message);
        public Task Send(long chatId, IBotMessage message)
        {
            Assert.That(chatId, Is.EqualTo(1));
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }
}
