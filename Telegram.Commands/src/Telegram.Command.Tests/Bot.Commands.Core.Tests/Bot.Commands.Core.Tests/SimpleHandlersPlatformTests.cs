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
    [TestCase(false)]
    [TestCase(true)]
    public async Task SharedCommandsHandleMessagesSessionsAndCallbacks(bool max)
    {
        var configuration = Configuration();
        var services = new ServiceCollection();
        new Startup(configuration).ConfigureServices(services);
        services.AddLogging();
        var sent = new RecordingSender();
        services.AddSingleton<ISampleMessageSender>(sent);
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

        async Task Send(string text, bool callback = false)
        {
            // Each update gets a fresh DI scope, just like a webhook request.
            using var scope = provider.CreateScope();
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
                await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<Mx.Update>>().Handle(JsonSerializer.Deserialize<Mx.Update>(json));
            }
            else
            {
                var message = new Tg.Message { Chat = new Tg.Chat { Id = 1, Type = global::Telegram.Bot.Types.Enums.ChatType.Private }, From = new Tg.User { Id = 2 }, Text = text };
                var update = callback
                    ? new Tg.Update { CallbackQuery = new Tg.CallbackQuery { Id = "cb", From = message.From, Message = message, Data = text } }
                    : new Tg.Update { Message = message };
                await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<Tg.Update>>().Handle(update);
            }
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
        var payload = sent.Messages.Last().Callbacks.Single().CallbackText;
        await Send(payload, callback: true);
        Assert.That(sent.Messages.Last().Text, Is.EqualTo("Profile saved: Alex"));
        using var check = provider.CreateScope();
        Assert.That(check.ServiceProvider.GetRequiredService<ISessionsStore>().GetSessionInfo(DateTime.Now, 1, 2), Is.Null);
        if (max)
            await Assert.ThrowsAsync<Bot.Commands.Core.Exceptions.CommandExtractionException>(() => Send("/myfluent"));
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
        Assert.That(scope.ServiceProvider.GetRequiredService<BotRuntime>().Platform, Is.EqualTo(SimpleHandlers.Domain.BotPlatform.Max));
        accessor.HttpContext.Request.Path = "/" + Settings.RouteToUpdate;
        using var telegram = provider.CreateScope();
        Assert.That(telegram.ServiceProvider.GetRequiredService<IBotProfile>().BotName, Is.EqualTo("telegram_sample"));
        Assert.That(telegram.ServiceProvider.GetRequiredService<BotRuntime>().Platform, Is.EqualTo(SimpleHandlers.Domain.BotPlatform.Telegram));
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
        public Task Send(long chatId, IBotMessage message)
        {
            Assert.That(chatId, Is.EqualTo(1));
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }
}
