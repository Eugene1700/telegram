using System;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions
{
    public static class CallbackRowBuilderOnlyMoveExtensions {
    
        public static ICallbackRowBuilder<TObj, TStates> NextFromCallback<TObj, TStates>(this ICallbackRowBuilder<TObj, TStates> builder, 
            CallbackData callback, 
            TStates nextStateId,
            bool forceRerender = true)
        {
            return builder.NextFromCallback((s,o) => callback, nextStateId, forceRerender);
        }
    
        public static ICallbackRowBuilderBase<TObj, TStates> NextFromCallback<TObj, TStates>(this ICallbackRowBuilderBase<TObj, TStates> builder, 
            CallbackData callback, 
            TStates nextStateId,
            bool forceRerender = true)
        {
            return builder.NextFromCallback((s,o) => callback, nextStateId, forceRerender);
        }

        public static ICallbackRowBuilder<TObj, TStates> NextFromCallback<TObj, TStates>(this ICallbackRowBuilder<TObj, TStates> builder, 
            string text, 
            string data, 
            TStates nextStateId,
            bool forceRerender = true)
        {
            return builder.NextFromCallback(new CallbackData
            {
                Text = text,
                CallbackText = data
            }, nextStateId, forceRerender);
        }
    
        public static ICallbackRowBuilderBase<TObj, TStates> NextFromCallback<TObj, TStates>(this ICallbackRowBuilderBase<TObj, TStates> builder, 
            string text, 
            string data, 
            TStates nextStateId, bool forceRerender = true)
        {
            return builder.NextFromCallback(new CallbackData
            {
                Text = text,
                CallbackText = data
            }, nextStateId, forceRerender);
        }
    }
}