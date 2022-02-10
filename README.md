Helpers for telegram api dotnet client, which has been implemeted in https://github.com/TelegramBots/Telegram.Bot.

# How to use it
Telegram.Commands.Core.TelegramCommandService for automatic resolving telegram bot commands for the format: ```/<command> <args>``` from update message or CallbackQuery or PreCheckoutQuery and etc. And you can make chains of commands as state machine.
  
1. For dotnetcore dependency injection just call Telegram.Commands.DependencyInjection.TelegramDependencyExtensions.UseTelegramCommandsServices from startup file.
2. Implement Telegram.Commands.Abstract.IAuthProvider
3. Implement Telegram.Commands.Abstract.ITelegramBotProfile for each bot.
5. Implement Telegram.Commands.Abstract.ISessionStore for storing service info about command handling flow.
5. Release needed commands for bot in your project
6. From Update methods call Telegram.Commands.Core.TelegramCommandService.Handle for automatic handling commands.

# Make command

There are three kinds of commands:
1. IQueryTelegramCommand<TQuery> - it is a simple handler. We can get info about handler directly from query (for example Text field for Message).
2. ISessionTelegramCommand<TQuery, TSessionObject> - it is a handler for chaining command, where we can use strong type of query. For example, photo uploading after some /command.
3. IBehaviorCommand<TSessionObject> - it is a handler with interceptors for IQueryTelegramCommand<TQuery> and ISessionTelegramCommand<TQuery, TSessionObject>. Use this type, when you need a chain with other actions. For example, photo uploading after some /command with cancel action (CallbackQuery).
More information in [Sample](https://github.com/Eugene1700/telegram/tree/master/Telegram.Commands/src/Telegram.Commands.Samples/SimpleHandlers) project.
*TQuery* - query from Telegram.Bots (Message, Callback and etc.) 
*TSessionObject* is object, which you can exchage between the commands of chain.
  
```
public interface IQueryTelegramCommand<in TQuery>
{
    Task<ITelegramCommandExecutionResult> Execute(TQuery query);
}
  
public interface ISessionTelegramCommand<in TQuery, TSessionObject>
{
    Task<ITelegramCommandExecutionResult> Execute(TQuery query, TSessionObject sessionObject);
}
  
public interface IBehaviorCommand<TSessionObject>
{
    Task<ITelegramCommandExecutionResult> Execute<TQuery>(IQueryTelegramCommand<TQuery> queryCommand, TQuery query, TSessionObject sessionObject);
    Task<ITelegramCommandExecutionResult> Execute<TQuery>(ISessionTelegramCommand<TQuery> sessionCommand, TQuery query, TSessionObject sessionObject);
    Task<ITelegramCommandExecutionResult> DefaultExecute<TQuery>(TQuery query, TSessionObject sessionObject);
}
```

## Example
```
[Sample](https://github.com/Eugene1700/telegram/blob/master/Telegram.Commands/src/Telegram.Commands.Samples/SimpleHandlers/Services/Commands/SendPhotoCommand.cs) 
[Command(Name = "my", ChatArea = ChatArea.Private)]
public class MyCommand : IQueryTelegramCommand<Message>
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
 
  [Sample](https://github.com/Eugene1700/telegram/blob/master/Telegram.Commands/src/Telegram.Commands.Samples/SimpleHandlers/Services/Commands/GetPhotoSizeSession.cs)
[Command(Name = "nextcommand", ChatArea = ChatArea.Private)]
public class NextCommand : ISessionTelegramCommand<Message, long>
{
    private readonly ITelegramBotClient _telegramClient;

    public MyCommand(ITelegramBotClient telegramClient)
    {
        _telegramClient = telegramClient;
    }
  
    public async Task<ITelegramCommandExecutionResult> Execute(Message query, long sessionObject)
    {
        var chatId = query.GetChatId();
  
        await _telegramClient.SendTextMessageAsync(chatId,
                $"SessionObject={sessionObject} Message={query.Text}");
       return TelegramCommandExecutionResult.Break();
    }
}
    

```
## Behaviors command

See [sample](https://github.com/Eugene1700/telegram/blob/master/Telegram.Commands/src/Telegram.Commands.Samples/SimpleHandlers/Services/Commands/GetPhotoMenuBehavior.cs)
  
## Forwarding
 
You can exit from command hadler with ways:
  TelegramCommandExecutionResult.Ahead - to create or continue chain.
  TelegramCommandExecutionResult.Break  - to interrupt chain or simple exit.
  TelegramCommandExecutionResult.Freeze - to freeze state of current chain. 
  
## Chat type contraction

You can use command only for private, group, channel chats. Set the flag in CommandAttribute.
  
  ```
  [Flags]
  public enum ChatArea
  {
    Private = 1,
    Group = 2,
    Channel = 4,
    SuperGroup = 8,
  }
  ```
  
## Swarms
  
  You can unite some commands in swarms and distribute them between different bots. Set the Swarms in CommandAttribute.
  
## Authorization
  
  You can set the Authorized flag for enable authorization of command with IAuthProvider.

## Exceptions

# Other helpers

## InlineMarkupQueryBuilder
  
For easy creating keyaboards.
  
  ```
  public InlineMarkupQueryBuilder AddInlineKeyboardButtons<TCommand>(CallbackData[] callbackQueries)
            where TCommand : IQueryTelegramCommand<CallbackQuery>;
  public InlineMarkupQueryBuilder AddInlineKeyboardButtons<TCommand, TSessionObject>(CallbackData[] callbackQueries)
            where TCommand : ISessionTelegramCommand<CallbackQuery, TSessionObject>;
  public InlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand>(CallbackData callbackData)
            where TCommand : IQueryTelegramCommand<CallbackQuery>;
  public InlineMarkupQueryBuilder AddInlineKeyboardButton<TCommand, TSessionObject>(CallbackData callbackData)
            where TCommand : ISessionTelegramCommand<CallbackQuery, TSessionObject>;
  public InlineMarkupQueryBuilder AddInlineKeyboardButton(CallbackData[] callbackQueries);
  public InlineMarkupQueryBuilder AddInlineKeyboardButton(CallbackData callbackData);
  public InlineMarkupQueryBuilder InlineKeyboardButtonsRow(CallbackDataWithCommand[] callbackData);
  
  public IReplyMarkup GetResult();
  ```

## TelegramCommandExtensions

