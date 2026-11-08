# Test coverage review

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

## VK adapter checks (2026-11-07)

`VkAdapterTests` adds 15 offline cases covering private/group routing, legacy
message shape, text matchers, native callback JSON and command payload round trips,
active sessions, ignored edits/outgoing replies/service actions, common keyboard
rows and links, unsupported button modes, missing IDs, plain text formatting, and
Fluent callback data preservation. The full suite passed 113 cases at this check.
QuickStart also passed for VK, Telegram, and Max.

These checks do not measure updated coverage percentages or exercise live VK
delivery, callback acknowledgement, upload APIs, keyboard limits, or webhook setup.
The coverage table above remains the earlier measurement.

## Remaining gaps

The later SimpleHandlers VK integration adds the same scenario suite for all
three platforms, request-filter/controller tests, mocked SDK send/edit/upload
checks, and an interleaved three-bot SQLite persistence test. The latter verifies
`BOT_NAME` isolation with equal chat/user IDs and restores Fluent state across
fresh scopes. Shared keyboard builder regression tests cover interface access,
typed overloads, command metadata, native modes, empty rows, and failed batches.
These additions do not update the historical coverage percentages above.

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

## WhatsApp coverage

WhatsAppApiTests verifies native Cloud API requests, errors, reply contexts,
keyboard limits and validation before sending menu chunks. WhatsAppWebhookTests
covers raw-body signatures, disabled endpoints, WABA/phone filtering and a fresh
scope per batched message. WhatsAppConfigurationTests covers profile selection,
BotName validation and opt-in subscription after HTTP startup.
SimpleHandlersPlatformTests exercises WhatsApp commands through the real sender
and a fake HTTP handler. ThreePlatformSessionTests now includes five platforms
with identical chat/user IDs in an isolated in-memory SQLite database. No live
WhatsApp account, approved-template delivery, duplicate-event queue or distributed
locking is tested.

## In-memory storage and survey comparison

InMemoryStorageTests checks UseInMemoryStorage registration, reuse across scopes,
BotName/provider isolation, JSON snapshots of Fluent state, expiry, completion
and chat moves. The standalone
[SurveyComparison](examples/SurveyComparison/README.md) runs the same questionnaire
through Bot.Commands and aiogram using actual subprocess restarts and compares
visible output. Its verification script is separate from dotnet test.

## Nested Fluent routing

FluentRoutingTests checks the serialized session route collection, parent links,
detached caller snapshots without nested collections, invalid route/result handling
and target authorization. The update transaction rolls back the destination
session saved by `Next` when authorization fails. The standalone
[FluentRouting example](examples/FluentRouting/README.md) checks 17 updates with
actual process restarts, plus the same sequence using in-memory storage. It
covers message and callback transitions, two nested callers and cancellation
without applying the child's success result. It does not test concurrent or
duplicate updates, live delivery or crashes during execution.

`InMemoryRoutingBehaviorTests` executes the actual SimpleHandlers `/routeorder`
scenario through both Telegram and Max resolvers. Independent service providers
share an in-memory backend. It checks nested return/cancellation, concurrent
duplicate callbacks, distinct overlapping updates, progress for other users/bots,
failed parent prompts, exceptions after resolver session writes, cancelled lock
waits, retries and duplicate completion after the session ends. Assertions cover
messages, command identity, persisted parent/child data and continuation state.
These are process-local behavior checks, not PostgreSQL transaction tests or
evidence of distributed durability. External side effects remain outside rollback.

The dynamic-routing test calls the same SimpleHandlers city flow from two
different callers, resumes at different states on a fresh DI container, and
checks caller identity, distinct route IDs and returned data without registering
route links. MessengerRoutingTests also covers Max `BotStarted`: typed event
dispatch, private-chat identity, locale and unchanged/absent start payload.
