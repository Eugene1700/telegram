# SimpleHandlers: Telegram and Max

`Telegram.Commands/src/Telegram.Commands.Samples/SimpleHandlers` is an ASP.NET Core
webhook application that can serve both bots at the same time.

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

The older photo, dropdown, `/myfluent`, and `/global` demonstrations remain
Telegram-specific. Their behavior commands are restricted to the `telegram` swarm,
so Max cannot accidentally execute a handler requiring a Telegram client.

## Profiles and sessions

Every webhook request has its own DI scope. `BotRuntime` selects the profile and
sender and supplies the platform to the shared `SessionStore`. Both bots use
EntityStorage and the same `COMMAND_SESSION` table in `dev.db`.

Sessions are looked up by `(Platform, ChatId, UserId)`. `PLATFORM` is an enum column:
`Telegram = 0` (the default) and `Max = 1`. Creation records the current platform;
reads and updates are restricted to that platform. Payloads use `System.Text.Json`.

This sample has no schema migrations. Use a sample database whose schema includes
`PLATFORM`; an older database without that column must be recreated or adjusted
manually. Existing records assigned the default value belong to Telegram.
EntityStorage itself is unchanged.

This is a teaching sample: authorization permits all users, and it does not provide
distributed per-session locking or update deduplication.

## Verification

`SimpleHandlersPlatformTests` runs the shared commands through both resolvers with
a recording sender. Both cases use a test session store and a fresh
scope per update, including a real serialized callback payload. These tests do not
call either messenger API or register live webhooks.
