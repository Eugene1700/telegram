using Instagram.Messaging.Models;
namespace Instagram.Messaging;

public interface IInstagramClient
{
    Task SubscribeToMessagingAsync(CancellationToken cancellationToken = default);
    Task<InstagramSendResult> SendTextAsync(string recipientId, string text, IReadOnlyList<InstagramButton>? buttons = null,
        CancellationToken cancellationToken = default);
    Task<InstagramSendResult> SendImageAsync(string recipientId, Uri imageUrl, CancellationToken cancellationToken = default);
}
