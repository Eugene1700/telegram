# SimpleHandlers: Telegram, Max, VK, Instagram, and WhatsApp

`Telegram.Commands/src/Telegram.Commands.Samples/SimpleHandlers` is an ASP.NET Core
webhook application that can serve all five bots at the same time.

## Configuration

Use [appsettings.Example.json](../Telegram.Commands/src/Telegram.Commands.Samples/SimpleHandlers/appsettings.Example.json)
as a reference. It is not loaded automatically. Provide real credentials through
environment variables or a local secret store. The loaded configuration includes
disabled `VkBotProfile` and `InstagramBotProfile` sections; existing Telegram and Max settings remain independent.

| Setting | Purpose |
| --- | --- |
| `BotProfile:Enabled` | Telegram endpoint and startup registration; defaults to true |
| `MaxBotProfile:Enabled` | Max endpoint and startup registration; defaults to false |
| `VkBotProfile:Enabled` | VK Callback API endpoint; defaults to false |
| `Token` in each profile | Corresponding bot/community token |
| `BotName` in each profile | Unique session partition; Telegram also uses it for mentions |
| `BaseUrl` in each profile | Public HTTPS base URL |
| Telegram/Max `RegisterWebhook` | Register at startup; defaults to true |
| `MaxBotProfile:WebhookSecret` | Secret checked in the Max request header |
| `VkBotProfile:GroupId` | Positive numeric VK community ID |
| `VkBotProfile:WebhookSecret` | Secret checked in the JSON request body |
| `VkBotProfile:ConfirmationCode` | Confirmation response supplied by VK |
| `ConnectionStrings:Sessions` | EntityStorage SQLite connection; default `Data Source=dev2.db` |

Environment variables use double underscores: `VkBotProfile__Token`,
`VkBotProfile__Enabled=true`, and so on. Enabled profiles must have distinct,
nonempty `BotName` values (checked case-insensitively). An enabled VK profile
also requires a token, positive group ID, secret, and confirmation code.

Routes:

- Telegram: `POST /api/message/update`
- Max: `POST /api/max/message/update`
- VK: `POST /api/vk/message/update`
- Instagram: `GET` (verification) and `POST /api/instagram/message/update`
- WhatsApp: `GET` (verification) and `POST /api/whatsapp/message/update`
- Health: `GET /ping`

Run from the sample directory so relative database paths stay consistent:

```powershell
cd Telegram.Commands/src/Telegram.Commands.Samples/SimpleHandlers
dotnet run
```

**Existing sample startup behavior:** `Startup.Configure` calls
`EnsureDeleted()` and `EnsureCreated()` on the configured database. It starts
empty and discards previous sessions on each application restart. Do not point
this sample at a database whose contents you need to preserve. The VK integration
does not change this behavior or introduce a migration.

## VK Callback API setup

Create a Callback API server in your community settings with the public URL
ending in `/api/vk/message/update`. Copy its confirmation code into
`VkBotProfile:ConfirmationCode`, set the same secret in VK and the profile,
and enable `message_new` and `message_event`.

VK registration is managed in the community settings; there is no startup
registration service or `RegisterWebhook` switch for VK. `BaseUrl` is profile
metadata, not an automatic subscription request.

`WebhookAccessFilter` rejects disabled routes with HTTP 404. For VK it validates
the secret and community ID before action services are resolved, and rewinds the
body for model binding. Invalid credentials return HTTP 401; malformed JSON
returns HTTP 400. Confirmation requests return the configured plain-text code.
Handled updates return plain-text `ok`. Callback events are acknowledged through
`messages.sendMessageEventAnswer`. Processing failures return HTTP 500.

The controller accepts JSON with ASP.NET's normal formatter and uses VkNet's
Newtonsoft.Json converter for `GroupUpdate`, without changing Telegram/Max
serialization globally. The sample does not implement deduplication or a queue;
production hosts should supply ordered, idempotent processing.

Telegram and Max retain their existing registration services. Use
`RegisterWebhook=false` when managing those subscriptions externally.

## Commands and senders

Commands are grouped by scenario under `Services/Commands`: `Start`, `Ask`,
`Profile`, `Photos`, `DropDown`, `MyFluent`, `GlobalIntercept`, and `Cancel`.
Shared bases contain the conversation logic. Concrete Telegram/Max/VK/Instagram/WhatsApp descendants
declare native interfaces and their platform swarm. Session data and state types
stay with their scenario.

All five platforms support:

- `/start`, `/ask`, and `/profile`, including the profile callback.
- The `register` message matcher as an entry to the profile flow.
- Photo collection, dropdown/multiple selection, and cancellation.
- `/myfluent`, `/otherfluent`, and `/global` demonstrations.

`BotRuntime` selects the profile from the request route. DI selects
`TelegramSampleMessageSender`, `MaxSampleMessageSender`, or
`VkSampleMessageSender` or `InstagramSampleMessageSender`; commands depend on `ISampleMessageSender`.

The VK sender sends/edits plain text and keyboards, uploads and saves photos or
documents, and preserves file intent and filenames. Edits use the callback's
**conversation-local** message ID with its peer ID. An edit without buttons
explicitly removes the old keyboard. The incoming photo sample reports size as
"not provided" when VK supplies no byte count.

Keyboard builders share row/metadata handling in Core. Adapters own native button
conversion and formatting. VK command/Fluent payloads use the adapter's JSON
envelope; see [adapters](adapters.md#vk-integration).

## Profiles and sessions

Each request gets its own DI scope and resolves the selected `IBotProfile`
before its store and commands. All platforms use the same application-owned
`SessionStore`, EntityStorage integration, and `COMMAND_SESSION` table.

The store writes and filters `BOT_NAME`, chat ID, and user ID during creation,
lookup, and updates. Equal user/chat IDs in different bots therefore do not share
a conversation. Swarms select commands; `BotName` isolates persisted sessions.
Shared session contracts and EntityStorage remain unchanged.

## Verification

`SimpleHandlersPlatformTests` runs the same command/session/callback scenarios
through all five adapters with a recording sender. `ThreePlatformSessionTests`
now interleaves five Fluent flows with equal user/chat IDs against a temporary
in-memory SQLite database and checks persisted names, state restoration, and
completion isolation across fresh scopes.

`VkWebhookTests` covers profile selection, duplicate names, request validation,
confirmation, and callback acknowledgement. `VkMessageSenderTests` uses mocked
SDK categories and an HTTP handler for media uploads. These tests do not contact
live messenger APIs or load production bot credentials.

## VK webhook registration

`VkBotProfile:RegisterWebhook` defaults to false. For automatic registration, set
it to true and supply `BaseUrl` (public HTTPS), `GroupId`, `Token`,
`WebhookSecret`, `BotName` and an explicit `CallbackApiVersion` supported by VK.
The token must have permissions for the community callback-management methods.

`ConfigureVkWebhook` runs after the HTTP server starts. It obtains the API
confirmation code before registering the server, then enables `message_new` and
`message_event`. The fetched confirmation code is held in configuration memory;
it is not written to appsettings. The API code can differ from the dashboard code.
`CallbackServerId` selects an existing server explicitly; zero finds the same URL
or creates a server. For URL changes, supply the old server ID to update it rather
than creating another server. Unrelated servers are not deleted. Use one instance
for registration to avoid concurrent creation. Shutdown keeps subscriptions.

For manual registration (`RegisterWebhook=false`), set the callback URL to
`https://YOUR-HOST/api/vk/message/update` in the VK community dashboard, copy its
`ConfirmationCode` into the profile, configure the matching secret and enable
both events. Restart the sample after changing local settings.

## Instagram webhook configuration

The separate `InstagramBotProfile` includes `RegisterWebhook`, `BaseUrl`,
`AccountId`, `ApiVersion`, `Token`, `AppSecret`, and `VerifyToken`.
The callback URL is `https://YOUR-HOST/api/instagram/message/update`.
Configure that URL and verify token in the Meta dashboard; setting
`RegisterWebhook=true` subscribes the account to messages and postbacks after
startup. These are two distinct setup steps. See [Instagram setup](instagram.md)
for all settings, permissions and supported behavior.

Instagram has concrete command descendants for the same shared scenarios and
uses the `instagram` swarm. The application store isolates all four profiles by
BotName. Instagram menus are sent in groups of at most three buttons; edits
publish a new menu. Image reception is supported without downloading attachments;
outgoing raw file/photo bytes are not uploaded.

## WhatsApp configuration

The disabled WhatsAppBotProfile includes PhoneNumberId, BusinessAccountId, Token,
ApiVersion, BaseUrl, AppSecret, VerifyToken and RegisterWebhook. Configure the
callback in Meta's dashboard; RegisterWebhook subscribes the app to the WABA after
HTTP startup. See [WhatsApp](whatsapp.md) for the full setup and message limits.
The whatsapp swarm uses the same shared commands and a distinct BotName. The
sample sender splits menus, shortens long display titles and publishes a new
message for edits. Callback payloads are preserved.
