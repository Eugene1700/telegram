# SimpleHandlers: Telegram and Max

`Telegram.Commands/src/Telegram.Commands.Samples/SimpleHandlers` is an ASP.NET Core
webhook application that can serve both bots at the same time.

## Command folders

Commands are grouped by scenario under `Services/Commands`: `Ask`, `Profile`,
`Photos`, `DropDown`, `MyFluent`, `GlobalIntercept`, `Start`, and `Cancel`.
Each folder contains the shared command logic, concrete Telegram/Max commands,
and its session data and state types. Shared helpers live in `Shared`.
The folder move preserves existing namespaces.

`Profile/ProfileFunnelMatcher.cs` implements `ICommandMatcher<TQuery, SharedProfileCommand>`.
Register closed types for each supported input, for example:

```csharp
services.AddScoped<ICommandMatcher<Telegram.Bot.Types.Message>,
    ProfileFunnelMatcher<Telegram.Bot.Types.Message>>();
services.AddScoped<ICommandMatcher<Max.Bot.Types.Message>,
    ProfileFunnelMatcher<Max.Bot.Types.Message>>();
```

`IsMatch(IQueryContext<TQuery> query)` receives a typed context. Message and callback
matchers are injected separately by each adapter; message matchers never handle
callback updates. A nonempty matcher list replaces name-based routing for incoming
commands, including slash commands; a failed match does not fall back to a name.
Active sessions still resolve their saved command. Behavior sessions use matchers
to select incoming commands to intercept.
Ambiguous matches are rejected; swarm, authorization and chat-area checks remain
in effect.

The sample registers only the profile funnel matcher for messages. In this mode,
start with `register` or `хочу зарегистрироваться`; slash commands do not start
other flows. Remove these two matcher registrations to use the slash-command
walkthrough below, or provide explicit matchers for the other entry commands.

## Configure and run

Use `appsettings.Example.json` as a configuration reference. It is not loaded
automatically. Merge the sections into your local configuration and provide tokens
through environment variables or your deployment's secret store. Keep your existing
Telegram `BotProfile` values when adding Max.

| Setting | Purpose |
| --- | --- |
| `BotProfile:Enabled` | Telegram webhook handler and registration; defaults to `true` |
| `MaxBotProfile:Enabled` | Max webhook handler and registration; defaults to `false` |
| `Token` in each profile | The corresponding bot token |
| `BotName` in each profile | Bot name; Telegram uses it for mention matching |
| `Platform` in each profile | Command platform; defaults to `telegram` / `max` for the corresponding profile |
| `BaseUrl` in each profile | Public HTTPS base URL reachable by that messenger |
| `RegisterWebhook` in each profile | Register at application startup; defaults to `true` |
| `MaxBotProfile:WebhookSecret` | Shared secret registered with Max and checked on incoming requests |
| `ConnectionStrings:Sessions` | Shared EntityStorage SQLite database; defaults to `Data Source=dev.db` |

For environment variables, replace `:` with `__`, for example
`MaxBotProfile__Token` and `MaxBotProfile__Enabled=true`. Set both `Enabled` flags
to `true` for simultaneous operation. The existing Telegram route remains valid:

- Telegram: `POST /api/message/update`
- Max: `POST /api/max/message/update`
- Health check: `GET /ping`

Run from the sample directory so relative database paths stay consistent:

```powershell
cd Telegram.Commands/src/Telegram.Commands.Samples/SimpleHandlers
dotnet run
```

If webhooks are managed outside the application, set `RegisterWebhook=false` for
the corresponding profile and register its route yourself. Max subscribes to
`message_created`, `message_callback`, and `bot_started`. The application retains
the Max subscription when stopping. Disabled webhook routes return HTTP 404.

## Shared commands

In a private chat with either bot:

1. `/start` sends a welcome message.
2. `/ask` opens a typed session; the next text message completes it.
3. `/profile` starts a Fluent conversation. Reply with your name or press **Use Alex**.

`StartCommand`, `AskCommand`, and `NameReplyCommand` implement both native message
interfaces. `SharedProfileCommand` shares its state machine and common callback
model. `ISampleMessageSender` converts outgoing messages and keyboards through the
selected adapter. DI selects `TelegramSampleMessageSender` or `MaxSampleMessageSender`;
each receives its SDK client directly. Controllers declare their resolver and client
as action parameters. `WebhookAccessFilter` checks platform availability and the Max
secret before those dependencies are resolved. Max's **Start** event sends the introductory message as well.

Concrete Telegram and Max commands declare `Platform` in their command attributes.
`BotProfile.Platform` selects implementations, including behavior and Fluent commands.
The bot profiles do not need platform swarms.

## Profiles and sessions

Set `Sessions:UseInMemory=true` (or `Sessions__UseInMemory=true` in the environment)
to select `services.UseInMemoryStorage()` instead of EntityStorage. This skips
the sample database reset. The default remains EntityStorage.

The in-memory store isolates sessions by `(BotName, ChatId, UserId)` and takes
JSON snapshots, including Fluent state. State is shared across DI scopes and
disappears when the process exits. An `IBotProfile` with a nonempty `BotName`
and an `IClock` must be registered.

For a process-local update boundary, resolve `InMemorySessionsStore` in a fresh
scope and call `ExecuteUpdateAsync(eventId, fingerprint, chatId, userId, handler)`.
It serializes updates to the same session, suppresses successful duplicate
deliveries and commits session changes only after the handler succeeds. Failed
updates may be retried with the same event ID. All writers must use this boundary.
The sample controllers do not enable it automatically. Separate service providers
can share an `InMemorySessionStorage` instance; separate processes cannot. Receipts
and locks remain for its lifetime. This provides neither distributed durability
nor rollback of external sends.

Every webhook request has its own DI scope. `BotRuntime` selects the profile and
sender. By default both bots use EntityStorage and the same `COMMAND_SESSION`
table in the configured database (default `dev2.db`).

Sessions are looked up by `(BotName, ChatId, UserId)`. Creation records the
current bot name; reads and updates are restricted to that bot.
Payloads use `System.Text.Json`.

With default storage, `Startup.Configure` uses EntityStorage's `EfDbContext` to call
`Database.EnsureDeleted()` followed by `Database.EnsureCreated()`. The configured
session database is recreated empty with the current schema, including `BOT_NAME`.
All previous sessions are discarded. No migrations or seed sessions are used.
EntityStorage itself is unchanged.

This is a teaching sample: authorization permits all users, and it does not provide
distributed per-session locking or update deduplication.

## Verification

`SimpleHandlersPlatformTests` runs the shared commands through both resolvers with
a recording sender. Both cases use a test session store and a fresh
scope per update, including a real serialized callback payload. These tests do not
call either messenger API or register live webhooks.
