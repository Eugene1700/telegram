Helpers for telegram api dotnet client, which has been implemeted in https://github.com/TelegramBots/Telegram.Bot.

## How to use it
Telegram.Commands.Core.TelegramCommandService for automatic resolving telegram bot commands for the format: /<command> <something args> from update message.
  
1. For dotnetcore dependency injection call Telegram.Commands.DependencyInjection.TelegramDependencyExtensions.AddCommands from startup file.
2. Implement Telegram.Commands.Abstract.ITelegramCommandFactory
3. Implement Telegram.Commands.Abstract.IAuthProvider
4. Implement Telegram.Commands.Abstract.ITelegramBotProfile for Webhook.
5. Release needed commands for bot in your project
6. From Update methods call Telegram.Commands.Core.TelegramCommandService.Handle for automatic handling commands.

## Example for implementing ITelegramCommandFactory

```
public class TelegramCommandFactory : ITelegramCommandFactory
{
    private readonly IServiceProvider _serviceProvider;

    public TelegramCommandFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<ITelegramCommand<T>> GetCommand<T>(T message, Type commandType)
    {
        return await Task.FromResult((ITelegramCommand<T>)_serviceProvider.GetService(commandType));
    }
}
```

## Make command
Use CommandAttribute for your class of command and you have to implement Telegram.Commands.Abstract.Interfaces.ITelegramCommand.

# Other helpers

## InlineMarkupQueryBuilder

## TelegramCommandExtensions

