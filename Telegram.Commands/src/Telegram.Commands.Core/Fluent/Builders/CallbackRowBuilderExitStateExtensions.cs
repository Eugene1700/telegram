using System;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders;

public static class CallbackRowBuilderExitStateExtensions {
    
    public static ICallbackRowBuilder<TObj> ExitStateByCallback<TObj, TEnum>(this ICallbackRowBuilder<TObj> builder, string callbackId, 
        Func<TObj, CallbackData> callbackProvider, TEnum stateId) where TEnum : Enum
    {
        return builder.ExitStateByCallback(callbackId, callbackProvider, stateId.ToString());
    }
    
    public static ICallbackRowBuilder<TObj> ExitStateByCallback<TObj, TEnum>(this ICallbackRowBuilder<TObj> builder, string callbackId, 
        CallbackData callback, TEnum stateId) where TEnum : Enum
    {
        return builder.ExitStateByCallback(callbackId, _ => callback, stateId.ToString());
    }

    public static ICallbackRowBuilder<TObj> ExitStateByCallback<TObj, TEnum>(this ICallbackRowBuilder<TObj> builder, string callbackId, 
        string text, string data, TEnum stateId) where TEnum : Enum
    {
        return builder.ExitStateByCallback(callbackId, new CallbackData
        {
            Text = text,
            CallbackText = data
        }, stateId);
    }
}