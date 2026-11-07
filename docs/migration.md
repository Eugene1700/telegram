# Migrating to the messenger-independent API

[README](../README.md) · [Getting started](getting-started.md)

The shared projects now use `Bot.Commands.*` names and have no dependency on a messenger SDK or adapter. This is a breaking source/API migration: old Telegram-prefixed contracts are not retained as aliases in the shared assemblies. These instructions describe the source tree, not a numbered published release.

## Projects and namespaces

| Previous project/namespace | Current project/namespace |
| --- | --- |
| `Telegram.Commands.Abstract` | `Bot.Commands.Abstract` |
| `Telegram.Commands.Core` | `Bot.Commands.Core` |
| `Telegram.Commands.UI` | `Bot.Commands.UI` |
| `Telegram.Commands.DependencyInjection` | `Bot.Commands.DependencyInjection` |

Update project references and `using` directives. The folders and `.csproj` files under `Telegram.Commands/src` have been renamed accordingly. The top-level historical source directory and solution filename remain unchanged. Core's package ID is now `Bot.Commands.Core`; do not assume this package/version has already been published.

Both adapters now follow the same naming scheme:

| Previous adapter name | Current adapter name |
| --- | --- |
| Project/namespace `Telegram.Commands.Realization` | `Bot.Commands.Telegram` |
| Max namespace `Bot.Command.Max` | `Bot.Commands.Max` (project name unchanged) |
| `TelegramBotCommandService` | `TelegramCommandService` |

Resolver implementations are `Bot.Commands.Telegram.Services.TelegramCommandService`
and `Bot.Commands.Max.Services.MaxCommandService`. Update project paths and imports;
the `UseTelegramCommandsServices()` and `UseMaxCommandsServices()` extensions retain
their names and remain in `Bot.Commands.DependencyInjection`.

## Public contracts

| Previous name | Current name |
| --- | --- |
| `IQueryTelegramCommand<T>` | `IQueryCommand<T>` |
| `ISessionTelegramCommand<T, TData>` | `ISessionCommand<T, TData>` |
| `IBehaviorTelegramCommand<TData>` | `IBehaviorCommand<TData>` |
| `ITelegramCommandExecutionResult` | `ICommandExecutionResult` |
| `TelegramCommandExecutionResult` | `CommandExecutionResult` |
| `ITelegramCommandDescriptor` | `ICommandDescriptor` |
| `ITelegramEventDescriptor` | `IEventDescriptor` |
| `ITelegramCommandFactory` | `ICommandFactory` |
| `TelegramCommandFactory` | `CommandFactory` |
| `ITelegramCommandException` | `ICommandException` |
| `TelegramCommandsChatAreaException` | `CommandChatAreaException` |
| `TelegramCommandsPermissionException` | `CommandPermissionException` |
| `TelegramCommandsInternalException` | `CommandsInternalException` |
| `TelegramExtractionCommandException` | `CommandExtractionException` |
| `TelegramExtractionCommandInternalException` | `CommandExtractionInternalException` |
| `TelegramResult` | `BotResult` |
| `TelegramProfileExtensions` | `BotProfileExtensions` |

The neutral command interfaces are now the primary definitions. Behavior overloads, generic constraints, Fluent interception hooks, and result callbacks all use the neutral contracts too. Reflection-based consumers must update interface-name assumptions, not only imports.

## Update command signatures

If upgrading from the older direct-message API, query and session commands receive a context:

```csharp
// Earlier signature:
Task<ITelegramCommandExecutionResult> Execute(Message message);

// Current signature:
Task<ICommandExecutionResult> Execute(QueryContext<Message> context);
```

Use `context.GetChatId()` and `context.GetFromId()` for common metadata. Use `context.Query` for SDK-specific fields. Session handlers add their data argument after the context; behavior overloads receive `QueryContext<TQuery>` as well.

## Adapter registration and dependencies

The common DI project no longer references either adapter or UI. Reference the adapter your host uses:

```csharp
using Bot.Commands.DependencyInjection;

services.UseTelegramCommandsServices(); // Extension supplied by Bot.Commands.Telegram.
// OR
services.UseMaxCommandsServices();      // Extension supplied by Bot.Commands.Max.
```

Both adapters reference common DI and register their own resolver. `UseBotCommandsServices()` registers only the shared engine and discovered commands. Add a separate `Bot.Commands.UI` reference when using dropdown/pagination helpers.

The old concrete `TelegramCommandService` integration is replaced by `IBotCommandResolver<TUpdate>`. A Max-only application no longer needs the Telegram SDK through common DI. An application using both adapters must still isolate its profiles, clients, and session stores.

## Session data: migrate stored field names

`ISessionInfo`, `CommandSession`, and the sample store now use:

| Previous field/property | Current field/property |
| --- | --- |
| `TelegramChatId` | `ChatId` |
| `TelegramUserId` | `UserId` |

Update custom store implementations and any named-argument/reflection code. Existing JSON documents or database schemas do not migrate automatically. Either migrate persisted fields/columns before deployment or explicitly map the old storage names to the new properties inside your store implementation. Do not merely rename properties and assume existing sessions will deserialize correctly.

The bundled `SimpleHandlers` sample's domain properties have also changed. Its existing SQLite database may require a schema migration or a separately created demonstration database. No user database is modified by this source migration.

Keep the entire `FluentObject<TData, TState>` wrapper when persisting Fluent sessions. Migrate command names and state values if those change independently. Store bot/platform identity separately because it is still absent from the common session key.

## Addressing and formatting moved to adapters

`QueryContext<TQuery>.RequiresBotMention` has been removed. Core does not interpret `@` as a bot-address separator. Its command token parser preserves that character literally. Telegram mention validation and suffix removal now happen in `TelegramCommandService` for message contexts. Host code that previously called the common `ExtractCommand` expecting Telegram-specific suffix removal should use the adapter's resolver or implement an explicit addressing policy.

`TelegramFormatsHelper` and the former `MarkupMessageBuilder` have moved from Core to `Bot.Commands.Telegram.Extensions` and `Bot.Commands.Telegram.Services`, respectively. Rename `MarkupMessageBuilder` usages to `TelegramMarkupMessageBuilder`. Max exposes `MaxMarkupMessageBuilder` and `MaxInlineMarkupQueryBuilder` in its adapter. Plain text is now escaped consistently, while `AppendMarkdown` preserves raw markup; remove manual escaping from plain-text arguments.

Core's `MessageBuilderBase.GetMessages` no longer defaults to a 4096-character limit. Supply a platform-appropriate limit explicitly where needed. It remains a generic message builder, not a complete validator for SDK message limits.

## Outgoing messages and Fluent callbacks

| Earlier concept | Current API |
| --- | --- |
| Telegram-specific outgoing message abstractions | `ITextMessage`, `IBotMessage`, `BotMessage` |
| SDK markup on a common message | `Callbacks`, converted by the selected adapter |
| `TelegramParseMode` / `BotMessageParseMode` | Adapter-owned `TelegramTextFormat` or `MaxTextFormat`; null for plain text |
| `message.ReplyMarkup` | Telegram `message.ToReplyMarkup()` or Max `message.ToInlineKeyboard()` |

Preserve `CallbackDataWithCommand.Row` when adapting keyboards. Core keyboard command helpers include the SDK query type among their generic arguments.

Fluent `Entry`, `GlobalIntercept`, `Next`, `OnCallback`, and `Exit` still receive native SDK queries. Message senders receive contexts; an untyped sender should match `QueryContext<Message>`, not `Message`. See the [handler type table](fluent.md#handler-types-contexts-versus-sdk-objects). This namespace migration does not unify those callback signatures.

Avoid `AheadFluent(..., fire: true)` until `ExecuteResult.Fire` is handled by the resolver. See [commands and sessions](commands.md) for current result and expiry semantics.

## Verify the migration

Build the solution, run the tests, and run the [offline quick start](examples/QuickStart) for both platforms. The tests include shared-assembly dependency checks and a custom query context exercising Core without either messenger's SDK types.
