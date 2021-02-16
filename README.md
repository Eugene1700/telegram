Helpers for telegram api dotnet client, which has been implemeted in https://github.com/TelegramBots/Telegram.Bot.

## How to use it
Telegram.Commands.Core.TelegramCommandService for automatic resolving telegram bot commands for the format: ```/<command> <args>``` from update message or CallbackQuery or PreCheckoutQuery. And you can make chains of commands as state machine.
  
1. For dotnetcore dependency injection call Telegram.Commands.DependencyInjection.TelegramDependencyExtensions.AddCommands from startup file.
2. Implement Telegram.Commands.Abstract.IAuthProvider
3. Implement Telegram.Commands.Abstract.ITelegramBotProfile for Webhook.
5. Implement Telegram.Commands.Abstract.ISessionStore for chains of commands
5. Release needed commands for bot in your project
6. From Update methods call Telegram.Commands.Core.TelegramCommandService.Handle for automatic handling commands.

## Make command
Use CommandAttribute for your class of command and you have to implement Telegram.Commands.Abstract.Interfaces.ITelegramCommand.

# Other helpers

## InlineMarkupQueryBuilder

## TelegramCommandExtensions

