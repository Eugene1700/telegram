using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Bot.Commands.Core.Services;

/// <summary>Shared process-local storage. Register one instance per service provider.</summary>
public sealed class InMemorySessionStorage
{
    internal readonly object Sync = new();
    internal readonly Dictionary<(string Bot, long Chat, long User), Entry> Sessions = new();
    internal sealed record Entry(string CommandQuery, DateTime OpenedAt, DateTime? ExpiredAt,
        long ChatId, long UserId, JsonElement Data);
}
