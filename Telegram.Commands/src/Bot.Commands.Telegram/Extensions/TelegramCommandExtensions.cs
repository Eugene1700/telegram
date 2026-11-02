using global::Telegram.Bot.Types;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Core;

namespace Bot.Commands.Telegram.Extensions;

public static class TelegramCommandExtensions
{
    public static string GetCommandQuery<TCommand>() where TCommand : IQueryCommand<Message>
    {
        return BotCommandExtensions.GetCommandInfo<TCommand, Message>().GetCommandQuery();
    }
}