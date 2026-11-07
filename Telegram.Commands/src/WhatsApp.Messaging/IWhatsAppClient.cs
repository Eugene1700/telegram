using WhatsApp.Messaging.Models;
namespace WhatsApp.Messaging;

public interface IWhatsAppClient
{
    Task<string> SendTextAsync(string recipientId, string text, IReadOnlyList<WhatsAppReplyButton>? buttons = null, CancellationToken cancellationToken = default);
    Task<string> SendImageAsync(string recipientId, Uri imageUrl, CancellationToken cancellationToken = default);
    Task SubscribeToMessagingAsync(CancellationToken cancellationToken = default);
}
