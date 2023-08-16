using System;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions
{
    public static class CallbackRowBuilderExitCommandExtensions
    {
        public static  ICallbackRowBuilder<TObj, TStates> ExitFromCallback<TObj, TStates, TCommand>(this ICallbackRowBuilder<TObj, TStates> builder, 
            Func<TStates, TObj, CallbackData> callbackProvider)
            where TCommand : IQueryTelegramCommand<CallbackQuery>
        {
            return builder.ExitFromCallback((s, o) =>
            {
                var callbackData = callbackProvider(s, o);
                return new CallbackData
                {
                    Text = callbackData.Text,
                    CallbackMode = callbackData.CallbackMode,
                    CallbackText = callbackData.CallbackText,
                };
            }, TelegramCommandExtensions.GetCommandInfo<TCommand, CallbackQuery>());
        }

        public static ICallbackRowBuilder<TObj, TStates> ExitFromCallback<TObj, TStates, TCommand>(this ICallbackRowBuilder<TObj, TStates> builder, 
            string text, string data)
            where TCommand : IQueryTelegramCommand<CallbackQuery>
        {
            return builder.ExitFromCallback<TObj, TStates, TCommand>((s,o) => new CallbackData
            {
                Text = text,
                CallbackText = data
            });
        }

    }
}