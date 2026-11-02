using Bot.Commands.Abstract.Interfaces;

namespace QuickStart;

public sealed class UtcClock : IClock
{
    public DateTime Now => DateTime.UtcNow;
}

public sealed class DemoProfile : IBotProfile
{
    // No SDK client is resolved in this offline example.
    public string Key => "";
    public string BaseUrl => "";
    public string UpdateRoute => "/updates";
    public string BotName => "example_bot";
    public GroupMessageHandlingStrategy GroupMessageHandlingStrategy => GroupMessageHandlingStrategy.Direct;
    public string[] Swarms => null;
}

public sealed class DemoAuthorization : IAuthProvider
{
    public Task<bool> AuthUser<TQuery>(long userId, ICommandExecutionContext<TQuery> context)
        => Task.FromResult(true); // Demo only: replace with your application's policy.
}

// A sequential, in-memory teaching example, not a production storage implementation.
public sealed class DemoSessionStore : ISessionsStore
{
    private readonly Dictionary<(long Chat, long User), ISessionInfoWithData> _sessions = new();

    public ISessionInfo GetSessionInfo(DateTime now, long chatId, long userId)
        => Find(now, chatId, userId);

    public ISessionInfoWithData GetSessionInfoWithData(DateTime now, long chatId,
        long userId, string commandQuery, Type sessionObject)
    {
        var session = Find(now, chatId, userId);
        if (session == null || session.CommandQuery != commandQuery) return null;
        if (session.Data != null && !sessionObject.IsInstanceOfType(session.Data))
            throw new InvalidOperationException("Stored session data has an unexpected type.");
        return session;
    }

    public ISessionInfoWithData<T> GetSessionInfoWithData<T>(DateTime now, long chatId,
        long userId, string commandQuery)
    {
        var session = GetSessionInfoWithData(now, chatId, userId, commandQuery, typeof(T));
        return session == null ? null : new TypedSession<T>(session);
    }

    public Task CreateSession(ISessionInfoWithData session)
    {
        _sessions[(session.ChatId, session.UserId)] = session;
        return Task.CompletedTask;
    }

    public Task UpdateSession(ISessionInfoWithData session, long chatIdFrom)
    {
        _sessions.Remove((chatIdFrom, session.UserId));
        return CreateSession(session);
    }

    private ISessionInfoWithData Find(DateTime now, long chatId, long userId)
    {
        if (!_sessions.TryGetValue((chatId, userId), out var session)) return null;
        return session.ExpiredAt == null || session.ExpiredAt > now ? session : null;
    }

    private sealed class TypedSession<T>(ISessionInfoWithData source) : ISessionInfoWithData<T>
    {
        public string CommandQuery => source.CommandQuery;
        public DateTime OpenedAt => source.OpenedAt;
        public DateTime? ExpiredAt => source.ExpiredAt;
        public long ChatId { get; set; } = source.ChatId;
        public long UserId { get; set; } = source.UserId;
        public T Data => (T)source.Data;
    }
}
