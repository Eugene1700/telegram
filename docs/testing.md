# Test coverage review

`InMemoryStorageTests` covers registration, sample configuration, BotName
isolation, detached JSON snapshots, expiry, completion, moves, rollback/retry and
duplicate delivery across two providers sharing a memory backend.
`ImmediatePipelineTests` covers a Fluent command firing another Fluent command,
preservation of its session on the next update, and bounded immediate cycles
with rollback. `Fire` saves the destination in `Next` and re-enters the ordinary
`QueryHandler`, including authorization, chat checks and execution filters.

Measured on 2026-11-04 using the installed Coverlet collector and the full NUnit
suite. The baseline had 63 passing cases; the updated suite has 89. No test makes
a real HTTP request: SDK clients use a recording HttpMessageHandler with synthetic
responses and dummy tokens. Persistence tests use isolated SQLite databases in
memory, with EntityStorage unchanged.

## Measured coverage

Percentages are line / branch coverage. EntityStorage is a referenced dependency
and is excluded from this summary. These measurements describe exercised code,
not completeness of platform support.

| Assembly | Before | After |
| --- | --- | --- |
| Bot.Commands.Abstract | 61.3% / 100.0% | 61.3% / 100.0% |
| Bot.Commands.Core | 77.0% / 65.2% | 80.0% / 67.0% |
| Bot.Commands.DependencyInjection | 100.0% / 91.7% | 100.0% / 91.7% |
| Bot.Commands.Max | 69.4% / 42.0% | 69.4% / 42.0% |
| Bot.Commands.Telegram | 47.2% / 50.7% | 47.2% / 52.2% |
| Bot.Commands.UI | 38.6% / 27.4% | 38.6% / 27.4% |
| SimpleHandlers | 73.5% / 45.6% | 81.4% / 64.0% |

Both sample sender implementations now have 100% line coverage, including their
async send/edit methods. This is request-contract coverage against the installed
SDKs; it does not validate acceptance by live messenger services.

## Added and strengthened scenarios

- Telegram text/photo/document send and edit: endpoint, multipart bytes, filename,
  caption, keyboard, chat and message identifiers.
- Max image/file upload and send/edit: upload type, multipart bytes, upload token,
  final request, caption, optional keyboard and fallback filename.
- Max plain text: no upload and no empty inline keyboard attachment.
- SDK failures: Telegram rejection propagates; Max upload authorization failure
  prevents subsequent upload/message requests.
- Fluent callback payload round trips: empty values, equals signs, ampersands,
  embedded field-like values, percent signs and Unicode.
- Ordinary text beginning with "fid" reaches the message handler.
- SQLite session persistence across request scopes for both Telegram and Max:
  BOT_NAME, chat/user identity, state/data updates, and session completion.
- ExitFromCallback cancellation through the actual resolver for both platforms,
  checking that the conversation session is released.

Two Core defects were reproduced by the new tests and fixed: reserved characters
corrupted callback values, and prefix matching misclassified ordinary text as a
Fluent callback. Generated callback values are now URI-escaped and decoded on
receipt; the callback marker must match the complete "fid=" key.

## Remaining gaps

- UI pagination and dropdown edge cases account for substantial uncovered branches.
- Telegram event-specific contexts and adapter convenience overloads still have
  incomplete coverage.
- Custom Fluent interception flags and result callbacks need a dedicated matrix.
- Webhook registration, application startup, and transport retry behavior are not
  comprehensively exercised.
- HTTP status/JSON fixtures cannot prove live API compatibility or messenger UI
  behavior. Live integration tests are deliberately outside this offline suite.

## Reproduce

From the repository root:

```powershell
dotnet test Telegram.Commands/src/Telegram.Command.Tests/Bot.Commands.Core.Tests/Bot.Commands.Core.Tests/Bot.Commands.Core.Tests.csproj --no-restore -m:1 -p:OutputPath=bin/Validation/ --collect:"XPlat Code Coverage" --results-directory .test-results
```

The Validation output directory avoids replacing assemblies used by a running
sample. Cobertura XML is produced under .test-results, which is ignored by Git.
The test fixtures do not load production bot credentials or the sample's dev.db.
