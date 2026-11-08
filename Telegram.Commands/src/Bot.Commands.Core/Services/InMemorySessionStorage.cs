using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Threading;

namespace Bot.Commands.Core.Services;

/// <summary>Shared process-local storage. Register one instance per service provider.</summary>
public sealed class InMemorySessionStorage
{
    internal readonly object Sync = new();
    internal readonly Dictionary<(string Bot, long Chat, long User), Entry> Sessions = new();
    internal readonly ConcurrentDictionary<string, SemaphoreSlim> Gates = new();
    internal readonly Dictionary<(string Bot, string Event), (string Fingerprint, long Chat, long User)> Receipts = new();
    internal sealed record Entry(string CommandQuery, DateTime OpenedAt, DateTime? ExpiredAt,
        long ChatId, long UserId, JsonElement Data);
}
