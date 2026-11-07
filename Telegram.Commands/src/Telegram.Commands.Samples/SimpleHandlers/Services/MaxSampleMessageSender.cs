using System.Threading.Tasks;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Max.Models;
using Bot.Commands.Telegram.Extensions;
using global::Telegram.Bot;
using global::Max.Bot;

namespace SimpleHandlers.Services;

public sealed class MaxSampleMessageSender(MaxClient client) : ISampleMessageSender
{
    public async Task Edit(long chatId, string messageId, IBotMessage message)
    {
        var attachments = new System.Collections.Generic.List<global::Max.Bot.Types.Requests.AttachmentRequest>();
        if (message.Photo is { Length: > 0 })
        {
            using var stream = new System.IO.MemoryStream(message.Photo, writable: false);
            var type = message.SendAsFile ? global::Max.Bot.Types.Enums.UploadType.File : global::Max.Bot.Types.Enums.UploadType.Image;
            var upload = await client.Files.UploadAsync(type, stream,
                message.FileName ?? (message.SendAsFile ? "attachment.bin" : "photo.jpg"));
            attachments.Add(global::Max.Bot.Types.Requests.AttachmentRequest.FromUpload(type, upload));
        }
        if (message.ToInlineKeyboard() is { } keyboard)
            attachments.Add(global::Max.Bot.Types.Requests.AttachmentRequest.Keyboard(keyboard));
        await client.Messages.EditMessageAsync(messageId, new global::Max.Bot.Types.Requests.EditMessageRequest
        {
            Text = message.Text, Format = message.Format.ToTextFormat(), Attachments = attachments.ToArray()
        });
    }
    public async Task Send(long chatId, IBotMessage message)
    {
        if (message.Photo is not { Length: > 0 })
        {
            await client.Messages.SendMessageAsync(chatId, message.Text, keyboard: message.ToInlineKeyboard(), format: message.Format.ToTextFormat());
            return;
        }
        using var stream = new System.IO.MemoryStream(message.Photo, writable: false);
        var type = message.SendAsFile ? global::Max.Bot.Types.Enums.UploadType.File : global::Max.Bot.Types.Enums.UploadType.Image;
        var upload = await client.Files.UploadAsync(type, stream,
            message.FileName ?? (message.SendAsFile ? "attachment.bin" : "photo.jpg"));
        var attachments = new System.Collections.Generic.List<global::Max.Bot.Types.Requests.AttachmentRequest>
        {
            global::Max.Bot.Types.Requests.AttachmentRequest.FromUpload(type, upload)
        };
        if (message.ToInlineKeyboard() is { } keyboard)
            attachments.Add(global::Max.Bot.Types.Requests.AttachmentRequest.Keyboard(keyboard));
        await client.Messages.SendMessageWithAttachmentsAsync(attachments, chatId: chatId,
            text: message.Text, format: message.Format.ToTextFormat());
    }
}