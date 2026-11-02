using global::Telegram.Bot.Types;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Fluent.Builders;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders;
using Bot.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;
using Bot.Commands.Core.Services;

namespace Bot.Commands.Telegram.Extensions
{
    public static class CallbackRowBuilderStronglyTypedExtensions
    {
        public static ICallbackRowBuilder<TObj, TStates> OnCallback<TObj, TStates>(
            this ICallbackRowBuilder<TObj, TStates> builder,
            string text,
            string data,
            Func<CallbackQuery, TStates, TObj, string, Task<TStates>> handler,
            bool force)
        {
            return builder.OnCallback(new CallbackDataWithCommand
            {
                Text = text,
                CallbackText = data
            }, handler, force);
        }

        public static ICallbackRowBuilderBase<TObj, TStates> OnCallback<TObj, TStates>(
            this ICallbackRowBuilderBase<TObj, TStates> builder,
            string text,
            string data,
            Func<CallbackQuery, TStates, TObj, string, Task<TStates>> handler,
            bool force)
        {
            return builder.OnCallback(new CallbackDataWithCommand
            {
                Text = text,
                CallbackText = data
            }, handler, force);
        }
    }
}