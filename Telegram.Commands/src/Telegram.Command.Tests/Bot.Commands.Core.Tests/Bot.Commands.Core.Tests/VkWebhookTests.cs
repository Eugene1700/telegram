using System.Text;
using System.Text.Json;
using Bot.Commands.Abstract.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SimpleHandlers;
using SimpleHandlers.Controllers;
using SimpleHandlers.Services;
using VkNet.Abstractions;
using VkNet.Model;

namespace Bot.Commands.Core.Tests;

public class VkWebhookTests
{
    internal static IConfiguration Config(bool enabled = true, string name = "vk_sample") =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            ["BotProfile:Enabled"] = "true", ["BotProfile:RegisterWebhook"] = "false", ["BotProfile:BotName"] = "telegram_sample",
            ["MaxBotProfile:Enabled"] = "true", ["MaxBotProfile:RegisterWebhook"] = "false", ["MaxBotProfile:BotName"] = "max_sample",
            ["VkBotProfile:Enabled"] = enabled.ToString(), ["VkBotProfile:BotName"] = name,
            ["VkBotProfile:GroupId"] = "123", ["VkBotProfile:WebhookSecret"] = "test-secret",
            ["VkBotProfile:ConfirmationCode"] = "confirmed", ["VkBotProfile:Token"] = "dummy"
        }).Build();

    [TestCase(false, 123, "test-secret", 404)]
    [TestCase(true, 124, "test-secret", 401)]
    [TestCase(true, 123, "wrong", 401)]
    [TestCase(true, 123, "test-secret", 0)]
    public async Task ResourceFilterValidatesBeforeActionAndRewindsBody(bool enabled, int group, string secret, int status)
    {
        var json = JsonSerializer.Serialize(new { group_id = group, secret, type = "confirmation" });
        var http = new DefaultHttpContext();
        http.Request.Path = "/" + Settings.VkRouteToUpdate;
        http.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var filter = new WebhookAccessFilter(Config(enabled), new BotRuntime(new HttpContextAccessor { HttpContext = http }));
        var action = new ActionContext(http, new Microsoft.AspNetCore.Routing.RouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
        var context = new ResourceExecutingContext(action, new List<IFilterMetadata>(),
            new List<Microsoft.AspNetCore.Mvc.ModelBinding.IValueProviderFactory>());
        var called = false;
        await filter.OnResourceExecutionAsync(context, async () =>
        {
            called = true;
            using var reader = new StreamReader(http.Request.Body, leaveOpen: true);
            Assert.That(await reader.ReadToEndAsync(), Is.EqualTo(json));
            return new ResourceExecutedContext(action, new List<IFilterMetadata>());
        });
        Assert.That(called, Is.EqualTo(status == 0));
        Assert.That((context.Result as StatusCodeResult)?.StatusCode ?? 0, Is.EqualTo(status));
    }

    [Test]
    public void EnabledBotNamesMustBeUniqueAcrossAllPlatforms()
    {
        Assert.Throws<InvalidOperationException>(() => new Startup(Config(name: "TELEGRAM_SAMPLE")).ConfigureServices(new ServiceCollection()));
        Assert.Throws<InvalidOperationException>(() => new Startup(Config(name: "max_sample")).ConfigureServices(new ServiceCollection()));
        Assert.Throws<InvalidOperationException>(() => new Startup(Config(name: " ")).ConfigureServices(new ServiceCollection()));
    }

    [TestCase("{", 400)]
    [TestCase("[]", 401)]
    [TestCase("{\"group_id\":123}", 401)]
    public async Task MalformedOrUnsignedBodiesNeverReachAction(string json, int status)
    {
        var http = new DefaultHttpContext();
        http.Request.Path = "/" + Settings.VkRouteToUpdate;
        http.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var filter = new WebhookAccessFilter(Config(), new BotRuntime(new HttpContextAccessor { HttpContext = http }));
        var action = new ActionContext(http, new Microsoft.AspNetCore.Routing.RouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
        var context = new ResourceExecutingContext(action, new List<IFilterMetadata>(),
            new List<Microsoft.AspNetCore.Mvc.ModelBinding.IValueProviderFactory>());
        await filter.OnResourceExecutionAsync(context, () => throw new AssertionException("Action must not execute."));
        Assert.That(((StatusCodeResult)context.Result).StatusCode, Is.EqualTo(status));
        Assert.That(http.Request.Body.Position, Is.Zero);
    }

    [Test]
    public void VkScopeSelectsVkProfileStoreAndSender()
    {
        var services = new ServiceCollection();
        new Startup(Config()).ConfigureServices(services);
        services.AddLogging();
        services.AddSingleton(Mock.Of<IVkApi>());
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IHttpContextAccessor>().HttpContext = new DefaultHttpContext();
        provider.GetRequiredService<IHttpContextAccessor>().HttpContext.Request.Path = "/" + Settings.VkRouteToUpdate;
        using var scope = provider.CreateScope();
        var profile = scope.ServiceProvider.GetRequiredService<IBotProfile>();
        Assert.That(profile.BotName, Is.EqualTo("vk_sample"));
        Assert.That(profile.UpdateRoute, Is.EqualTo(Settings.VkRouteToUpdate));
        Assert.That(profile.Swarms, Is.EqualTo(new[] { "vk" }));
        Assert.That(scope.ServiceProvider.GetRequiredService<ISampleMessageSender>(), Is.TypeOf<VkSampleMessageSender>());
        Assert.That(scope.ServiceProvider.GetRequiredService<ISessionsStore>(), Is.TypeOf<SessionStore>());
    }

    [Test]
    public async Task ConfirmationAndCallbackReturnVkPlainTextResponses()
    {
        var messages = new Mock<IMessagesCategory>();
        var client = new Mock<IVkApi>();
        client.SetupGet(c => c.Messages).Returns(messages.Object);
        messages.Setup(m => m.SendMessageEventAnswerAsync("event", 2, 1, It.IsAny<EventData>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var resolver = new Mock<IBotCommandResolver<GroupUpdate>>();
        resolver.Setup(r => r.Handle(It.IsAny<GroupUpdate>())).Returns(Task.CompletedTask);
        using var http = new HttpClient(new RecordingHttpHandler((_, _) => throw new AssertionException("Unexpected upload")));
        var sender = new VkSampleMessageSender(client.Object, http);
        var controller = new VkMessageController(NullLogger<VkMessageController>.Instance, Config());
        var confirm = JsonSerializer.Deserialize<JsonElement>("""{"type":"confirmation"}""");
        Assert.That(((ContentResult)await controller.Update(confirm, client.Object, resolver.Object, sender)).Content, Is.EqualTo("confirmed"));
        resolver.Verify(r => r.Handle(It.IsAny<GroupUpdate>()), Times.Never);
        var callback = JsonSerializer.Deserialize<JsonElement>("""
            {"type":"message_event","group_id":123,"object":{"event_id":"event","peer_id":1,"user_id":2,"conversation_message_id":7,"payload":{"bot_commands":"/start"}}}
            """);
        var result = (ContentResult)await controller.Update(callback, client.Object, resolver.Object, sender);
        Assert.That(result.Content, Is.EqualTo("ok"));
        Assert.That(result.ContentType, Is.EqualTo("text/plain"));
        messages.Verify(m => m.SendMessageEventAnswerAsync("event", 2, 1, It.IsAny<EventData>(), It.IsAny<CancellationToken>()), Times.Once);
        resolver.Verify(r => r.Handle(It.Is<GroupUpdate>(u => u.Instance is MessageEvent)), Times.Once);
    }
}
