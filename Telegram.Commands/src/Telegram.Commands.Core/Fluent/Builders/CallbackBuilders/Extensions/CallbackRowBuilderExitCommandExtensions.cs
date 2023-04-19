using System;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;

public static class CallbackRowBuilderExitCommandExtensions
{
    public static  ICallbackRowBuilder<TObj> ExitFromCallback<TObj, TCommand>(this ICallbackRowBuilder<TObj> builder, 
        Func<TObj, CallbackData> callbackProvider)
        where TCommand : IQueryTelegramCommand<CallbackQuery>
    {
        return builder.ExitFromCallback(o =>
        {
            var callbackData = callbackProvider(o);
            return new CallbackData
            {
                Text = callbackData.Text,
                CallbackMode = callbackData.CallbackMode,
                CallbackText = callbackData.CallbackText,
            };
        }, TelegramCommandExtensions.GetCommandInfo<TCommand, CallbackQuery>());
    }

    public static ICallbackRowBuilder<TObj> ExitFromCallback<TObj, TCommand>(this ICallbackRowBuilder<TObj> builder, string text, string data)
        where TCommand : IQueryTelegramCommand<CallbackQuery>
    {
        return builder.ExitFromCallback<TObj, TCommand>(_ => new CallbackData
        {
            Text = text,
            CallbackText = data
        });
    }

}