using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions
{
    public static class CallbackRowBuilderStronglyTypedExtensions {
        public static ICallbackRowBuilder<TObj, TStates, TCallbacks> OnCallback<TObj, TStates, TCallbacks>(
            this ICallbackRowBuilder<TObj, TStates, TCallbacks> builder,
            TCallbacks callbackId,
            string text,
            string data,
            Func<CallbackQuery, TObj, string, Task<TStates>> handler, bool force)
        {
            return builder.OnCallback(callbackId, new CallbackDataWithCommand
            {
                Text = text,
                CallbackText = data
            }, handler, force);
        }
    
        public static ICallbackRowBuilderBase<TObj, TStates, TCallbacks> OnCallback<TObj, TStates, TCallbacks>(this ICallbackRowBuilderBase<TObj, TStates, TCallbacks> builder, 
            TCallbacks callbackId,
            string text, 
            string data, 
            Func<CallbackQuery, TObj, string, Task<TStates>> handler,
            bool force)
        {
            return builder.OnCallback(callbackId, new CallbackDataWithCommand
            {
                Text = text,
                CallbackText = data
            }, handler, force);
        }
    }
}