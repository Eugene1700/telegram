# Project guidance

These conventions consolidate the project chats reviewed on 2026-11-07. Apply them to this repository; newer explicit user instructions take precedence.

## Shared conventions

- Keep Abstract, Core, UI, and common DependencyInjection independent of messenger SDKs and adapters. Platform addressing, formatting, update conversion, and keyboard conversion belong in adapters.
- Use `IQueryContext<TQuery>` throughout command and Fluent handlers. Preserve generic typing; do not reintroduce the removed `QueryContext<TQuery>` base class, a non-generic context, or conversion wrappers solely to erase types.
- Put shared command logic in generic methods or generic base classes. Concrete messenger descendants implement the SDK-specific interfaces and declare their swarms. Lower layers must not know those descendants.
- Declare each namespace-level type in its own file. Group commands by scenario, keeping their session data and state types in that scenario's folder. Nested implementation types may stay with their owner.
- Inject explicit dependencies into application classes; keep service selection and composition in the DI setup rather than passing `IServiceProvider` through business logic.
- In SimpleHandlers, isolate sessions by `BotName`, `ChatId`, and `UserId` in the application store. Do not change shared session contracts just to add a messenger. Preserve the EntityStorage integration; do not modify EntityStorage itself as a side effect. Do not add production migrations or reset databases merely to adjust a sample.
- Run behavioral checks with synthetic updates and fake senders, without real messenger API requests. Use local persistence integration tests when storage behavior is the subject of the change.
- Write public library documentation in English. The explicitly requested Russian Habr article remains Russian. Update examples and affected documentation with public API changes.
- Add task-owned source, test, documentation, and skill files to Git with explicit paths. Do not stage unrelated work, local databases, generated binaries, or IDE settings. Staging does not authorize a commit or push.

## Reusable skills

Read the relevant skill for the task:

- [Bot.Commands development](skills/bot-commands-development/SKILL.md): architecture, generic commands, adapters, session boundaries.
- [Bot.Commands testing](skills/bot-commands-testing/SKILL.md): routing, Fluent behavior, serialization, offline regression checks.
- [Bot.Commands documentation](skills/bot-commands-documentation/SKILL.md): English guides, runnable examples, the Russian article, SDK assessment.

## Sources and superseded decisions

Reviewed the available active history by project ID and repository directory; no archived chats were returned. The four preceding project chats were:

- `Доделать универсальные команды` — `01a19806-218a-7ef1-b64e-83480ec068bc`: architecture, typed contexts, file organization, persistence, tests, English docs, Git staging.
- `Дополнить статью о Telegram-ботах` — `01a1b744-e82d-7522-aa70-32e8807d5455`: Fluent/UI/events narrative, runnable report example for Telegram and Max, Git staging.
- `Оценить добавление адаптеров` — `01a1b784-1197-7bf3-8116-3668ca8856e9`: preserve session contracts, application-owned bot isolation, SDK-first adapter assessment. This chat was still in progress; its implementation is not treated as completed here.
- `Найти свежую альтернативу MaxMessenger.Bot` — `01a1a5f0-33d4-7b11-a79f-7855a2a95215`: investigate SDK availability and prerelease filtering; no blanket instruction to replace the SDK.

Later decisions supersede earlier suggestions: `BotName` replaces platform-only session isolation; `IQueryContext<TQuery>` replaces the concrete context base; common command bases do not implement messenger-specific interfaces. Historical test counts, package freshness claims, and one-time database recreation requests are not permanent requirements.
