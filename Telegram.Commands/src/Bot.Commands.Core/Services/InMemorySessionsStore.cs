using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Models;

namespace Bot.Commands.Core.Services;

/// <summary>Bot-scoped session access with JSON snapshots; data must be JSON serializable.</summary>
public sealed class InMemorySessionsStore : ISessionsStore
{
    private readonly InMemorySessionStorage _storage;
    private readonly IClock _clock;
    private readonly string _bot;
    public InMemorySessionsStore(InMemorySessionStorage storage, IBotProfile profile, IClock clock)
    {
        _storage = storage;
        _clock = clock;
        _bot = !string.IsNullOrWhiteSpace(profile.BotName) ? profile.BotName
            : throw new ArgumentException("BotName is required for in-memory session isolation.", nameof(profile));
    }
    public ISessionInfo GetSessionInfo(DateTime now, long chatId, long userId)
        => Read(now, chatId, userId, null, null);
    public ISessionInfoWithData GetSessionInfoWithData(DateTime now, long chatId, long userId, string commandQuery, Type sessionObject)
        => Read(now, chatId, userId, commandQuery, sessionObject);
    public ISessionInfoWithData<T> GetSessionInfoWithData<T>(DateTime now, long chatId, long userId, string commandQuery)
    {
        var value = Read(now, chatId, userId, commandQuery, typeof(T));
        return value == null ? null : new Typed<T>(value);
    }
    private CommandSession Read(DateTime now, long chat, long user, string command, Type type)
    {
        lock (_storage.Sync)
        {
            var key = (_bot, chat, user);
            if (!_storage.Sessions.TryGetValue(key, out var entry)) return null;
            if (entry.ExpiredAt <= now) { _storage.Sessions.Remove(key); return null; }
            if (command != null && command != entry.CommandQuery) return null;
            return new CommandSession { ChatId = chat, UserId = user, CommandQuery = entry.CommandQuery,
                OpenedAt = entry.OpenedAt, ExpiredAt = entry.ExpiredAt,
                Data = type == null ? null : entry.Data.Deserialize(type) };
        }
    }
    public Task CreateSession(ISessionInfoWithData session) => Write(session, null);
    public Task UpdateSession(ISessionInfoWithData session, long chatIdFrom) => Write(session, chatIdFrom);
    private Task Write(ISessionInfoWithData session, long? from)
    {
        var now = _clock.Now;
        var data = session.ExpiredAt <= now ? default : JsonSerializer.SerializeToElement(session.Data);
        lock (_storage.Sync)
        {
            foreach (var key in _storage.Sessions.Where(p => p.Value.ExpiredAt <= now).Select(p => p.Key).ToArray())
                _storage.Sessions.Remove(key);
            if (from.HasValue) _storage.Sessions.Remove((_bot, from.Value, session.UserId));
            var target = (_bot, session.ChatId, session.UserId);
            if (session.ExpiredAt <= now) _storage.Sessions.Remove(target);
            else _storage.Sessions[target] = new(session.CommandQuery, session.OpenedAt, session.ExpiredAt,
                session.ChatId, session.UserId, data);
        }
        return Task.CompletedTask;
    }
    private sealed class Typed<T>(CommandSession value) : ISessionInfoWithData<T>
    {
        public string CommandQuery => value.CommandQuery;
        public DateTime OpenedAt => value.OpenedAt;
        public DateTime? ExpiredAt => value.ExpiredAt;
        public long ChatId { get; set; } = value.ChatId;
        public long UserId { get; set; } = value.UserId;
        public T Data => (T)value.Data;
    }
}
