using System;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;

public static class CallbackRowBuilderOnlyMoveExtensions {
    
    public static ICallbackRowBuilder<TObj, TStates, TCallbacks> NextFromCallback<TObj, TStates, TCallbacks>(this ICallbackRowBuilder<TObj, TStates, TCallbacks> builder, 
        TCallbacks callbackId, 
        CallbackData callback, 
        TStates nextStateId)
    {
        return builder.NextFromCallback(callbackId, _ => callback, nextStateId);
    }
    
    public static ICallbackRowBuilderBase<TObj, TStates, TCallbacks> NextFromCallback<TObj, TStates, TCallbacks>(this ICallbackRowBuilderBase<TObj, TStates, TCallbacks> builder, 
        TCallbacks callbackId, 
        CallbackData callback, 
        TStates nextStateId)
    {
        return builder.NextFromCallback(callbackId, _ => callback, nextStateId);
    }

    public static ICallbackRowBuilder<TObj, TStates, TCallbacks> NextFromCallback<TObj, TStates, TCallbacks>(this ICallbackRowBuilder<TObj, TStates, TCallbacks> builder, 
        TCallbacks callbackId, 
        string text, 
        string data, 
        TStates nextStateId)
    {
        return builder.NextFromCallback(callbackId, new CallbackData
        {
            Text = text,
            CallbackText = data
        }, nextStateId);
    }
    
    public static ICallbackRowBuilderBase<TObj, TStates, TCallbacks> NextFromCallback<TObj, TStates, TCallbacks>(this ICallbackRowBuilderBase<TObj, TStates, TCallbacks> builder, 
        TCallbacks callbackId, 
        string text, 
        string data, 
        TStates nextStateId)
    {
        return builder.NextFromCallback(callbackId, new CallbackData
        {
            Text = text,
            CallbackText = data
        }, nextStateId);
    }
}