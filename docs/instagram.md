# Instagram messaging

The independent `Instagram.Messaging` project contains a small HTTP client and native
webhook models for Instagram API with Instagram Login. It has no Bot.Commands
dependencies and can be used on its own. It is a repository-owned client for the
official API, not an official Meta .NET SDK. `Bot.Commands.Instagram` references it
and translates native messaging events into typed command contexts.

## Account and application setup

Use an Instagram professional account (Business or Creator), an Instagram user
access token with messaging permissions (`instagram_business_manage_messages`),
and an explicitly selected supported Graph API version. OAuth, token renewal, app
review and account onboarding are outside this client. Conversations must be
initiated by the Instagram user. See Meta's
[Instagram API collection](https://www.postman.com/meta/instagram/documentation/6yqw8pt/instagram-api)
and [webhook setup](https://developers.facebook.com/docs/instagram-platform/webhooks).

SimpleHandlers has a disabled `InstagramBotProfile`. Populate these settings using
environment variables or a secret store:

| Setting | Purpose |
| --- | --- |
| `Enabled` | Enable the Instagram adapter and endpoint |
| `BotName` | Unique application session partition, e.g. `instagram_sample` |
| `Token` | Instagram user access token |
| `AccountId` | Professional Instagram account ID matching webhook entry/recipient IDs |
| `ApiVersion` | Explicit supported version in `vNN.N` format; no implicit latest version |
| `BaseUrl` | Public HTTPS base URL |
| `AppSecret` | App secret used to validate `X-Hub-Signature-256` |
| `VerifyToken` | Your verification string shared with the Meta dashboard |
| `RegisterWebhook` | Opt in to account-level event subscription after HTTP startup; default false |

In the Meta dashboard, configure callback URL
`https://YOUR-HOST/api/instagram/message/update`, the same `VerifyToken`, and
the `messages` and `messaging_postbacks` fields. The app's GET endpoint answers
the verification challenge. `ConfigureInstagramWebhook` calls
`IInstagramClient.SubscribeToMessagingAsync` (`POST /me/subscribed_apps`) when
`RegisterWebhook=true`; this account subscription does not replace dashboard
callback configuration. Subscriptions are retained on shutdown. Startup failures
are surfaced; there is no silent retry or token refresh.

POST requests validate the raw-body HMAC before JSON binding, filter the configured
account, and process each event in its own DI scope. Disabled endpoints return 404.
The sample does not implement a durable webhook queue, deduplication or distributed
session locking; production hosts must address duplicate delivery and concurrency.

## Adapter and SDK usage

Register the adapter with `UseInstagramCommandsServices()` and resolve
`IBotCommandResolver<InstagramMessagingEvent>` for one event. Hosts must validate
webhook authenticity/account ownership and split webhook batches themselves, as
the sample controller does.

- Messages and incoming image attachments use `IQueryContext<InstagramMessage>`.
- Button postbacks use `IQueryContext<InstagramPostback>`.
- Quick reply payloads route through the message context.
- Echoes, deletion notifications, receipts and unsupported events are ignored.
- Both chat and user session IDs are the sender's Instagram-scoped ID. The adapter
  requires canonical positive Int64 IDs and rejects overflow; it never hashes or
  truncates them. SDK models and API methods keep string IDs without this limit.
- Session contracts are unchanged. SimpleHandlers partitions storage by
  `BotName + ChatId + UserId`, including across fresh webhook scopes.

The SDK exposes `SendTextAsync`, `SendImageAsync` (public HTTPS image URL) and
`SubscribeToMessagingAsync`. It accepts an injected `HttpClient` and
`InstagramOptions`; dependency injection or ASP.NET are not required.

`InstagramInlineMarkupQueryBuilder` uses the common builder and preserves
command payloads. It flattens rows into a button template, allows postback and URL
buttons, and rejects more than three buttons. The SimpleHandlers sender splits
larger menus into messages with at most three buttons each. Updating a menu sends
a new message; older menus remain visible. Plain text is supported; foreign text
formats, payments and raw file/photo uploads are rejected. For outgoing images,
publish an HTTPS URL and use the SDK directly. Incoming images are not downloaded
and do not expose a reliable file size.

Messaging eligibility and the platform's reply window are independent of framework
session expiry. API failures are surfaced with HTTP status; responses containing
secrets are not copied into exceptions. Tests use fake HTTP handlers and synthetic
updates; live account delivery has not been tested.

Try the offline example:

```sh
dotnet run --project docs/examples/QuickStart -- instagram
```
