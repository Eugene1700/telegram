using System.Collections.Generic;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Instagram;
using global::Instagram.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SimpleHandlers.Services;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.Utils;

namespace Bot.Commands.Core.Tests;

public class WebhookRegistrationTests
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task VkRegistrationCreatesOrReusesServerAndSubscribesBothEvents(bool exists)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> {
            ["VkBotProfile:RegisterWebhook"] = "true", ["VkBotProfile:GroupId"] = "99",
            ["VkBotProfile:BaseUrl"] = "https://example.com", ["VkBotProfile:BotName"] = "vk_sample",
            ["VkBotProfile:WebhookSecret"] = "secret", ["VkBotProfile:CallbackApiVersion"] = "5.199"
        }).Build();
        var groups = new Mock<IGroupsCategory>(MockBehavior.Strict);
        groups.Setup(g => g.GetCallbackConfirmationCodeAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync("api-code");
        groups.Setup(g => g.GetCallbackServersAsync(99, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VkCollection<CallbackServerItem>(exists ? 1UL : 0UL, exists
                ? new[] { new CallbackServerItem { Id = 7, Url = "https://example.com/api/vk/message/update" } }
                : Array.Empty<CallbackServerItem>()));
        groups.Setup(g => g.AddCallbackServerAsync(99, "https://example.com/api/vk/message/update", "vk_sample", "secret", It.IsAny<CancellationToken>()))
            .Callback(() => Assert.That(config["VkBotProfile:ConfirmationCode"], Is.EqualTo("api-code"))).ReturnsAsync(7);
        groups.Setup(g => g.EditCallbackServerAsync(99, 7, "https://example.com/api/vk/message/update", "vk_sample", "secret", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var api = new Mock<IVkApi>(MockBehavior.Strict);
        api.SetupGet(a => a.Groups).Returns(groups.Object);
        api.Setup(a => a.CallAsync<bool>("groups.setCallbackSettings", It.IsAny<VkParameters>(), false, It.IsAny<CancellationToken>()))
            .Callback<string, VkParameters, bool, CancellationToken>((_, p, _, _) => {
                Assert.That(p["message_new"], Is.EqualTo("1"));
                Assert.That(p["message_event"], Is.EqualTo("1"));
                Assert.That(p["server_id"], Is.EqualTo("7"));
                Assert.That(p["api_version"], Is.EqualTo("5.199"));
            }).ReturnsAsync(true);
        var service = new ConfigureVkWebhook(api.Object, config);
        await service.StartAsync(default);
        api.VerifyGet(a => a.Groups, Times.Never);
        await service.StartedAsync(default);
        groups.Verify(g => g.AddCallbackServerAsync(99, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            exists ? Times.Never() : Times.Once());
        groups.Verify(g => g.EditCallbackServerAsync(99, 7, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            exists ? Times.Once() : Times.Never());
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task InstagramRegistrationIsOptInAndRunsAfterServerStart(bool enabled)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> {
            ["InstagramBotProfile:RegisterWebhook"] = enabled.ToString(),
            ["InstagramBotProfile:BaseUrl"] = "https://example.com"
        }).Build();
        var api = new Mock<IInstagramClient>(MockBehavior.Strict);
        api.Setup(a => a.SubscribeToMessagingAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var service = new ConfigureInstagramWebhook(api.Object, config, NullLogger<ConfigureInstagramWebhook>.Instance);
        await service.StartAsync(default);
        api.Verify(a => a.SubscribeToMessagingAsync(It.IsAny<CancellationToken>()), Times.Never);
        await service.StartedAsync(default);
        api.Verify(a => a.SubscribeToMessagingAsync(It.IsAny<CancellationToken>()), enabled ? Times.Once() : Times.Never());
    }

    [Test]
    public async Task SubscriptionUsesAccountTokenAndChecksSuccess()
    {
        using var handler = new RecordingHttpHandler((r, body) => {
            Assert.That(r.RequestUri!.AbsolutePath, Is.EqualTo("/v23.0/me/subscribed_apps"));
            Assert.That(r.Headers.Authorization?.Parameter, Is.EqualTo("dummy"));
            Assert.That(Uri.UnescapeDataString(body), Is.EqualTo("subscribed_fields=messages,messaging_postbacks"));
            return RecordingHttpHandler.Json("""{"success":true}""");
        });
        using var http = new HttpClient(handler);
        await new InstagramClient(http, new() { AccountId = "99", AccessToken = "dummy", ApiVersion = "v23.0" })
            .SubscribeToMessagingAsync();
    }

    [Test]
    public async Task InstagramSenderSplitsButtonsAndEditPublishesNewMenu()
    {
        using var handler = new RecordingHttpHandler((_, _) => RecordingHttpHandler.Json("""{"recipient_id":"2","message_id":"mid"}"""));
        using var http = new HttpClient(handler);
        var sender = new InstagramSampleMessageSender(new InstagramClient(http,
            new() { AccountId = "99", AccessToken = "dummy", ApiVersion = "v23.0" }));
        var callbacks = Enumerable.Range(1, 4).Select(i => new CallbackDataWithCommand {
            Text = i.ToString(), CallbackText = "/pick " + i, CallbackMode = CallbackMode.CallbackData }).ToArray();
        await sender.Edit(2, "old-mid", new BotMessage("Choose", null, null, Array.AsReadOnly(callbacks)));
        Assert.That(handler.Requests.Count, Is.EqualTo(2));
        var builder = new InstagramInlineMarkupQueryBuilder().InlineKeyboardButtonsRow(callbacks);
        Assert.Throws<NotSupportedException>(() => builder.GetResult());
        var invalid = callbacks[0] with { CallbackText = "" };
        await Assert.ThrowsAsync<ArgumentException>(() => sender.Send(2,
            new BotMessage("Choose", null, null, Array.AsReadOnly(callbacks.Append(invalid).ToArray()))));
        Assert.That(handler.Requests.Count, Is.EqualTo(2), "Validate all chunks before sending the first one.");
    }
}
