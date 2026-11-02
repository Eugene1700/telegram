using global::Telegram.Bot.Types;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders;

namespace Bot.Commands.Telegram.Extensions
{
    public static class CallbackRowBuilderExitCommandExtensions
    {
        public static  ICallbackRowBuilder<TObj, TStates> ExitFromCallback<TObj, TStates, TCommand>(this ICallbackRowBuilder<TObj, TStates> builder, 
            Func<TStates, TObj, CallbackData> callbackProvider)
            where TCommand : IQueryCommand<CallbackQuery>
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
            }, BotCommandExtensions.GetCommandInfo<TCommand, CallbackQuery>());
        }

        public static ICallbackRowBuilder<TObj, TStates> ExitFromCallback<TObj, TStates, TCommand>(this ICallbackRowBuilder<TObj, TStates> builder, 
            string text, string data)
            where TCommand : IQueryCommand<CallbackQuery>
        {
            return builder.ExitFromCallback<TObj, TStates, TCommand>((s,o) => new CallbackData
            {
                Text = text,
                CallbackText = data
            });
        }

    }
}