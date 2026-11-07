# WhatsApp Cloud API

`WhatsApp.Messaging` is a separate .NET 10 project with no Bot.Commands dependencies.
It implements the official Cloud API over an injected HttpClient. It is a
repository-owned client, not an official Meta .NET SDK.
`Bot.Commands.WhatsApp` provides typed contexts, command routing and keyboard
conversion. Both projects are included in the solution under SDKs and Adapters.

## SimpleHandlers configuration

Use the disabled `WhatsAppBotProfile` in
[appsettings.Example.json](../Telegram.Commands/src/Telegram.Commands.Samples/SimpleHandlers/appsettings.Example.json).
Supply credentials through environment variables or your secret store.

| Setting | Meaning |
| --- | --- |
| Enabled | Enable WhatsApp hosting; default false |
| BotName | Unique session partition, e.g. whatsapp_sample |
| Token | Access token authorized for the WABA and phone number |
| PhoneNumberId | Cloud API business phone number ID, not its display phone number |
| BusinessAccountId | WhatsApp Business Account ID (WABA) |
| ApiVersion | Explicit supported Graph API version in vNN.N format |
| BaseUrl | Public HTTPS application base URL |
| AppSecret | Meta app secret for validating signed webhook bodies |
| VerifyToken | Your verification string shared with the app dashboard |
| RegisterWebhook | Subscribe the app to the WABA after HTTP startup; default false |

Environment variable names use double underscores, e.g.
`WhatsAppBotProfile__Token`.

Configure the WhatsApp webhook in the Meta app dashboard with callback URL
`https://YOUR-HOST/api/whatsapp/message/update`, the same VerifyToken, and the
`messages` field. GET answers the verification challenge. POST verifies
`X-Hub-Signature-256` against the raw body, then checks the WABA and phone number
IDs before dispatching messages. Status notifications are acknowledged without
executing commands. Disabled endpoints return 404.

With RegisterWebhook=true, ConfigureWhatsAppWebhook calls
`POST /{BusinessAccountId}/subscribed_apps` after the HTTP server starts.
This account subscription does not configure the dashboard callback URL.
Subscriptions are preserved on shutdown. Failures are surfaced rather than
silently retried. Tokens need the appropriate messaging permissions and
business-management permissions for WABA subscription.

See Meta's [Cloud API collection](https://www.postman.com/meta/whatsapp-business-platform/collection/wlk6lh4/whatsapp-cloud-api)
and [WABA subscription request](https://www.postman.com/meta/whatsapp-business-platform/request/26gui66/subscribe-to-a-waba).
Account onboarding, phone-number registration, token renewal and app approval are
outside this implementation. Free-form replies depend on WhatsApp's customer
service window; this client does not send approved templates outside that window.
A persisted command session does not grant permission to send a message.

## Supported behavior

Register `UseWhatsAppCommandsServices()` and resolve
`IBotCommandResolver<WhatsAppMessage>` for one native message. The host is
responsible for signature/account validation and splitting webhook batches.
SimpleHandlers gives each message a fresh DI scope.

| Incoming payload | Context |
| --- | --- |
| text | IQueryContext&lt;WhatsAppMessage&gt;, text.body |
| image | IQueryContext&lt;WhatsAppMessage&gt;, native image metadata/caption |
| interactive.button_reply | IQueryContext&lt;WhatsAppReply&gt;, reply ID |
| interactive.list_reply | IQueryContext&lt;WhatsAppReply&gt;, reply ID |
| button (template quick reply) | IQueryContext&lt;WhatsAppReply&gt;, payload |
| statuses, reactions, system, unsupported types | Ignored by command routing |

Reply contexts retain the native reply object and the enclosing message.
GetMessageId returns the referenced outgoing menu ID, when supplied.
Incoming media is not downloaded; the photo sample reports no byte size.

ChatId and UserId both use the sender's canonical numeric WhatsApp ID. Adapter
conversion rejects invalid values and Int64 overflow rather than hashing or
truncating them. API models keep string IDs. The store remains application-owned
and partitions by BotName + ChatId + UserId. No shared session contracts or
EntityStorage implementation changes are required.

## Outgoing API and menus

IWhatsAppClient exposes SendTextAsync, SendImageAsync (public HTTPS URL), and
SubscribeToMessagingAsync. Sending returns the native message ID. Errors retain
HTTP status without copying potentially sensitive response bodies.

The native WhatsAppInlineMarkupQueryBuilder uses the common builder's metadata,
then produces reply buttons. It supports up to three buttons per message, unique
IDs, 20-character titles and 256-character payloads. Unsupported button modes
(including URL/payment buttons) fail explicitly. This client does not yet expose
CTA URL buttons, outgoing list messages, template sending or media uploads.

SimpleHandlers preserves all menu actions by splitting them into messages of at
most three buttons and separating repeated IDs. It shortens display titles longer
than 20 UTF-16 code units with an ellipsis, preserving callback data. It validates
all chunks before sending any. Edit publishes a new menu; older menus remain
visible. Raw file/photo bytes and foreign messenger formats are rejected.
Text uses WhatsApp's native text behavior; no cross-messenger markup conversion
is attempted.

All shared SimpleHandlers scenarios have WhatsApp descendants and the whatsapp
swarm: start, ask, profile, Fluent flows, global interception, photo collection,
dropdowns and cancellation.

## Offline verification

```sh
dotnet run --project docs/examples/QuickStart -- whatsapp
```

Tests cover HTTP payloads, errors, signatures, account filtering, batched scopes,
subscription lifecycle, native replies, menu limits, shared commands and SQLite
session isolation across five profiles. They do not verify delivery to live
accounts. Production hosts must supply duplicate-delivery handling, ordered
processing and distributed session coordination as needed.
