using System.Collections.ObjectModel;
using System.Net;
using Bot.Commands.Abstract.Messages;
using Moq;
using SimpleHandlers.Services;
using VkNet.Abstractions;
using VkNet.Enums.StringEnums;
using VkNet.Model;

namespace Bot.Commands.Core.Tests;

public class VkMessageSenderTests
{
    [Test]
    public async Task TextSendAndEditUsePeerAndConversationIdAndRemoveOldButtons()
    {
        var messages = new Mock<IMessagesCategory>();
        MessagesSendParams sent = null;
        MessageEditParams edited = null;
        messages.Setup(m => m.SendAsync(It.IsAny<MessagesSendParams>(), It.IsAny<CancellationToken>()))
            .Callback<MessagesSendParams, CancellationToken>((p, _) => sent = p).ReturnsAsync(11);
        messages.Setup(m => m.EditAsync(It.IsAny<MessageEditParams>(), It.IsAny<CancellationToken>()))
            .Callback<MessageEditParams, CancellationToken>((p, _) => edited = p).ReturnsAsync(true);
        var client = new Mock<IVkApi>();
        client.SetupGet(c => c.Messages).Returns(messages.Object);
        using var http = new HttpClient(new RecordingHttpHandler((_, _) => throw new AssertionException("Unexpected upload")));
        var sender = new VkSampleMessageSender(client.Object, http);
        await sender.Send(42, new BotMessage("plain", null, null, null));
        await sender.Edit(42, "7", new BotMessage("updated", null, null, null));
        Assert.That(sent.PeerId, Is.EqualTo(42));
        Assert.That(sent.RandomId, Is.GreaterThan(0));
        Assert.That(sent.Message, Is.EqualTo("plain"));
        Assert.That(sent.Keyboard, Is.Null);
        Assert.That(edited.ConversationMessageId, Is.EqualTo(7));
        Assert.That(edited.MessageId, Is.Null);
        Assert.That(edited.PeerId, Is.EqualTo(42));
        Assert.That(edited.Keyboard.Buttons, Is.Empty);
    }

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public async Task AttachmentsUploadAndSaveBeforeSending(bool file, bool edit)
    {
        var messages = new Mock<IMessagesCategory>();
        var photos = new Mock<IPhotoCategory>();
        var docs = new Mock<IDocsCategory>();
        var client = new Mock<IVkApi>();
        client.SetupGet(c => c.Messages).Returns(messages.Object);
        client.SetupGet(c => c.Photo).Returns(photos.Object);
        client.SetupGet(c => c.Docs).Returns(docs.Object);
        var server = new UploadServerInfo { UploadUrl = "https://upload.example.test/media" };
        photos.Setup(p => p.GetMessagesUploadServerAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync(server);
        docs.Setup(d => d.GetMessagesUploadServerAsync(42, DocMessageType.Doc, It.IsAny<CancellationToken>())).ReturnsAsync(server);
        var photo = new Photo { Id = 12, OwnerId = 2 };
        photos.Setup(p => p.SaveMessagesPhotoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReadOnlyCollection<Photo>(new[] { photo }));
        var doc = Newtonsoft.Json.JsonConvert.DeserializeObject<Message>("""{"attachments":[{"type":"doc","doc":{"id":12,"owner_id":2,"title":"report.pdf","ext":"pdf","size":3,"date":1}}]}""").Attachments.Single();
        docs.Setup(d => d.SaveAsync(It.IsAny<string>(), "report.pdf", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReadOnlyCollection<Attachment>(new[] { doc }));
        MessagesSendParams sent = null;
        messages.Setup(m => m.SendAsync(It.IsAny<MessagesSendParams>(), It.IsAny<CancellationToken>()))
            .Callback<MessagesSendParams, CancellationToken>((p, _) => sent = p).ReturnsAsync(11);
        MessageEditParams edited = null;
        messages.Setup(m => m.EditAsync(It.IsAny<MessageEditParams>(), It.IsAny<CancellationToken>()))
            .Callback<MessageEditParams, CancellationToken>((p, _) => edited = p).ReturnsAsync(true);
        var handler = new RecordingHttpHandler((_, _) => RecordingHttpHandler.Json(file ? """{"file":"uploaded"}""" : """{"photo":"uploaded","server":1,"hash":"hash"}"""));
        using var http = new HttpClient(handler);
        var sender = new VkSampleMessageSender(client.Object, http);
        var message = new BotMessage("caption", new byte[] { 65, 66, 67 }, null, null)
        {
            SendAsFile = file, FileName = file ? "report.pdf" : "photo.jpg"
        };
        if (edit) await sender.Edit(42, "7", message);
        else await sender.Send(42, message);
        Assert.That(handler.Requests.Single().Body, Does.Contain(file ? "report.pdf" : "photo.jpg").And.Contain("ABC"));
        Assert.That((edit ? edited.Attachments : sent.Attachments).Single(), Is.SameAs(file ? doc.Instance : photo));
        Assert.That(edit ? edited.Message : sent.Message, Is.EqualTo("caption"));
        if (file) photos.Verify(p => p.GetMessagesUploadServerAsync(It.IsAny<long?>(), It.IsAny<CancellationToken>()), Times.Never);
        else docs.Verify(d => d.GetMessagesUploadServerAsync(It.IsAny<long?>(), It.IsAny<DocMessageType?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task FailedUploadDoesNotSendMessage()
    {
        var client = new Mock<IVkApi> { DefaultValue = DefaultValue.Mock };
        Mock.Get(client.Object.Photo).Setup(p => p.GetMessagesUploadServerAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UploadServerInfo { UploadUrl = "https://upload.example.test/media" });
        using var http = new HttpClient(new RecordingHttpHandler((_, _) => RecordingHttpHandler.Json("{}", HttpStatusCode.BadRequest)));
        var sender = new VkSampleMessageSender(client.Object, http);
        await Assert.ThrowsAsync<HttpRequestException>(() => sender.Send(42, new BotMessage("image", new byte[] { 1 }, null, null)));
        Mock.Get(client.Object.Messages).Verify(m => m.SendAsync(It.IsAny<MessagesSendParams>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
