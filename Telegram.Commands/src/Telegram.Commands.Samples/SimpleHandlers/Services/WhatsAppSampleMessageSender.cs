using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.WhatsApp;
using Bot.Commands.WhatsApp.Models;
using WhatsApp.Messaging;
using WhatsApp.Messaging.Models;

namespace SimpleHandlers.Services;

public sealed class WhatsAppSampleMessageSender(IWhatsAppClient client) : ISampleMessageSender
{
    public async Task Send(long chatId, IBotMessage message)
    {
        var text = message.ToWhatsAppText();
        if (message.Photo is { Length: > 0 })
            throw new NotSupportedException("Publish media at an HTTPS URL and use IWhatsAppClient.SendImageAsync. Raw files/photos are not uploaded by this sample.");
        var chunks = new List<IReadOnlyList<WhatsAppReplyButton>>();
        var current = new List<WhatsAppReplyButton>();
        foreach (var callback in message.Callbacks ?? Enumerable.Empty<CallbackDataWithCommand>())
        {
            // The sample adapts display labels only. Callback payloads are never truncated.
            var title = callback.Text;
            if (title?.Length > 20)
                title = title[..(char.IsHighSurrogate(title[18]) ? 18 : 19)] + "…";
            var button = new WhatsAppInlineMarkupQueryBuilder()
                .AddInlineKeyboardButton(callback with { Text = title }).GetResult().Single();
            if (current.Count == 3 || current.Any(b => b.Id == button.Id))
            {
                chunks.Add(current.ToArray());
                current.Clear();
            }
            current.Add(button);
        }
        if (current.Count > 0) chunks.Add(current.ToArray());
        // Validate every message before issuing any request.
        if (chunks.Count == 0) WhatsAppClient.ValidateText(text);
        foreach (var chunk in chunks) WhatsAppClient.ValidateText(text, chunk);
        var recipient = chatId.ToString(CultureInfo.InvariantCulture);
        if (chunks.Count == 0) await client.SendTextAsync(recipient, text);
        else foreach (var chunk in chunks)
            await client.SendTextAsync(recipient, text, chunk);
    }

    // Cloud API does not edit existing menus. Older menus remain visible.
    public Task Edit(long chatId, string messageId, IBotMessage message) => Send(chatId, message);
}
