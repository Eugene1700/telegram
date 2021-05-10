Helpers for telegram api dotnet client, which has been implemeted in https://github.com/TelegramBots/Telegram.Bot.

# How to use it
Telegram.Commands.Core.TelegramCommandService for automatic resolving telegram bot commands for the format: ```/<command> <args>``` from update message or CallbackQuery or PreCheckoutQuery. And you can make chains of commands as state machine.
  
1. For dotnetcore dependency injection call Telegram.Commands.DependencyInjection.TelegramDependencyExtensions.UseTelegramCommandsServices from startup file.
2. Implement Telegram.Commands.Abstract.IAuthProvider
3. Implement Telegram.Commands.Abstract.ITelegramBotProfile for Webhook.
5. Implement Telegram.Commands.Abstract.ISessionStore for chains of commands
5. Release needed commands for bot in your project
6. From Update methods call Telegram.Commands.Core.TelegramCommandService.Handle for automatic handling commands.

# Make command
Use CommandAttribute for your class of command and you have to implement Telegram.Commands.Abstract.Interfaces.ITelegramCommand and add CommandAttribute.

```
public interface ITelegramCommand<in TQuery>
{
    Task<ITelegramCommandExecutionResult> Execute(TQuery query);
}
```

## Example
```
[Command(Name = "my", Permission = Permissions.User)]
public class MyCommand : ITelegramCommand<Message>
{
    private readonly ITelegramBotClient _telegramClient;

    public MyCommand(ITelegramBotClient telegramClient)
    {
        _telegramClient = telegramClient;
    }
    public async Task<ITelegramCommandExecutionResult> Execute(Message query)
    {
        var chatId = query.GetChatId();

        if (query.Text.Contains("ahead"))
        {
            await _telegramClient.SendTextMessageAsync(chatId,
                $"Your message");
            //Next command of chain, you can store data between commands
            return TelegramCommandExecutionResult.Ahead<NextCommand, Message, long>(1);
        }

        if (query.Text.Contains("break"))
        {
            await _telegramClient.SendTextMessageAsync(chatId,
                $"Your message");
            //interrupt execution of chain
            return TelegramCommandExecutionResult.Break();
        }

        if (query.Text.Contains("freeze"))
        {
            await _telegramClient.SendTextMessageAsync(chatId,
                $"Your message");
            //freeze state of chains and you go this command again
            return TelegramCommandExecutionResult.Freeze();
        }
        throw new TelegramDomainException("You have to input an action");
    }
}
    

```
## Permissions

## Chat type contraction

## Reactions

## Exceptions

# Other helpers

## InlineMarkupQueryBuilder

## TelegramCommandExtensions

