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
    [TestCase(SamplePlatform.Telegram)]
    [TestCase(SamplePlatform.Max)]
    [TestCase(SamplePlatform.Vk)]
    [TestCase(SamplePlatform.Instagram)]
    [TestCase(SamplePlatform.WhatsApp)]
    public async Task SharedCommandsHandleMessagesSessionsAndCallbacks(SamplePlatform platform)
    {
        var max = platform == SamplePlatform.Max;
        var vk = platform == SamplePlatform.Vk;
        var instagram = platform == SamplePlatform.Instagram;
        var whatsapp = platform == SamplePlatform.WhatsApp;
        var configuration = Configuration();
        var services = new ServiceCollection();
        new Startup(configuration).ConfigureServices(services);
        services.AddLogging();
        using var http = new HttpClient(new RecordingHttpHandler((_, _) =>
            RecordingHttpHandler.Json("""{"messages":[{"id":"wamid"}]}""")));
        var transport = whatsapp ? new WhatsAppSampleMessageSender(new global::WhatsApp.Messaging.WhatsAppClient(http,
            new() { PhoneNumberId = "99", AccessToken = "dummy", ApiVersion = "v23.0" })) : null;
        var sent = new RecordingSender(instagram || whatsapp ? 2 : 1, transport);
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
            "/" + (whatsapp ? Settings.WhatsAppRouteToUpdate : instagram ? Settings.InstagramRouteToUpdate : vk ? Settings.VkRouteToUpdate : max ? Settings.MaxRouteToUpdate : Settings.RouteToUpdate);

        var whatsappCallbackIndex = 0;
        async Task Send(string text, bool callback = false, bool photo = false)
        {
            // Each update gets a fresh DI scope, just like a webhook request.
            using var scope = provider.CreateScope();
            if (whatsapp)
            {
                var callbackKind = callback ? whatsappCallbackIndex++ % 3 : -1;
                var json = JsonSerializer.Serialize(new {
                    from = "2", id = "wamid",
                    type = callback ? (callbackKind == 2 ? "button" : "interactive") : photo ? "image" : "text",
                    text = callback || photo ? null : new { body = text },
                    image = photo ? new { id = "media" } : null,
                    context = callback ? new { id = "menu" } : null,
                    button = callbackKind == 2 ? new { payload = text, text = "choose" } : null,
                    interactive = callback && callbackKind != 2 ? new {
                        type = callbackKind == 0 ? "button_reply" : "list_reply",
                        button_reply = callbackKind == 0 ? new { id = text, title = "choose" } : null,
                        list_reply = callbackKind == 1 ? new { id = text, title = "choose" } : null
                    } : null
                });
                await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<global::WhatsApp.Messaging.Models.WhatsAppMessage>>()
                    .Handle(JsonSerializer.Deserialize<global::WhatsApp.Messaging.Models.WhatsAppMessage>(json));
            }
            else if (instagram)
            {
                var json = JsonSerializer.Serialize(new
                {
                    sender = new { id = "2" }, recipient = new { id = "99" },
                    message = callback ? null : new { mid = "mid", text,
                        attachments = photo ? new[] { new { type = "image", payload = new { url = "https://example.com/photo.jpg" } } } : null },
                    postback = callback ? new { mid = "mid", title = "choose", payload = text } : null
                });
                await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<global::Instagram.Messaging.Models.InstagramMessagingEvent>>()
                    .Handle(JsonSerializer.Deserialize<global::Instagram.Messaging.Models.InstagramMessagingEvent>(json));
            }
            else if (vk)
            {
                var json = callback
                    ? Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        type = "message_event", group_id = 1,
                        @object = new { peer_id = 1, user_id = 2, conversation_message_id = 7, event_id = "cb",
                            payload = Newtonsoft.Json.Linq.JObject.Parse(Bot.Commands.Vk.VkCallbackPayload.Encode(text)) }
                    })
                    : Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        type = "message_new", group_id = 1,
                        @object = new { message = new { peer_id = 1, from_id = 2, conversation_message_id = 7, text,
                            attachments = photo ? new[] { new { type = "photo", photo = new { id = 12, owner_id = 2 } } } : null },
                            client_info = new { keyboard = true } }
                    });
                await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<VkNet.Model.GroupUpdate>>()
                    .Handle(Newtonsoft.Json.JsonConvert.DeserializeObject<VkNet.Model.GroupUpdate>(json));
            }
            else if (max)
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

        await Send("/start");
        Assert.That(sent.Messages.Last().Text, Does.Contain("/profile"));
        await Send("/ask");
        if (whatsapp)
        {
            var count = sent.Messages.Count;
            foreach (var type in new[] { "reaction", "system", "unsupported", "interactive" })
            {
                using var ignored = provider.CreateScope();
                await ignored.ServiceProvider.GetRequiredService<IBotCommandResolver<global::WhatsApp.Messaging.Models.WhatsAppMessage>>()
                    .Handle(new() { From = "2", Id = "ignored", Type = type });
            }
            Assert.That(sent.Messages.Count, Is.EqualTo(count));
        }
        if (instagram)
        {
            var count = sent.Messages.Count;
            foreach (var extra in new[] { "\"message\":{\"text\":\"echo\",\"is_echo\":true}",
                         "\"message\":{\"mid\":\"mid\",\"is_deleted\":true}",
                         "\"read\":{\"mid\":\"mid\"}", "\"reaction\":{\"reaction\":\"love\"}" })
            {
                using var ignored = provider.CreateScope();
                var json = "{\"sender\":{\"id\":\"2\"},\"recipient\":{\"id\":\"99\"}," + extra + "}";
                await ignored.ServiceProvider.GetRequiredService<IBotCommandResolver<global::Instagram.Messaging.Models.InstagramMessagingEvent>>()
                    .Handle(JsonSerializer.Deserialize<global::Instagram.Messaging.Models.InstagramMessagingEvent>(json));
            }
            Assert.That(sent.Messages.Count, Is.EqualTo(count));
        }
        await Send("Ada");
        Assert.That(sent.Messages.Last().Text, Is.EqualTo("Nice to meet you, Ada!"));
        await Send("/profile");
        await Send("Grace");
        Assert.That(sent.Messages.Last().Text, Is.EqualTo("Profile saved: Grace"));
        await Send("/profile");
        var payload = sent.Messages.Last().Callbacks.Single().CallbackText;
        await Send(payload, callback: true);
        Assert.That(sent.Messages.Last().Text, Is.EqualTo("Profile saved: Alex"));
        async Task Click(string label)
        {
            var button = sent.Messages.AsEnumerable().Reverse().SelectMany(m => m.Callbacks ?? Enumerable.Empty<CallbackDataWithCommand>())
                .First(c => c.Text == label);
            await Send(button.CommandDescriptor == null ? button.CallbackText : $"/{button.CommandDescriptor.Name} {button.CallbackText}", callback: true);
        }
        await Send("register");
        await Send("Funnel User");
        Assert.That(sent.Messages.Last().Text, Is.EqualTo("Profile saved: Funnel User"));
        foreach (var command in new[] { "/dropdown", "/dropdownmultiple" })
        {
            await Send(command);
            var dropdown = sent.Messages.Last().Callbacks.First();
            await Send($"/{dropdown.CommandDescriptor.Name} {dropdown.CallbackText}", callback: true);
            await Click("Item1");
            Assert.That(sent.Messages.Last().Callbacks.Any(c => c.Text == "✓ Item1"), Is.True);
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
                .GetSessionInfo(DateTime.Now, instagram || whatsapp ? 2 : 1, 2), Is.Null);
        await Send("/global");
        await Send("toggle");
        await Send("intercept me");
        Assert.That(sent.Messages.Last().Text, Is.EqualTo("Intercepted message"));
        await Click("Stop");
        // A message-only funnel must not consume a callback with the same text.
        await Assert.ThrowsAsync<Bot.Commands.Core.Exceptions.CommandExtractionException>(
            () => Send("register", callback: true));
        Assert.That(executed.All(t => t.Name.StartsWith(whatsapp ? "WhatsApp" : instagram ? "Instagram" : vk ? "Vk" : max ? "Max" : "Telegram", StringComparison.Ordinal)), Is.True);
        using var check = provider.CreateScope();
        Assert.That(check.ServiceProvider.GetRequiredService<ISessionsStore>().GetSessionInfo(DateTime.Now, instagram || whatsapp ? 2 : 1, 2), Is.Null);

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
        ["MaxBotProfile:BotName"] = "max_sample",
        ["VkBotProfile:Enabled"] = "true", ["VkBotProfile:BotName"] = "vk_sample",
        ["VkBotProfile:Token"] = "dummy",
        ["VkBotProfile:GroupId"] = "1", ["VkBotProfile:WebhookSecret"] = "test-secret",
        ["VkBotProfile:ConfirmationCode"] = "confirmation",
        ["InstagramBotProfile:Enabled"] = "true", ["InstagramBotProfile:BotName"] = "instagram_sample",
        ["InstagramBotProfile:AccountId"] = "99", ["InstagramBotProfile:Token"] = "dummy",
        ["InstagramBotProfile:ApiVersion"] = "v23.0", ["InstagramBotProfile:AppSecret"] = "test-secret",
        ["InstagramBotProfile:VerifyToken"] = "test-verify",
        ["WhatsAppBotProfile:Enabled"] = "true", ["WhatsAppBotProfile:BotName"] = "whatsapp_sample",
        ["WhatsAppBotProfile:PhoneNumberId"] = "99", ["WhatsAppBotProfile:BusinessAccountId"] = "88",
        ["WhatsAppBotProfile:Token"] = "dummy", ["WhatsAppBotProfile:ApiVersion"] = "v23.0",
        ["WhatsAppBotProfile:AppSecret"] = "secret", ["WhatsAppBotProfile:VerifyToken"] = "verify"
    }).Build();

    private sealed class RecordingSender(long expectedChat = 1, ISampleMessageSender transport = null) : ISampleMessageSender
    {
        public List<IBotMessage> Messages { get; } = new();
        public Task Edit(long chatId, string messageId, IBotMessage message) => Send(chatId, message);
        public Task Send(long chatId, IBotMessage message)
        {
            Assert.That(chatId, Is.EqualTo(expectedChat));
            Messages.Add(message);
            return transport?.Send(chatId, message) ?? Task.CompletedTask;
        }
    }
}
