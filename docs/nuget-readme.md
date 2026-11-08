# Bot.Commands

Commands, sessions, and fluent conversation flows for Telegram and Max bots on .NET 10.

| Package | Purpose |
| --- | --- |
| Bot.Commands.Abstract | Messenger-independent contracts |
| Bot.Commands.Core | Command execution, sessions and Fluent state machines |
| Bot.Commands.DependencyInjection | Microsoft DI registration |
| Bot.Commands.UI | Dropdown and pagination helpers |
| Bot.Commands.Telegram | Telegram.Bot adapter |
| Bot.Commands.Max | MaxMessenger.Bot adapter |

Install the adapter for your messenger; it brings in the shared engine and DI
dependencies. Add Bot.Commands.UI separately if you need its helpers. Use the
same release version for all Bot.Commands packages. Preview releases require
enabling prerelease packages in your NuGet client.

Your application owns update delivery, SDK clients, outgoing messages, session
storage and hosting. Max integration depends on a prerelease MaxMessenger.Bot SDK.

- [Getting started](https://github.com/Eugene1700/telegram/blob/main/docs/getting-started.md)
- [Runnable quick start](https://github.com/Eugene1700/telegram/tree/main/docs/examples/QuickStart)
- [Fluent conversations](https://github.com/Eugene1700/telegram/blob/main/docs/fluent.md)
- [Migration from the Telegram-prefixed API](https://github.com/Eugene1700/telegram/blob/main/docs/migration.md)

Licensed under MIT.
