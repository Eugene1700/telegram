---
name: bot-commands-documentation
description: Maintain Bot.Commands public guides, runnable samples, and its Habr article, or assess messenger SDK choices. Keep descriptions aligned with implemented behavior and the requested audience.
---

# Bot.Commands documentation and examples

Read [root conventions](../../AGENTS.md), [CONTRIBUTING.md](../../CONTRIBUTING.md), and the relevant implementation. Public library documentation is English; [the Habr article](../../docs/habr-article.md) is an explicitly requested Russian deliverable.

## Guides and examples

- Verify current API names, signatures, routing precedence, session lifetime behavior, and adapter support against source. Do not copy superseded context classes or Telegram-prefixed shared types from chat history.
- Explain the boundary between shared execution and application-owned delivery, clients, storage, and sending. Distinguish implemented support, SDK capabilities, plans, and untested live integrations.
- Keep complete examples runnable, and mark partial fragments and application-provided methods clearly. Use relative repository links.
- For cross-messenger scenarios, share the command logic and demonstrate Telegram and Max. Provide synthetic demo runs without tokens when appropriate; live modes accept the user's own tokens via configuration/environment.
- Do not include secrets or imply that passing synthetic demos verifies live APIs. Update migration notes when a public contract deliberately changes.

Existing example checks, run from the repository root when affected:

```powershell
dotnet run --project docs/examples/QuickStart -- telegram
dotnet run --project docs/examples/QuickStart -- max
dotnet run --project docs/examples/ReportsBot -- demo telegram
dotnet run --project docs/examples/ReportsBot -- demo max
```

## Habr narrative

Present the library as a coherent story around a practical dialogue, rather than a catalogue of interfaces. Explain commands and sessions, then show how FluentCommand expresses validation, transitions, callbacks, back navigation, and cancellation. Include UI helpers (dropdowns and pagination) and the event mechanism, connecting each to the same scenario. Show capabilities through working behavior, not unsupported superlatives. Keep the article and its ReportsBot application consistent.

## SDK assessment

Check current primary package/repository information when asked about maintenance, availability, or alternatives. Distinguish publication dates, prerelease status, target frameworks, claimed capabilities, and verified integration. If a NuGet package seems absent, check its exact ID and prerelease filtering. A version number or an old chat recommendation alone is not evidence of reliability or authorization to migrate dependencies.

Check links and affected example builds/runs before delivery. Add task-owned documentation and sample files to Git; do not commit or publish solely because files were staged.
