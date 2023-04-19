using System;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;

public static class CallbackRowBuilderOnlyMoveExtensions {
    
    public static ICallbackRowBuilder<TObj> NextStateFromCallback<TObj, TEnum>(this ICallbackRowBuilder<TObj> builder, string callbackId, 
        Func<TObj, CallbackData> callbackProvider, TEnum stateId) where TEnum : Enum
    {
        return builder.NextStateFromCallback(callbackId, callbackProvider, stateId.ToString());
    }
    
    public static ICallbackRowBuilder<TObj> NextStateFromCallback<TObj, TEnum>(this ICallbackRowBuilder<TObj> builder, string callbackId, 
        CallbackData callback, TEnum stateId) where TEnum : Enum
    {
        return builder.NextStateFromCallback(callbackId, _ => callback, stateId.ToString());
    }
    
    public static ICallbackRowBuilderBase<TObj> NextStateFromCallback<TObj, TEnum>(this ICallbackRowBuilderBase<TObj> builder, string callbackId, 
        CallbackData callback, TEnum stateId) where TEnum : Enum
    {
        return builder.NextStateFromCallback(callbackId, _ => callback, stateId.ToString());
    }

    public static ICallbackRowBuilder<TObj> NextStateFromCallback<TObj, TEnum>(this ICallbackRowBuilder<TObj> builder, string callbackId, 
        string text, string data, TEnum stateId) where TEnum : Enum
    {
        return builder.NextStateFromCallback(callbackId, new CallbackData
        {
            Text = text,
            CallbackText = data
        }, stateId);
    }
    
    public static ICallbackRowBuilderBase<TObj> NextStateFromCallback<TObj, TEnum>(this ICallbackRowBuilderBase<TObj> builder, string callbackId, 
        string text, string data, TEnum stateId) where TEnum : Enum
    {
        return builder.NextStateFromCallback(callbackId, new CallbackData
        {
            Text = text,
            CallbackText = data
        }, stateId);
    }
}