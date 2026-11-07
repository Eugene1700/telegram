using System.Text.Json;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Models;

namespace SurveyComparison;

// A sequential demo store, not a multi-process transactional database.
public sealed class FileSessionsStore(string path, IBotProfile profile) : ISessionsStore
{
    private string Key(long chat, long user) => $"{profile.BotName}|{chat}|{user}";
    private Dictionary<string, CommandSession> Load() => File.Exists(path)
        ? JsonSerializer.Deserialize<Dictionary<string, CommandSession>>(File.ReadAllText(path)) : new();
    public ISessionInfo GetSessionInfo(DateTime now, long chat, long user) => Read(now, chat, user, null, null);
    public ISessionInfoWithData GetSessionInfoWithData(DateTime now, long chat, long user, string command, Type type)
        => Read(now, chat, user, command, type);
    public ISessionInfoWithData<T> GetSessionInfoWithData<T>(DateTime now, long chat, long user, string command)
    {
        var value = Read(now, chat, user, command, typeof(T));
        return value == null ? null : new Typed<T>(value);
    }
    private CommandSession Read(DateTime now, long chat, long user, string command, Type type)
    {
        if (!Load().TryGetValue(Key(chat, user), out var value) || value.ExpiredAt <= now ||
            (command != null && command != value.CommandQuery)) return null;
        value.Data = type == null ? null : ((JsonElement)value.Data).Deserialize(type);
        return value;
    }
    public Task CreateSession(ISessionInfoWithData session) => Write(session, session.ChatId);
    public Task UpdateSession(ISessionInfoWithData session, long from) => Write(session, from);
    private Task Write(ISessionInfoWithData session, long from)
    {
        var sessions = Load();
        sessions.Remove(Key(from, session.UserId));
        if (!(session.ExpiredAt <= DateTime.UtcNow))
            sessions[Key(session.ChatId, session.UserId)] = new CommandSession { CommandQuery = session.CommandQuery,
                OpenedAt = session.OpenedAt, ExpiredAt = session.ExpiredAt, ChatId = session.ChatId, UserId = session.UserId, Data = session.Data };
        File.WriteAllText(path + ".tmp", JsonSerializer.Serialize(sessions));
        File.Move(path + ".tmp", path, true);
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
