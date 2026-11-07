using Bot.Commands.Abstract.Attributes;

namespace Bot.Commands.Abstract.Commands;

public abstract class QueryContext<TQuery>(TQuery query) : IQueryContext<TQuery>
{
    public TQuery Query { get; } = query;
    public abstract long GetChatId();
    public abstract long GetFromId();
    public abstract string GetData();
    public abstract bool IsGroupMessage();
    public abstract bool IsEvent();
    public abstract EventType? GetEventType();
    public abstract ChatArea GetChatType();
    public abstract string? GetMessageId();
}
