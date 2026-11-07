using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Vk.Models;
using VkNet.Abstractions;
using VkNet.Enums.StringEnums;
using VkNet.Model;

namespace SimpleHandlers.Services;

public sealed class VkSampleMessageSender(IVkApi client, HttpClient uploads) : ISampleMessageSender
{
    public async Task Send(long chatId, IBotMessage message)
    {
        var text = message.ToVkText();
        var keyboard = message.ToInlineKeyboard();
        var attachments = await Upload(chatId, message);
        await client.Messages.SendAsync(new MessagesSendParams
        {
            PeerId = chatId, RandomId = RandomNumberGenerator.GetInt32(1, int.MaxValue),
            Message = text, Keyboard = keyboard, Attachments = attachments
        });
    }

    public async Task Edit(long chatId, string messageId, IBotMessage message)
    {
        var id = long.Parse(messageId, NumberStyles.None, CultureInfo.InvariantCulture);
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(messageId));
        var text = message.ToVkText();
        var keyboard = message.ToInlineKeyboard() ?? new MessageKeyboard
        {
            Inline = true, Buttons = Array.Empty<MessageKeyboardButton[]>()
        };
        var attachments = await Upload(chatId, message);
        await client.Messages.EditAsync(new MessageEditParams
        {
            PeerId = chatId, ConversationMessageId = id,
            Message = text, Keyboard = keyboard, Attachments = attachments
        });
    }

    private async Task<IEnumerable<MediaAttachment>> Upload(long peerId, IBotMessage message)
    {
        if (message.Photo is not { Length: > 0 }) return Array.Empty<MediaAttachment>();
        var server = message.SendAsFile
            ? await client.Docs.GetMessagesUploadServerAsync(peerId, DocMessageType.Doc)
            : await client.Photo.GetMessagesUploadServerAsync(peerId);
        using var multipart = new MultipartFormDataContent();
        var content = new ByteArrayContent(message.Photo);
        var fileName = message.FileName ?? (message.SendAsFile ? "attachment.bin" : "photo.jpg");
        var mediaType = message.SendAsFile ? "application/octet-stream" :
            System.IO.Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".png" => "image/png", ".gif" => "image/gif", ".webp" => "image/webp", _ => "image/jpeg"
            };
        content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        multipart.Add(content, message.SendAsFile ? "file" : "photo", fileName);
        using var response = await uploads.PostAsync(server.UploadUrl, multipart);
        response.EnsureSuccessStatusCode();
        var uploaded = await response.Content.ReadAsStringAsync();
        if (message.SendAsFile)
            return (await client.Docs.SaveAsync(uploaded, fileName, null)).Select(a => a.Instance).ToArray();
        return (await client.Photo.SaveMessagesPhotoAsync(uploaded)).Cast<MediaAttachment>().ToArray();
    }
}
