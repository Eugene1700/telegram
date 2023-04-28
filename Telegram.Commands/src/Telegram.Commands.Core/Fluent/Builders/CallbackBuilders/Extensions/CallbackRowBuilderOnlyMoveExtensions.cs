using System;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions
{
    public static class CallbackRowBuilderOnlyMoveExtensions {
    
        public static ICallbackRowBuilder<TObj, TStates, TCallbacks> NextFromCallback<TObj, TStates, TCallbacks>(this ICallbackRowBuilder<TObj, TStates, TCallbacks> builder, 
            TCallbacks callbackId, 
            CallbackData callback, 
            TStates nextStateId,
            bool force)
        {
            return builder.NextFromCallback(callbackId, _ => callback, nextStateId, force);
        }
    
        public static ICallbackRowBuilderBase<TObj, TStates, TCallbacks> NextFromCallback<TObj, TStates, TCallbacks>(this ICallbackRowBuilderBase<TObj, TStates, TCallbacks> builder, 
            TCallbacks callbackId, 
            CallbackData callback, 
            TStates nextStateId,
            bool force)
        {
            return builder.NextFromCallback(callbackId, _ => callback, nextStateId, force);
        }

        public static ICallbackRowBuilder<TObj, TStates, TCallbacks> NextFromCallback<TObj, TStates, TCallbacks>(this ICallbackRowBuilder<TObj, TStates, TCallbacks> builder, 
            TCallbacks callbackId, 
            string text, 
            string data, 
            TStates nextStateId,
            bool force)
        {
            return builder.NextFromCallback(callbackId, new CallbackData
            {
                Text = text,
                CallbackText = data
            }, nextStateId, force);
        }
    
        public static ICallbackRowBuilderBase<TObj, TStates, TCallbacks> NextFromCallback<TObj, TStates, TCallbacks>(this ICallbackRowBuilderBase<TObj, TStates, TCallbacks> builder, 
            TCallbacks callbackId, 
            string text, 
            string data, 
            TStates nextStateId, bool force)
        {
            return builder.NextFromCallback(callbackId, new CallbackData
            {
                Text = text,
                CallbackText = data
            }, nextStateId, force);
        }
    }
}