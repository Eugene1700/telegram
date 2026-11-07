using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using Bot.Commands.Abstract.Messages;
using SimpleHandlers.Services;
using global::Telegram.Bot;
using global::Max.Bot;
using global::Max.Bot.Configuration;

namespace Bot.Commands.Core.Tests;

public class MessageSenderTests
{
    private static readonly byte[] Content = Encoding.UTF8.GetBytes("attachment-test-content");

    private static BotMessage Message(string kind, bool keyboard, string filename = "sample.png")
        => new("caption", kind == "text" ? null : Content, null,
            keyboard ? new ReadOnlyCollection<CallbackDataWithCommand>(new[]
            {
                new CallbackDataWithCommand { Text = "Next", CallbackText = "payload" }
            }) : null)
        {
            SendAsFile = kind == "file", FileName = filename
        };

    [TestCase("text", false)]
    [TestCase("photo", false)]
    [TestCase("file", false)]
    [TestCase("text", true)]
    [TestCase("photo", true)]
    [TestCase("file", true)]
    [TestCase("file", false, null)]
    [TestCase("photo", true, null)]
    public async Task TelegramSendsAndEditsCorrectMessageKind(string kind, bool edit, string filename = "sample.png")
    {
        using var handler = new RecordingHttpHandler((_, _) => RecordingHttpHandler.Json(
            """{"ok":true,"result":{"message_id":42,"date":1700000000,"chat":{"id":12,"type":"private"}}}"""));
        using var http = new HttpClient(handler);
        var sender = new TelegramSampleMessageSender(new TelegramBotClient("123456:test-token", http));
        if (edit) await sender.Edit(12, "42", Message(kind, true, filename));
        else await sender.Send(12, Message(kind, true, filename));

        var request = handler.Requests.Single();
        var method = edit ? (kind == "text" ? "editMessageText" : "editMessageMedia")
            : kind == "text" ? "sendMessage" : kind == "file" ? "sendDocument" : "sendPhoto";
        Assert.That(request.Uri.AbsolutePath, Does.EndWith("/" + method).IgnoreCase);
        Assert.That(request.Body, Does.Contain("caption"));
        Assert.That(request.Body, Does.Contain("payload"));
        Assert.That(request.Body, Does.Contain("12"));
        if (edit) Assert.That(request.Body, Does.Contain("42"));
        if (kind != "text")
        {
            Assert.That(request.Body, Does.Contain(filename ?? (kind == "file" ? "attachment.bin" : "photo.jpg")));
            Assert.That(request.Body, Does.Contain(Encoding.UTF8.GetString(Content)));
        }
    }

    [TestCase("photo", false, false)]
    [TestCase("file", false, true)]
    [TestCase("photo", true, true)]
    [TestCase("file", true, false)]
    [TestCase("file", false, false, null)]
    [TestCase("photo", true, false, null)]
    public async Task MaxUploadsCorrectTypeAndPreservesCaptionAndKeyboard(string kind, bool edit, bool keyboard, string filename = "sample.png")
    {
        using var handler = new RecordingHttpHandler((request, _) =>
        {
            if (request.RequestUri.AbsolutePath == "/uploads")
                return RecordingHttpHandler.Json("""{"url":"https://upload.test/content"}""");
            if (request.RequestUri.Host == "upload.test")
                return RecordingHttpHandler.Json(kind == "photo"
                    ? """{"photos":{"1":{"token":"uploaded-token"}}}"""
                    : """{"token":"uploaded-token"}""");
            if (request.Method == HttpMethod.Put)
                return RecordingHttpHandler.Json("""{"success":true}""");
            return RecordingHttpHandler.Json(
                """{"success":true,"message":{"recipient":{"chat_id":12,"chat_type":"dialog"},"timestamp":0,"body":{"mid":"42","seq":1,"text":"caption"}}}""");
        });
        using var http = new HttpClient(handler);
        using var client = new MaxClient(new MaxBotOptions { Token = "test-token" }, httpClient: http);
        var sender = new MaxSampleMessageSender(client);
        if (edit) await sender.Edit(12, "42", Message(kind, keyboard, filename));
        else await sender.Send(12, Message(kind, keyboard, filename));

        Assert.That(handler.Requests.Count, Is.EqualTo(3));
        Assert.That(handler.Requests[0].Uri.Query, Does.Contain("type=" + (kind == "photo" ? "image" : "file")));
        Assert.That(handler.Requests[1].Body, Does.Contain(filename ?? (kind == "file" ? "attachment.bin" : "photo.jpg")));
        Assert.That(handler.Requests[1].Body, Does.Contain(Encoding.UTF8.GetString(Content)));
        var request = handler.Requests[2];
        Assert.That(request.Method, Is.EqualTo(edit ? "PUT" : "POST"));
        Assert.That(request.Uri.Query, Does.Contain(edit ? "message_id=42" : "chat_id=12"));
        using var json = JsonDocument.Parse(request.Body);
        Assert.That(json.RootElement.GetProperty("text").GetString(), Is.EqualTo("caption"));
        var attachments = json.RootElement.GetProperty("attachments").EnumerateArray().ToArray();
        Assert.That(attachments.Length, Is.EqualTo(keyboard ? 2 : 1));
        Assert.That(attachments[0].GetProperty("type").GetString(), Is.EqualTo(kind == "photo" ? "image" : "file"));
        Assert.That(attachments[0].GetProperty("payload").GetRawText(), Does.Contain("uploaded-token"));
        if (keyboard) Assert.That(attachments[1].GetProperty("type").GetString(), Is.EqualTo("inline_keyboard"));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task MaxPlainTextDoesNotUploadOrAttachAnEmptyKeyboard(bool edit)
    {
        using var handler = new RecordingHttpHandler((_, _) => RecordingHttpHandler.Json(edit ? """{"success":true}""" :
            """{"success":true,"message":{"body":{"mid":"42","seq":1,"text":"caption"}}}"""));
        using var http = new HttpClient(handler);
        using var client = new MaxClient(new MaxBotOptions { Token = "test-token" }, httpClient: http);
        var sender = new MaxSampleMessageSender(client);
        if (edit) await sender.Edit(12, "42", Message("text", false));
        else await sender.Send(12, Message("text", false));
        var request = handler.Requests.Single();
        using var json = JsonDocument.Parse(request.Body);
        Assert.That(json.RootElement.GetProperty("text").GetString(), Is.EqualTo("caption"));
        if (json.RootElement.TryGetProperty("attachments", out var attachments) && attachments.ValueKind != JsonValueKind.Null)
            Assert.That(attachments.GetArrayLength(), Is.Zero);
    }

    [Test]
    public async Task TelegramPropagatesApiFailure()
    {
        using var handler = new RecordingHttpHandler((_, _) => RecordingHttpHandler.Json(
            """{"ok":false,"error_code":400,"description":"Bad Request: rejected"}""", System.Net.HttpStatusCode.BadRequest));
        using var http = new HttpClient(handler);
        var sender = new TelegramSampleMessageSender(new TelegramBotClient("123456:test-token", http));
        var error = await Assert.ThrowsAsync<global::Telegram.Bot.Exceptions.ApiRequestException>(() => sender.Send(12, Message("file", false)));
        Assert.That(error.ErrorCode, Is.EqualTo(400));
        Assert.That(handler.Requests.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task MaxDoesNotSendMessageWhenUploadAuthorizationFails()
    {
        using var handler = new RecordingHttpHandler((_, _) => RecordingHttpHandler.Json(
            """{"code":"unauthorized","message":"Invalid token"}""", System.Net.HttpStatusCode.Unauthorized));
        using var http = new HttpClient(handler);
        using var client = new MaxClient(new MaxBotOptions { Token = "test-token" }, httpClient: http);
        var sender = new MaxSampleMessageSender(client);
        await Assert.ThrowsAsync<global::Max.Bot.Exceptions.MaxUnauthorizedException>(() => sender.Send(12, Message("file", false)));
        Assert.That(handler.Requests.Single().Uri.AbsolutePath, Is.EqualTo("/uploads"));
    }
}
