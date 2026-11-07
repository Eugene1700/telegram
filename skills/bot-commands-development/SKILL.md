---
name: bot-commands-development
description: Implement or review Bot.Commands commands, Fluent flows, messenger adapters, and SimpleHandlers integration in this repository. Apply its architecture and persistence boundaries to relevant changes.
---

# Bot.Commands development

Use the [root conventions](../../AGENTS.md) and inspect the relevant existing implementation before editing. The historical source directory is `Telegram.Commands/src`; public shared contracts use `Bot.Commands.*`.

## Commands and contexts

- Keep shared logic messenger-independent and generic. Concrete descendants supply message/callback types and swarms; do not restore MessageCommandBase, MessageSessionCommandBase, CallbackCommandBase, or CallbackSessionCommandBase as shared classes that know both SDKs.
- Use `IQueryContext<TQuery>` for command and Fluent inputs; access native fields through `Query`. Prefer generic message/callback parameters in reusable Fluent scenarios over using `object` merely to avoid typing. Existing intentional universal handlers need not be rewritten without a task reason.
- Use `ICommandMatcher<TQuery>` for custom matching. Verify that matching selects the correct query type and preserves command routing precedence.
- Route cross-cutting behavior through the shared execution pipeline. For execution filters, preserve authorization/chat checks, inherited filters, ordering, and failure behavior for normal, session, and behavior-composed commands.
- Organize each command scenario with its data and state types; keep namespace-level types in separate files.

## Adapters and outgoing messages

- Follow the naming and layering of Telegram and Max adapters. Keep SDK references, incoming event mapping, command addressing, text escaping, and markup conversion in the adapter.
- Preserve typed platform formatting. Core passes the common format abstraction through; a common enum must not pretend Telegram MarkdownV2 and another messenger's Markdown are identical.
- Keep shared builder operations such as bold text and links reusable while each adapter owns escaping and its format. Respect file-versus-photo intent and filenames in outgoing messages.
- Before creating a new SDK, verify whether a suitable .NET SDK exists and covers the required updates, callbacks, keyboards, and delivery mode. Do not infer suitability from a package name alone.

## Sessions and application integration

- Keep delivery, SDK clients, outgoing transport, and persistence in the host/application. SimpleHandlers supports simultaneous bot profiles; selecting a profile must keep command chains and store access on that bot.
- Use the application-owned `BotName` discriminator for shared storage, including creation, lookup, and update. A new adapter is not a reason to change `ISessionsStore` or the shared session key contract.
- Preserve EntityStorage as the sample persistence integration. Handle sample schema setup at the application context boundary; do not introduce migrations or automatic deletion without an actual requirement.
- Treat command identity, data serialization, and Fluent state as persisted contracts. When changing them, examine existing continuation behavior and document any deliberate incompatibility.

For verification use [the testing skill](../bot-commands-testing/SKILL.md); for public API changes update the relevant guide and runnable example using [the documentation skill](../bot-commands-documentation/SKILL.md).
