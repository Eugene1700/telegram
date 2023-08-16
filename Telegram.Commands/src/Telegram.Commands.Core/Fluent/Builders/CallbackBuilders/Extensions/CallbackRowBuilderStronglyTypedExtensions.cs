using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions
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