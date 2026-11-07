using System.Net;
using System.Text.Json;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.WhatsApp;
using Bot.Commands.WhatsApp.Models;
using global::WhatsApp.Messaging;
using global::WhatsApp.Messaging.Models;
using SimpleHandlers.Services;

namespace Bot.Commands.Core.Tests;

public class WhatsAppApiTests
{
    private static WhatsAppOptions Options() => new() { PhoneNumberId = "99", BusinessAccountId = "88", AccessToken = "dummy", ApiVersion = "v23.0" };

    [Test]
    public async Task ClientSendsNativeTextButtonsAndImageAndSubscribesWaba()
    {
        using var handler = new RecordingHttpHandler((request, _) =>
        {
            Assert.That(request.Method, Is.EqualTo(HttpMethod.Post));
            Assert.That(request.RequestUri!.Host, Is.EqualTo("graph.facebook.com"));
            Assert.That(request.RequestUri.Query, Is.Empty);
            Assert.That(request.Headers.Authorization?.ToString(), Is.EqualTo("Bearer dummy"));
            return RecordingHttpHandler.Json(request.RequestUri.AbsolutePath.EndsWith("/subscribed_apps")
                ? """{"success":true}""" : """{"messaging_product":"whatsapp","messages":[{"id":"wamid.1"}]}""");
        });
        using var http = new HttpClient(handler);
        var client = new WhatsAppClient(http, Options());
        Assert.That(await client.SendTextAsync("2", "Привет"), Is.EqualTo("wamid.1"));
        await client.SendTextAsync("2", "Choose", new[] { new WhatsAppReplyButton("/pick a&b=c", "Pick") });
        await client.SendImageAsync("2", new Uri("https://example.com/image.jpg"));
        await client.SubscribeToMessagingAsync();
        Assert.That(handler.Requests[0].Uri.AbsolutePath, Is.EqualTo("/v23.0/99/messages"));
        Assert.That(handler.Requests[3].Uri.AbsolutePath, Is.EqualTo("/v23.0/88/subscribed_apps"));
        using var text = JsonDocument.Parse(handler.Requests[0].Body);
        Assert.That(text.RootElement.GetProperty("messaging_product").GetString(), Is.EqualTo("whatsapp"));
        Assert.That(text.RootElement.GetProperty("to").GetString(), Is.EqualTo("2"));
        Assert.That(text.RootElement.GetProperty("text").GetProperty("body").GetString(), Is.EqualTo("Привет"));
        using var buttons = JsonDocument.Parse(handler.Requests[1].Body);
        var button = buttons.RootElement.GetProperty("interactive").GetProperty("action").GetProperty("buttons")[0];
        Assert.That(button.GetProperty("type").GetString(), Is.EqualTo("reply"));
        Assert.That(button.GetProperty("reply").GetProperty("id").GetString(), Is.EqualTo("/pick a&b=c"));
        using var image = JsonDocument.Parse(handler.Requests[2].Body);
        Assert.That(image.RootElement.GetProperty("image").GetProperty("link").GetString(), Is.EqualTo("https://example.com/image.jpg"));
        Assert.That(typeof(WhatsAppClient).Assembly.GetReferencedAssemblies().Any(a => a.Name!.StartsWith("Bot.Commands")), Is.False);
    }

    [Test]
    public async Task SenderSplitsMenusAndValidatesAllChunksBeforeSending()
    {
        using var handler = new RecordingHttpHandler((_, _) => RecordingHttpHandler.Json("""{"messages":[{"id":"wamid"}]}"""));
        using var http = new HttpClient(handler);
        var sender = new WhatsAppSampleMessageSender(new WhatsAppClient(http, Options()));
        var callbacks = Enumerable.Range(1, 4).Select(i => new CallbackDataWithCommand {
            Text = "Pick " + i, CallbackText = "/pick " + i, CallbackMode = CallbackMode.CallbackData }).ToArray();
        await sender.Edit(2, "old-message", new BotMessage("Choose", null, null, Array.AsReadOnly(callbacks)));
        Assert.That(handler.Requests.Count, Is.EqualTo(2));
        var invalid = callbacks[0] with { CallbackText = new string('a', 257) };
        await Assert.ThrowsAsync<ArgumentException>(() => sender.Send(2,
            new BotMessage("Choose", null, null, Array.AsReadOnly(callbacks.Append(invalid).ToArray()))));
        Assert.That(handler.Requests.Count, Is.EqualTo(2));
        Assert.Throws<NotSupportedException>(() => new WhatsAppInlineMarkupQueryBuilder().InlineKeyboardButtonsRow(callbacks).GetResult());
        Assert.Throws<NotSupportedException>(() => new WhatsAppInlineMarkupQueryBuilder()
            .AddInlineKeyboardButton(new CallbackData { Text = "Link", CallbackText = "https://example.com", CallbackMode = CallbackMode.Url }));
    }

    [Test]
    public async Task ApiFailuresAreSanitizedAndMalformedSuccessIsRejected()
    {
        using var handler = new RecordingHttpHandler((_, _) => RecordingHttpHandler.Json("dummy secret", HttpStatusCode.TooManyRequests));
        using var http = new HttpClient(handler);
        var error = await Assert.ThrowsAsync<HttpRequestException>(() => new WhatsAppClient(http, Options()).SendTextAsync("2", "hello"));
        Assert.That(error!.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
        Assert.That(error.Message, Does.Not.Contain("dummy"));
        using var malformed = new HttpClient(new RecordingHttpHandler((_, _) => RecordingHttpHandler.Json("{}")));
        var client = new WhatsAppClient(malformed, Options());
        await Assert.ThrowsAsync<JsonException>(() => client.SendTextAsync("2", "hello"));
        await Assert.ThrowsAsync<JsonException>(() => client.SubscribeToMessagingAsync());
        Assert.Throws<ArgumentException>(() => client.SendTextAsync("2", new string('a', 4097)));
        Assert.Throws<ArgumentException>(() => client.SendTextAsync("2", "choose", new[] { new WhatsAppReplyButton("a", "A"), new WhatsAppReplyButton("a", "B") }));
        Assert.Throws<ArgumentException>(() => client.SendImageAsync("2", new Uri("http://example.com/a")));
    }

    [TestCase("0")]
    [TestCase("02")]
    [TestCase("+123")]
    [TestCase("9223372036854775808")]
    public void AdapterRejectsUnrepresentableSessionIds(string id)
        => Assert.Throws<ArgumentException>(() => WhatsAppIdentity.ToSessionId(id));

    [Test]
    public void ContextRetainsNativeReplyAndUsesOriginalMenuId()
    {
        var message = JsonSerializer.Deserialize<WhatsAppMessage>("""
            {"from":"2","id":"incoming","type":"interactive","context":{"id":"outgoing"},
             "interactive":{"type":"list_reply","list_reply":{"id":"/pick a&b=c","title":"Pick","description":"details"}}}
            """)!;
        var context = new WhatsAppReplyQueryContext(message, message.Interactive!.ListReply!);
        Assert.That(context.Query, Is.SameAs(message.Interactive.ListReply));
        Assert.That(context.GetData(), Is.EqualTo("/pick a&b=c"));
        Assert.That(context.GetMessageId(), Is.EqualTo("outgoing"));
        Assert.That(context.GetChatId(), Is.EqualTo(2));
        Assert.That(context.GetFromId(), Is.EqualTo(2));
    }
}
