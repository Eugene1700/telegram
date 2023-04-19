using System;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;

public static class CallbackRowBuilderOnlyMoveExtensions {
    
    public static ICallbackRowBuilder<TObj> NextFromCallback<TObj, TEnum>(this ICallbackRowBuilder<TObj> builder, 
        string callbackId, 
        Func<TObj, CallbackData> callbackProvider, 
        TEnum nextStateId) where TEnum : Enum
    {
        return builder.NextFromCallback(callbackId, callbackProvider, nextStateId.ToString());
    }
    
    public static ICallbackRowBuilder<TObj> NextFromCallback<TObj, TEnum>(this ICallbackRowBuilder<TObj> builder, 
        string callbackId, 
        CallbackData callback, TEnum nextStateId) where TEnum : Enum
    {
        return builder.NextFromCallback(callbackId, _ => callback, nextStateId.ToString());
    }
    
    public static ICallbackRowBuilderBase<TObj> NextFromCallback<TObj, TEnum>(this ICallbackRowBuilderBase<TObj> builder, 
        string callbackId, 
        CallbackData callback, 
        TEnum nextStateId) where TEnum : Enum
    {
        return builder.NextFromCallback(callbackId, _ => callback, nextStateId.ToString());
    }

    public static ICallbackRowBuilder<TObj> NextFromCallback<TObj, TEnum>(this ICallbackRowBuilder<TObj> builder, 
        string callbackId, 
        string text, 
        string data, 
        TEnum nextStateId) where TEnum : Enum
    {
        return builder.NextFromCallback(callbackId, new CallbackData
        {
            Text = text,
            CallbackText = data
        }, nextStateId);
    }
    
    public static ICallbackRowBuilderBase<TObj> NextFromCallback<TObj, TEnum>(this ICallbackRowBuilderBase<TObj> builder, 
        string callbackId, 
        string text, 
        string data, 
        TEnum nextStateId) where TEnum : Enum
    {
        return builder.NextFromCallback(callbackId, new CallbackData
        {
            Text = text,
            CallbackText = data
        }, nextStateId);
    }
}