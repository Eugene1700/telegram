using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;

public static class CallbackRowBuilderExtensions
{
    public static ICallbackRowBuilder<TObj, TStates, TCallbacks> OnCallback<TObj, TStates, TCallbacks, TQuery>(this ICallbackRowBuilder<TObj, TStates, TCallbacks> builder, 
        TCallbacks callbackId,
        string text,
        string data, Func<TQuery, TObj, string, Task<TStates>> handler, bool force) where TQuery : class
    {
        return builder.OnCallback(callbackId, new CallbackData
        {
            Text = text,
            CallbackText = data
        }, handler, force);
    }

    public static ICallbackRowBuilder<TObj, TStates, TCallbacks> OnCallback<TObj, TStates, TCallbacks, TQuery>(this ICallbackRowBuilder<TObj, TStates, TCallbacks> builder, 
        TCallbacks callbackId, 
        CallbackData callback, 
        Func<TQuery, TObj, string, Task<TStates>> handler, bool force) where TQuery : class
    {
        return builder.OnCallback(callbackId,_ => callback, handler, force);
    }
    
    public static ICallbackRowBuilderBase<TObj, TStates, TCallbacks> OnCallback<TObj, TStates, TCallbacks, TQuery>(this ICallbackRowBuilderBase<TObj, TStates, TCallbacks> builder, 
        TCallbacks callbackId,
        CallbackData callback, 
        Func<TQuery, TObj, string, Task<TStates>> handler, bool force) where TQuery : class
    {
        return builder.OnCallback<TQuery>(callbackId, _ => callback, async (cq, o, d) =>
        {
            var res = await handler(cq, o, d);
            return res;
        }, force);
    }
}