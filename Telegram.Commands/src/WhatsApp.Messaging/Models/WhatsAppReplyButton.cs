namespace WhatsApp.Messaging.Models;

public sealed record WhatsAppReplyButton(string Id, string Title)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id) || Id.Length > 256 || Id != Id.Trim())
            throw new ArgumentException("WhatsApp reply button ID must contain 1–256 characters without surrounding spaces.");
        if (string.IsNullOrWhiteSpace(Title) || Title.Length > 20)
            throw new ArgumentException("WhatsApp reply button title must contain 1–20 characters.");
    }
}
