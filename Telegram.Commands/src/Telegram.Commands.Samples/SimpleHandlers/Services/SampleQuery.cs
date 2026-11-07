using System;
using System.Linq;
using Bot.Commands.Abstract.Commands;
using SimpleHandlers.Services.Commands.Models;
using Tg = global::Telegram.Bot.Types;
using Mx = global::Max.Bot.Types;
namespace SimpleHandlers.Services;

public static class SampleQuery
{
    public static long ChatId(object query) => query switch
    {
        IQueryContext<object> c => c.GetChatId(),
        Tg.Message m => m.Chat.Id,
        Tg.CallbackQuery c => c.Message.Chat.Id,
        Mx.Message m => m.Recipient.ChatId ?? throw new InvalidOperationException("Missing chat ID"),
        Mx.CallbackQuery c => c.Message.Recipient.ChatId ?? throw new InvalidOperationException("Missing chat ID"),
        _ => throw new InvalidOperationException("Unsupported query")
    };
    public static string Text(object query) => query switch { IQueryContext<object> c => c.GetData(), Tg.Message m => m.Text, Mx.Message m => m.Text, _ => null };
    public static string CallbackMessageId(object query) => query switch
    {
        IQueryContext<Tg.CallbackQuery> c => c.GetMessageId(),
        IQueryContext<Mx.CallbackQuery> c => c.GetMessageId(),
        IQueryContext<VkNet.Model.MessageEvent> c => c.GetMessageId(),
        IQueryContext<Instagram.Messaging.Models.InstagramPostback> c => c.GetMessageId(),
        IQueryContext<WhatsApp.Messaging.Models.WhatsAppReply> c => c.GetMessageId(),
        _ => null
    };
    public static PhotoSessionObject Photo(object query)
    {
        if (query is WhatsApp.Messaging.Models.WhatsAppMessage { Type: "image", Image: not null } whatsapp)
            return new PhotoSessionObject { MessageId = whatsapp.Id, PhotoSize = null };
        if (query is Instagram.Messaging.Models.InstagramMessage instagram &&
            instagram.Attachments?.Any(a => a.Type == "image") == true)
            return new PhotoSessionObject { MessageId = instagram.Mid, PhotoSize = null };
        if (query is VkNet.Model.Message vk && vk.Attachments?.Any(a => a.Instance is VkNet.Model.Photo) == true)
            return new PhotoSessionObject { MessageId = vk.ConversationMessageId?.ToString(), PhotoSize = null };
        if (query is Tg.Message t && t.Photo?.Length > 0)
            return new PhotoSessionObject { MessageId = t.MessageId.ToString(), PhotoSize = t.Photo.Max(p => p.FileSize) };
        if (query is Mx.Message m && m.Body?.Attachments?.OfType<Mx.PhotoAttachment>().FirstOrDefault() is { } photo)
            return new PhotoSessionObject { MessageId = m.Mid, PhotoSize = photo.FileSize };
        return null;
    }
}
