using System;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions
{
    public static class CallbackRowBuilderOnlyMoveExtensions {
    
        public static ICallbackRowBuilder<TObj, TStates> NextFromCallback<TObj, TStates>(this ICallbackRowBuilder<TObj, TStates> builder, 
            CallbackData callback, 
            TStates nextStateId,
            bool force)
        {
            return builder.NextFromCallback((s,o) => callback, nextStateId, force);
        }
    
        public static ICallbackRowBuilderBase<TObj, TStates> NextFromCallback<TObj, TStates>(this ICallbackRowBuilderBase<TObj, TStates> builder, 
            CallbackData callback, 
            TStates nextStateId,
            bool force)
        {
            return builder.NextFromCallback((s,o) => callback, nextStateId, force);
        }

        public static ICallbackRowBuilder<TObj, TStates> NextFromCallback<TObj, TStates>(this ICallbackRowBuilder<TObj, TStates> builder, 
            string text, 
            string data, 
            TStates nextStateId,
            bool force)
        {
            return builder.NextFromCallback(new CallbackData
            {
                Text = text,
                CallbackText = data
            }, nextStateId, force);
        }
    
        public static ICallbackRowBuilderBase<TObj, TStates> NextFromCallback<TObj, TStates>(this ICallbackRowBuilderBase<TObj, TStates> builder, 
            string text, 
            string data, 
            TStates nextStateId, bool force)
        {
            return builder.NextFromCallback(new CallbackData
            {
                Text = text,
                CallbackText = data
            }, nextStateId, force);
        }
    }
}