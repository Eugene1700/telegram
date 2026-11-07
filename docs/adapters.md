# Messenger adapters

[README](../README.md) · [Getting started](getting-started.md) · [Commands](commands.md)

The adapters translate incoming SDK updates into `IQueryContext<TQuery>` instances.

Adapter contexts implement this interface directly; there is no required base
class. They retain the original SDK object in `Query` and provide common metadata
through the interface methods.
They do not normalize every SDK object into a new universal message type. Common
metadata is available through the context; platform-specific fields remain on `Query`.

## Supported updates

### Telegram

Register with `UseTelegramCommandsServices()` and resolve
`IBotCommandResolver<Telegram.Bot.Types.Update>`.

| SDK update | Query type | Handling |
| --- | --- | --- |
| `Message` | `Telegram.Bot.Types.Message` | Text commands or input to an active session |
| `Message` with `NewChatMembers` | `Telegram.Bot.Types.Message` | `EventType.ChatMemberAdded` |
| `CallbackQuery` | `Telegram.Bot.Types.CallbackQuery` | Command payload or Fluent callback |
| `PreCheckoutQuery` | `Telegram.Bot.Types.Payments.PreCheckoutQuery` | Command extraction from `InvoicePayload` |
| `MyChatMember` | `Telegram.Bot.Types.ChatMemberUpdated` | `EventType.BotMemberAddedToChat` |
| Other update types | — | Ignored by this resolver |

For nontext messages, `GetData()` returns null, but `Query` still contains the
SDK message, so an active photo/session handler can inspect it. `ChatArea.Channel`
exists in the common model; that does not mean `ChannelPost` updates are handled.

The current `MyChatMember` context maps every such update to
`BotMemberAddedToChat`. An event handler should inspect the old/new membership
statuses before treating it as an actual bot addition.

Callback contexts require a chat-backed message. Inline-mode callbacks without
`CallbackQuery.Message` are not supported by the default context. Pre-checkout
contexts use the initiating user's ID as the chat ID and expose no message ID.
Payment/callback acknowledgments remain SDK integration responsibilities.

### Max

Register with `UseMaxCommandsServices()` and resolve
`IBotCommandResolver<Max.Bot.Types.Update>`.

| SDK update | Query type | Handling |
| --- | --- | --- |
| `MessageCreated` | `Max.Bot.Types.Message` | Commands or input to an active session |
| `MessageCallback` | `Max.Bot.Types.CallbackQuery` | Command payload or Fluent callback |
| `UserAdded` | `Max.Bot.Types.UserAddedUpdate` | `EventType.ChatMemberAdded` |
| Other update types | — | Ignored by this resolver |

The adapter supports the shapes used in these JSON updates:

- Message chat metadata can come from `Message.Recipient` when `Update.Chat` is absent.
- A callback's message can be the top-level `Update.Message`, outside `Update.Callback`.
- A user-added update can identify its chat through top-level `Update.ChatId`.

When manually constructing `MessageCallbackContext`, pass the top-level message
as its third argument if the callback does not contain it. When constructing
`UserAddedUpdateContext`, pass the top-level chat ID if necessary. The resolver
does this for you.

Max chat mapping is `dialog → Private`, `chat → Group`, and `channel → Channel`.
For user-added events with no chat object, the context assumes a group chat.
Unknown chat-type values are rejected rather than mapped silently.

`BotStarted` is not currently translated to `/start`. If your host needs that
behavior, explicitly handle the update or extend the adapter.

### VK

Register with `UseVkCommandsServices()` and resolve
`IBotCommandResolver<VkNet.Model.GroupUpdate>`. See [VK integration](#vk-integration)
for supported events, keyboard conversion, and host responsibilities.

## Command syntax

VK community bots support `/command arguments` in private conversations and chats.
The adapter does not require or strip Telegram-style `@bot` suffixes or VK mentions.
Use `ICommandMatcher<VkNet.Model.Message>` for non-command entry text.

| Context | Example |
| --- | --- |
| Telegram private message | `/hello` or `/hello arguments` |
| Telegram group/supergroup text message | `/hello@your_bot arguments` |
| Telegram callback | `/hello payload` |
| Max message or callback | `/hello arguments` |

Telegram group text commands without the matching mention are ignored when there
is no active session. Max and callbacks do not require a Telegram bot mention.
Existing sessions and behavior interception have their own precedence; see
[session routing](commands.md#a-two-step-interaction).

Mention validation and removal of the `@bot` suffix happen in the Telegram adapter.
The common parser does not assign any special meaning to `@`; other adapters may
define their own addressing policy. `RequiresBotMention` is not part of the common
query context. `IBotProfile.GroupMessageHandlingStrategy` is currently not consulted;
setting it to `All` or `Ignore` does not override the Telegram adapter's routing rules.

## Common outgoing messages

The common model is `IBotMessage`:

- `Text`: outgoing text.
- `Photo`: optional attachment bytes (the existing property name is retained).
- `SendAsFile`: false sends the bytes as a photo; true sends them as a file/document.
- `FileName`: optional filename including its extension. Sample senders default to
  `photo.jpg` for photos or `attachment.bin` for files.
- `Format`: an optional adapter-owned `IMessageTextFormat`; null means plain text.
- `Callbacks`: common callback descriptors, including their `Row` index.

To send an attachment as a file, use `new BotMessage("Caption", bytes, null, null)
{ SendAsFile = true, FileName = "report.pdf" }`. Builders also expose
`AddFile(bytes, "report.pdf")` and `AddPhoto(bytes)`. Fluent preserves these fields.
The sample Telegram and Max senders upload the attachment using the selected mode,
with `Text` as its caption and the configured keyboard. Without attachment bytes,
they send an ordinary text message.

Use the Core `InlineMarkupQueryBuilder` to retain row boundaries. For example:

```csharp
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Services;

var keyboard = new InlineMarkupQueryBuilder();
keyboard.InlineKeyboardButtonsRow(new[]
{
    new CallbackDataWithCommand { Text = "Accept", CallbackText = "/accept" },
    new CallbackDataWithCommand { Text = "Decline", CallbackText = "/decline" }
});
keyboard.InlineKeyboardButtonsRow(new[]
{
    new CallbackDataWithCommand
    {
        Text = "Project website",
        CallbackMode = CallbackMode.Url,
        CallbackText = "https://example.com"
    }
});
var message = new BotMessage("Choose an action", null,
    null, keyboard.GetCallbacks());
```

The command names in this example require corresponding application handlers.
If `CommandDescriptor` is set, the adapter prefixes the callback payload with
`/{descriptor.Name} `; do not add the same prefix twice.

### Telegram conversion

Import `Bot.Commands.Telegram.Extensions`:

```csharp
var markup = message.ToReplyMarkup();
var parseMode = message.Format.ToParseMode();
// Pass markup and parseMode to Telegram.Bot send/edit methods.
```

The Core keyboard builder also has a Telegram `GetResult()` extension. The
adapter-specific `TelegramInlineMarkupQueryBuilder` can be used directly when
the keyboard is intentionally Telegram-only.

### Max conversion

Import `Bot.Commands.Max.Models`:

```csharp
var markup = message.ToInlineKeyboard();
var format = message.Format.ToTextFormat();
// Attach markup and format using the Max SDK's message request API.
```

Conversion creates SDK values; it does not send a message or upload a photo.

Formatting belongs to adapters. Core preserves the format without interpreting it.
Use `TelegramTextFormat(Telegram.Bot.Types.Enums.ParseMode)` for Telegram and
`MaxTextFormat(Max.Bot.Types.Enums.TextFormat)` for Max. Passing a format to the
wrong adapter throws `NotSupportedException`.

`TelegramMarkupMessageBuilder` and `MaxMarkupMessageBuilder` set their own format
automatically and escape text for their platform. Select the appropriate builder
in your application adapter. Raw Markdown text is not portable between messengers.

| Button mode | Telegram | Max |
| --- | --- | --- |
| `CallbackData` | Callback button | Callback button |
| `Url` | URL button | Link button |
| `Inline` / `InlineCurrent` | Inline-query buttons | Throws `NotSupportedException` |
| `Payment` | Pay button | Throws `NotSupportedException` |

The common model describes supported conversions, not complete SDK feature parity.
Check payload size and content requirements in your SDK integration, especially
for generated keyboards and user-provided data.

Telegram MarkdownV2 escaping (`TelegramFormatsHelper`) and `TelegramMarkupMessageBuilder`
live in `Bot.Commands.Telegram.Extensions` and
`Bot.Commands.Telegram.Services`, respectively. Core's `MessageBuilderBase`
does not choose a platform format or default message-length limit. Pass a limit
explicitly when your adapter/application requires one.

### Message and keyboard builders

All native keyboard builders now derive from the SDK-independent
`AdapterInlineMarkupQueryBuilder<TBuilder, TQuery, TButton, TMarkup>`, which
extends Core's `InlineMarkupQueryBuilder`. Core owns callback descriptors,
command metadata, row numbering, and typed overloads. Each adapter supplies
button conversion and the native markup container. Existing adapter method names
and fluent return types remain available; native builders also implement
`IInlineMarkupQueryBuilder` and expose `GetCallbacks()`.

Array overloads still add one button per row, explicit rows stay grouped, and
unsupported native button modes fail during insertion before modifying the builder.

The adapters expose matching builder APIs:

| Telegram | Max |
| --- | --- |
| `Bot.Commands.Telegram.Services.TelegramMarkupMessageBuilder` | `Bot.Commands.Max.Services.MaxMarkupMessageBuilder` |
| `Bot.Commands.Telegram.TelegramInlineMarkupQueryBuilder` | `Bot.Commands.Max.MaxInlineMarkupQueryBuilder` |

```csharp
var telegram = new TelegramMarkupMessageBuilder()
    .AppendBoldLine("Order confirmed")
    .AppendPlain("Reference: ").AppendCopyContent("order_42")
    .GetMessages();
var max = new MaxMarkupMessageBuilder()
    .AppendBoldLine("Order confirmed")
    .AppendPlain("Reference: ").AppendCopyContent("order_42")
    .GetMessages();
```

Import the respective `Services` namespaces. Plain text and formatting-helper
arguments are escaped; `AppendMarkdown` accepts trusted raw markup. Bold and
underline delimiters follow each platform's syntax, documented by
[Telegram](https://core.telegram.org/bots/api#markdownv2-style) and
[Max](https://dev.max.ru/docs-api/use-cases/sending-messages/text-formatting).
Messages retain the common `IBotMessage` contract, including photos and callbacks.
Length splitting happens between appended segments; it does not split a single
oversized segment or validate raw markup spanning several segments.

Both keyboard builders provide `AddInlineKeyboardButton<TCommand>` and
`AddInlineKeyboardButton<TCommand, TSessionObject>` for native callback commands.
`AddInlineKeyboardButtons` adds one button per row; use
`InlineKeyboardButtonsRow` to place several buttons on one row. `GetResult()`
returns the native SDK keyboard. Max supports the common callback and URL modes;
other modes throw `NotSupportedException`.

Common `InlineMarkupQueryBuilder` instances also have adapter-specific `GetResult`
extensions. When importing both extension namespaces, call the desired extension
class explicitly to avoid overload ambiguity.

## VK integration

`Bot.Commands.Vk` uses [VkNet](https://github.com/vknet/vk) **1.78.0**.
It accepts community updates from Callback API or Bots Long Poll through
`IBotCommandResolver<VkNet.Model.GroupUpdate>`.

| VK update | Query type | Behavior |
| --- | --- | --- |
| `message_new` with `MessageNew.Message` | `VkNet.Model.Message` | Command, matcher, or active session |
| Legacy `message_new` with a direct `Message` | `VkNet.Model.Message` | Same routing |
| `message_event` | `VkNet.Model.MessageEvent` | Command payload or Fluent callback |
| Message with a chat service `Action` | — | Ignored; does not advance a session |
| `message_edit`, `message_reply`, confirmation, other events | — | Ignored by the resolver |

This adapter does not currently translate membership actions to `EventType`.
Neither community subscribers nor membership actions are ordinary command input.

Registration and dispatch fragment (the host supplies the profile, authorization,
clock, persistent store, validated JSON body, and service provider):

```csharp
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using VkNet.Model;

services.UseVkCommandsServices();

// VkNet models use Newtonsoft.Json converters, not System.Text.Json.
var update = JsonConvert.DeserializeObject<GroupUpdate>(validatedJson)
    ?? throw new InvalidOperationException("Empty VK update.");
using var scope = provider.CreateScope();
await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<GroupUpdate>>()
    .Handle(update);
```

Register the VkNet client separately; this extension only registers command services.
Use a community token and enable the required incoming event types in your host.
The host handles Callback API confirmation, secret/group validation, HTTP responses,
deduplication, and ordered processing. It also acknowledges `message_event` using
VkNet's `Messages.SendMessageEventAnswerAsync` with the native event's
`EventId`, `UserId`, and `PeerId`. Resolver completion does not acknowledge the
button to VK. Apply your host's error and retry policy to both processing and acknowledgement.

### Context and sessions

`VkMessageQueryContext` uses `PeerId` / `FromId`;
`VkMessageEventContext` uses `PeerId` / `UserId`.
Peers at or above `2_000_000_000` are mapped to `ChatArea.Group`, other peers
to `ChatArea.Private`. Missing peer/user IDs are rejected.

Both contexts return `ConversationMessageId` as a string from `GetMessageId()`,
or null when absent. This is **local to the peer**, not the global message `Id`.
Use it with the peer and VK API parameters that expect a conversation-local ID;
the native message remains available through `Query`.

Session contracts are unchanged. In user code, partition the store by a unique
`IBotProfile.BotName`, as in `SimpleHandlers`, and resolve the matching profile
for each update.

### Text and keyboards

```csharp
using Bot.Commands.Vk.Models;
using Bot.Commands.Vk.Services;

var message = new VkMessageBuilder().AppendPlain("Choose an action").GetMessages()[0];
var text = message.ToVkText();
var keyboard = message.ToInlineKeyboard();
// Pass text and keyboard to VkNet's MessagesSendParams.Message / Keyboard.
```

`VkMessageBuilder` produces plain text; `ToVkText()` rejects a non-null foreign
format instead of silently treating Telegram/Max markup as VK formatting.
The builder preserves common photo/file fields, but the host uploads attachments
and passes the returned VK attachment references to the SDK.

`VkInlineMarkupQueryBuilder` provides the same typed command/session button
overloads as Max, using `MessageEvent` as the callback query type.
`InlineKeyboardButtonsRow` preserves explicit rows; array `AddInlineKeyboardButtons`
overloads add one button per row. `GetResult()` returns a VkNet `MessageKeyboard`
with `Inline = true` and `OneTime = false`.
Common Core keyboards also have a VK `GetResult()` extension.

| Common button mode | VK action |
| --- | --- |
| `CallbackData` | `callback` |
| `Url` | `open_link` |
| `Inline`, `InlineCurrent`, `Payment` | Throws `NotSupportedException` |

Callback data is encoded as a JSON object:

```json
{"bot_commands":"/accept order-42"}
```

`VkCallbackPayload.Encode` creates this envelope; contexts decode it before Core
parses commands or Fluent callbacks. It preserves quotes, Unicode, and Fluent's
encoded fields. A command descriptor prefixes the inner string with
`/{command.Name} `, exactly once when generated by the builder.
For native text-button messages, only this envelope overrides `Message.Text`.
Other callback payloads remain available unchanged to application matchers.
Validate VK keyboard/payload limits in the host, including the JSON envelope's
size; conversions do not guarantee arbitrary common keyboards fit VK limits.

Run the offline text/session/Fluent example:

```sh
dotnet run --project docs/examples/QuickStart -- vk
```

See [VK's message schema](https://github.com/VKCOM/vk-api-schema/blob/master/messages/objects.json)
for native keyboard/message fields. Tests cover synthetic SDK updates and wire
payload round trips; a live community integration must be verified separately.

## Instagram

The `Bot.Commands.Instagram` adapter references the independent
`Instagram.Messaging` API project. Register with `UseInstagramCommandsServices()`
and dispatch individual `InstagramMessagingEvent` objects. Messages and postbacks
retain their native types; both DM session IDs map to the sender. No shared
session contracts change. The common keyboard builder converts to at most three
native template buttons. See [Instagram](instagram.md) for setup, account
subscriptions, ID limits, attachments and sample menu behavior.

## WhatsApp

Register UseWhatsAppCommandsServices() and dispatch individual native
WhatsApp.Messaging.Models.WhatsAppMessage objects. Text/images use message contexts;
interactive button/list and template quick replies use native WhatsAppReply
contexts. The independent WhatsApp.Messaging project owns Cloud API requests and
webhook models. See [WhatsApp](whatsapp.md) for capabilities, strict button limits,
sample adaptation and account setup.
