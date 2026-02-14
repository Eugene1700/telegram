using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Core;

namespace Telegram.Commands.Realization.Extensions;

public static class TelegramCommandExtensions
{
    public static string GetCommandQuery<TCommand>() where TCommand : IQueryTelegramCommand<Message>
    {
        return BotCommandExtensions.GetCommandInfo<TCommand, Message>().GetCommandQuery();
    }
}