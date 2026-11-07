---
name: bot-commands-testing
description: Validate Bot.Commands behavior and investigate routing, Fluent, adapter, or session persistence regressions with offline tests. Use for behavioral changes and requested test coverage work.
---

# Bot.Commands testing

Follow [root conventions](../../AGENTS.md). Select checks for the changed behavior; documentation-only or mechanical low-impact edits do not require new runtime tests.

## Behavioral checks

- Use synthetic SDK updates and fake senders. Do not call live Telegram, Max, or other messenger APIs for automated verification.
- For common behavior, cover both Telegram and Max where applicable; for a new adapter, exercise that adapter against the same shared command/Fluent expectations.
- For routing, check normal commands, callbacks, active sessions, behavior interception, and swarm/profile selection as relevant to the change. Preserve generic matcher dispatch.
- For Fluent changes, test entry, continuation, state transitions, callback exits, cancellation, and state restoration where affected. Assert the resulting messages/session behavior rather than matching implementation text.
- For execution filters, verify ordering and inheritance, authorization/chat gating, behavior composition, and that a filter failure prevents command execution and session mutation.
- For callback encoding, test round trips with `&` and `=` in data and avoid substring recognition of reserved keys such as `fid`. Preserve everything after the first key/value delimiter.

## Persistence regressions

Use a temporary local SQLite database with the real sample EntityStorage path when a fake store cannot expose the reported issue. Never use the developer's existing databases as test fixtures.

Check creation and continuation in a fresh request/scope, typed `Data` through interface-based serialization, the Fluent state wrapper, and the persisted `BOT_NAME` discriminator. Check create/update/completion and cross-bot isolation when relevant. A sent message alone does not demonstrate that a session was saved.

## Commands from the repository root

```powershell
dotnet build Telegram.Commands/src/Telegram.Commands.sln
dotnet test Telegram.Commands/src/Telegram.Command.Tests/Bot.Commands.Core.Tests/Bot.Commands.Core.Tests/Bot.Commands.Core.Tests.csproj -v quiet -m:1
```

Use a focused test filter during diagnosis. `--no-restore` requires a valid restore; `--no-build` requires a current build. If a running sample locks output files, the established alternate test output is `-p:OutputPath=bin/Validation/`.

Build/run affected documentation examples separately; they are outside the main solution. For coverage work, inspect and update [docs/testing.md](../../docs/testing.md), reporting actual uncovered behavior rather than pursuing a percentage alone.

Report what passed, failures or warnings that affect confidence, and which integrations were not exercised. Historical passing test counts are not an expected fixed total. Stage only task-owned changes with explicit paths.
