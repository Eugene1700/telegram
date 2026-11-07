# ReportsBot

A runnable companion to the [Russian Habr article](../../habr-article.md).
One shared Fluent flow runs through the Telegram or Max adapter, either offline
in the terminal or as a real bot using your own token. Requires the .NET 10 SDK.

## Run without a token

From the repository root:

```sh
dotnet run --project docs/examples/ReportsBot -- demo telegram
dotnet run --project docs/examples/ReportsBot -- demo max
```

Enter `/report`, a month such as `2026-10`, then `#1`, `#2`, etc. to press the
numbered buttons. The console uses real SDK updates, resolver scopes, Fluent
sessions and adapter-generated callback payloads. It does not call messenger APIs.

Commands:

- `/report`: period validation, multi-select department dropdown, CSV/HTML format,
  confirmation, backward navigation and file generation.
- `/archive`: your reports in pages of three, with file download buttons.
- `/help`: help without leaving the current flow.
- `/cancel`: release the current session.
- `/event`: simulate a member-added event (offline terminal only).
- `/quit`: leave the terminal demo.

Create at least four reports to see pagination. HTML can be opened in a browser
and printed to PDF; this example does not generate PDF files directly.

## Telegram with your own bot

Set `TELEGRAM_BOT_TOKEN` in the process environment using your own secret-management
or terminal workflow. The application reads it at startup; no token belongs in
source code or in this repository.

```sh
dotnet run --project docs/examples/ReportsBot -- telegram
```

Open the bot's private chat and send `/start` or `/report`. This example uses
sequential long polling. Use a dedicated test bot without an active webhook or
another poller. The application checks for a Telegram webhook and does not remove
it automatically. Callback actions edit the original menu; reports are sent as files.

## Max with your own bot

Set `MAX_BOT_TOKEN` in the process environment, then run:

```sh
dotnet run --project docs/examples/ReportsBot -- max
```

The Max host uses the SDK's updates endpoint for sequential long polling and the
files API for uploads. Use a dedicated bot configured for polling, without a
competing update consumer. Callback actions edit the menu. No token is bundled.

Run each bot as a separate process. Each process has its own profile, session store
and archive; Telegram and Max command implementations are selected with swarms.

## Automated offline check

```sh
dotnet run --project docs/examples/ReportsBot -- self-test
```

This throws on failure and exercises both adapters: invalid periods, help during
a session, empty selection, selecting/deselecting, backward navigation, HTML/CSV
contents, cancellation, completion, pagination, downloads, chat/user isolation,
and member-added events. No credentials or network calls are used by the check
after packages have been restored. It does not verify live API delivery.

## Source map

| File | Responsibility |
| --- | --- |
| `Flows.cs` | Shared generic report/archive flows, thin platform subclasses, help/cancel/events |
| `Reports.cs` | Session data, dropdown model, fictional report data and file generation |
| `Output.cs` | Console, Telegram and Max message/file delivery |
| `Program.cs` | DI, isolated bot profile, sequential polling and update scopes |
| `Demo.cs` | Interactive offline host and end-to-end smoke checks |
| `../QuickStart/Infrastructure.cs` | Linked teaching implementations of clock, authorization and memory store |

## Scope and limitations

- All amounts are deterministic fictional data, not a connection to a real reporting system.
- Reports are written below `AppContext.BaseDirectory/artifacts/<platform>`.
  The archive index and sessions live in memory and are lost on restart. Generated
  files remain on disk; the demo does not reload old files into its archive.
- Report/archive commands are private-chat commands. Group member events only send
  a welcome message directing users to a private chat.
- Authorization is intentionally open for this public-data demo. Replace it before
  connecting confidential reports. Archive access is scoped to chat and user.
- A flow request ID avoids generating a second report when retrying a failed send
  in the same process; this is not durable deduplication across restarts.
- Archive pages use a stable snapshot taken on entry. Reopen `/archive` to refresh.
  Old keyboards from completed or cancelled flows are not independently versioned;
  use the current menu. Persistent sessions, stale-button protection and production
  retry policies are separate integration work.
- The host logs exception types without SDK response bodies or token-bearing URLs.
  Polling/network failures stop the process; update failures show a generic message.

The sample is intentionally outside the main solution and does not change the
library or the existing SimpleHandlers application.
