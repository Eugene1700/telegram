using Bot.Commands.Abstract.Attributes;

namespace Bot.Commands.Abstract.Commands;

/// <summary>A typed adapter context. Covariance lets shared handlers accept any reference-type query.</summary>
public interface IQueryContext<out TQuery>
{
    TQuery Query { get; }
    long GetChatId();
    long GetFromId();
    string GetData();
    bool IsGroupMessage();
    bool IsEvent();
    EventType? GetEventType();
    ChatArea GetChatType();
    string? GetMessageId();
}
