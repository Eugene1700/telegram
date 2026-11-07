using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Instagram;
using Bot.Commands.Instagram.Models;
using Instagram.Messaging;

namespace SimpleHandlers.Services;

public sealed class InstagramSampleMessageSender(IInstagramClient client) : ISampleMessageSender
{
    public async Task Send(long chatId, IBotMessage message)
    {
        var text = message.ToInstagramText();
        if (message.Photo is { Length: > 0 })
            throw new NotSupportedException("Publish media at an HTTPS URL and use IInstagramClient.SendImageAsync. Raw files/photos are not uploaded by this sample.");
        var callbacks = message.Callbacks?.ToArray() ?? Array.Empty<CallbackDataWithCommand>();
        // Validate every chunk before making any outgoing request.
        var chunks = callbacks.Chunk(3).Select(chunk =>
        {
            var builder = new InstagramInlineMarkupQueryBuilder();
            builder.InlineKeyboardButtonsRow(chunk);
            return builder.GetResult();
        }).ToArray();
        var recipient = chatId.ToString(CultureInfo.InvariantCulture);
        if (chunks.Length == 0) await client.SendTextAsync(recipient, text);
        else foreach (var chunk in chunks)
            await client.SendTextAsync(recipient, text, chunk);
    }

    // The Send API has no edit-message operation. Publish the updated menu as a new message.
    public Task Edit(long chatId, string messageId, IBotMessage message) => Send(chatId, message);
}
