using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Models;

namespace Bot.Commands.Core.Services;

/// <summary>Bot-scoped session access with JSON snapshots; data must be JSON serializable.</summary>
public sealed class InMemorySessionsStore : ISessionsStore
{
    private readonly InMemorySessionStorage _storage;
    private readonly IClock _clock;
    private readonly string _bot;
    private int _running;
    private bool _transaction;
    private (string Bot, long Chat, long User) _key;
    private InMemorySessionStorage.Entry _pending;

    /// <summary>
    /// Process-local atomic session update with duplicate suppression. All concurrent writers must use this
    /// boundary. Share the same InMemorySessionStorage across simulated instances; receipts are not durable.
    /// Remote sends are not rolled back. Use a fresh scope per update and do not access it concurrently.
    /// </summary>
    public async Task<bool> ExecuteUpdateAsync(string eventId, string fingerprint, long chatId, long userId,
        Func<CancellationToken, Task> handle, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventId);
        ArgumentException.ThrowIfNullOrWhiteSpace(fingerprint);
        ArgumentNullException.ThrowIfNull(handle);
        if (Interlocked.Exchange(ref _running, 1) != 0)
            throw new InvalidOperationException("Each update requires its own scope; nested execution is not supported.");
        var eventGate = _storage.Gates.GetOrAdd(JsonSerializer.Serialize(new[] { "event", _bot, eventId }), _ => new SemaphoreSlim(1));
        var sessionGate = _storage.Gates.GetOrAdd(JsonSerializer.Serialize(new[] { "session", _bot, chatId.ToString(), userId.ToString() }), _ => new SemaphoreSlim(1));
        var eventHeld = false;
        var sessionHeld = false;
        try
        {
            await eventGate.WaitAsync(cancellationToken); eventHeld = true;
            await sessionGate.WaitAsync(cancellationToken); sessionHeld = true;
            InMemorySessionStorage.Entry original;
            lock (_storage.Sync)
            {
                if (_storage.Receipts.TryGetValue((_bot, eventId), out var receipt))
                {
                    if (receipt != (fingerprint, chatId, userId))
                        throw new InvalidOperationException("Event ID was reused with different content or addressing.");
                    return false;
                }
                _key = (_bot, chatId, userId);
                _storage.Sessions.TryGetValue(_key, out original);
                _pending = original;
                _transaction = true;
            }
            await handle(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            lock (_storage.Sync)
            {
                _storage.Sessions.TryGetValue(_key, out var current);
                if (!ReferenceEquals(current, original))
                    throw new InvalidOperationException("Session was changed outside the update boundary.");
                if (_pending == null) _storage.Sessions.Remove(_key);
                else _storage.Sessions[_key] = _pending;
                _storage.Receipts.Add((_bot, eventId), (fingerprint, chatId, userId));
            }
            return true;
        }
        finally
        {
            _transaction = false; _pending = null;
            if (sessionHeld) sessionGate.Release();
            if (eventHeld) eventGate.Release();
            Volatile.Write(ref _running, 0);
        }
    }
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
            InMemorySessionStorage.Entry entry;
            if (_transaction)
            {
                if (key != _key) throw new InvalidOperationException("An update may access only its own session.");
                entry = _pending;
            }
            else _storage.Sessions.TryGetValue(key, out entry);
            if (entry == null || entry.ExpiredAt <= now) return null;
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
            if (_transaction)
            {
                if ((_bot, session.ChatId, session.UserId) != _key || (from.HasValue && from.Value != _key.Chat))
                    throw new InvalidOperationException("Cross-session writes are not supported inside an update.");
                _pending = session.ExpiredAt <= now ? null : new(session.CommandQuery, session.OpenedAt,
                    session.ExpiredAt, session.ChatId, session.UserId, data);
                return Task.CompletedTask;
            }
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
