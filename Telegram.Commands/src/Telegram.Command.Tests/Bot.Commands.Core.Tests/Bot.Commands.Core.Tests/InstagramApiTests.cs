using System.Net;
using System.Text.Json;
using Bot.Commands.Instagram;
using Bot.Commands.Instagram.Models;
using global::Instagram.Messaging;
using global::Instagram.Messaging.Models;

namespace Bot.Commands.Core.Tests;

public class InstagramApiTests
{
    private static InstagramOptions Options() => new() { AccountId = "99", AccessToken = "test-token", ApiVersion = "v23.0" };

    [Test]
    public async Task ClientSendsTextTemplatesAndImagesThroughOfficialEndpoint()
    {
        using var handler = new RecordingHttpHandler((request, _) =>
        {
            Assert.That(request.Headers.Authorization?.ToString(), Is.EqualTo("Bearer test-token"));
            Assert.That(request.RequestUri!.AbsoluteUri, Is.EqualTo("https://graph.instagram.com/v23.0/99/messages"));
            return RecordingHttpHandler.Json("""{"recipient_id":"2","message_id":"mid"}""");
        });
        using var http = new HttpClient(handler);
        var client = new InstagramClient(http, Options());
        Assert.That((await client.SendTextAsync("2", "Привет")).MessageId, Is.EqualTo("mid"));
        await client.SendTextAsync("2", "Choose", new[] { new InstagramButton("postback", "One", "/choose 1"),
            new InstagramButton("web_url", "Site", Url: "https://example.com") });
        await client.SendImageAsync("2", new Uri("https://example.com/photo.jpg"));
        Assert.That(handler.Requests.All(r => r.Method == "POST"), Is.True);
        using var text = JsonDocument.Parse(handler.Requests[0].Body);
        Assert.That(text.RootElement.GetProperty("recipient").GetProperty("id").GetString(), Is.EqualTo("2"));
        Assert.That(text.RootElement.GetProperty("message").GetProperty("text").GetString(), Is.EqualTo("Привет"));
        using var template = JsonDocument.Parse(handler.Requests[1].Body);
        var payload = template.RootElement.GetProperty("message").GetProperty("attachment").GetProperty("payload");
        Assert.That(payload.GetProperty("template_type").GetString(), Is.EqualTo("button"));
        Assert.That(payload.GetProperty("buttons")[0].GetProperty("payload").GetString(), Is.EqualTo("/choose 1"));
        Assert.That(payload.GetProperty("buttons")[1].TryGetProperty("payload", out _), Is.False);
        using var image = JsonDocument.Parse(handler.Requests[2].Body);
        Assert.That(image.RootElement.GetProperty("message").GetProperty("attachment").GetProperty("payload")
            .GetProperty("url").GetString(), Is.EqualTo("https://example.com/photo.jpg"));
    }

    [Test]
    public async Task ErrorsPreserveStatusWithoutLeakingResponseBody()
    {
        using var handler = new RecordingHttpHandler((_, _) => RecordingHttpHandler.Json("test-token", HttpStatusCode.TooManyRequests));
        using var http = new HttpClient(handler);
        var client = new InstagramClient(http, Options());
        var error = await Assert.ThrowsAsync<HttpRequestException>(() => client.SendTextAsync("2", "hello"));
        Assert.That(error!.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
        Assert.That(error.Message, Does.Not.Contain("test-token"));
        Assert.That(handler.Requests.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task MalformedSuccessAndInvalidButtonsAreRejected()
    {
        using var handler = new RecordingHttpHandler((_, _) => RecordingHttpHandler.Json("{}"));
        using var http = new HttpClient(handler);
        var client = new InstagramClient(http, Options());
        await Assert.ThrowsAsync<JsonException>(() => client.SendTextAsync("2", "hello"));
        Assert.Throws<ArgumentException>(() => client.SendTextAsync("2", "hello",
            Enumerable.Repeat(new InstagramButton("postback", "a", "b"), 4).ToArray()));
        Assert.Throws<ArgumentException>(() => client.SendTextAsync("2", "hello", new[] { new InstagramButton("postback", "a", "") }));
        Assert.Throws<ArgumentException>(() => client.SendImageAsync("2", new Uri("http://example.com/image")));
        Assert.That(handler.Requests.Count, Is.EqualTo(1));
    }

    [TestCase("0")]
    [TestCase("02")]
    [TestCase("-2")]
    [TestCase("9223372036854775808")]
    public void AdapterRejectsIdsThatCannotFitSessionContract(string id)
        => Assert.Throws<ArgumentException>(() => InstagramIdentity.ToSessionId(id));

    [Test]
    public async Task SdkRetainsLargeStringIdsAndHasNoFrameworkDependency()
    {
        const string id = "9223372036854775808";
        using var handler = new RecordingHttpHandler((_, _) => RecordingHttpHandler.Json("""{"recipient_id":"9223372036854775808","message_id":"mid"}"""));
        using var http = new HttpClient(handler);
        Assert.That((await new InstagramClient(http, Options()).SendTextAsync(id, "hello")).RecipientId, Is.EqualTo(id));
        Assert.That(typeof(InstagramClient).Assembly.GetReferencedAssemblies().Any(a => a.Name!.StartsWith("Bot.Commands")), Is.False);
    }

    [Test]
    public void ContextsRetainNativeObjectsAndMapDirectMessageIdentity()
    {
        var update = JsonSerializer.Deserialize<InstagramMessagingEvent>("""{"sender":{"id":"2"},"recipient":{"id":"99"},"message":{"mid":"m1","text":"title","quick_reply":{"payload":"/ask"}}}""")!;
        var context = new InstagramMessageQueryContext(update);
        Assert.That(context.Query, Is.SameAs(update.Message));
        Assert.That(context.GetChatId(), Is.EqualTo(2));
        Assert.That(context.GetFromId(), Is.EqualTo(2));
        Assert.That(context.GetData(), Is.EqualTo("/ask"));
        Assert.That(context.GetMessageId(), Is.EqualTo("m1"));
        var postback = new InstagramMessagingEvent { Sender = update.Sender,
            Postback = new InstagramPostback { Payload = "/pick 1", Mid = "m2" } };
        Assert.That(new InstagramPostbackQueryContext(postback).GetData(), Is.EqualTo("/pick 1"));
    }
}
