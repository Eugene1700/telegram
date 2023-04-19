using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;

public static class CallbackRowBuilderExtensions
{
    public static ICallbackRowBuilder<TObj> OnCallback<TObj, TQuery>(this ICallbackRowBuilder<TObj> builder, 
        string callbackId,
        string text,
        string data, Func<TQuery, TObj, string, Task<string>> handler) where TQuery : class
    {
        return builder.OnCallback(callbackId, new CallbackData
        {
            Text = text,
            CallbackText = data
        }, handler);
    }

    public static ICallbackRowBuilder<TObj> OnCallback<TObj, TQuery>(this ICallbackRowBuilder<TObj> builder, 
        string callbackId, 
        CallbackData callback, 
        Func<TQuery, TObj, string, Task<string>> handler) where TQuery : class
    {
        return builder.OnCallback(callbackId,_ => callback, handler);
    }

    public static ICallbackRowBuilder<TObj> OnCallback<TObj, TQuery, TEnum>(this ICallbackRowBuilder<TObj> builder, 
        string callbackId,
        CallbackData callback, 
        Func<TQuery, TObj, string, Task<TEnum>> handler) where TQuery : class
    {
        return builder.OnCallback<TQuery>(callbackId, _ => callback, async (cq, o, d) =>
        {
            var res = await handler(cq, o, d);
            return res.ToString();
        });
    }
    
    public static ICallbackRowBuilderBase<TObj> OnCallback<TObj, TQuery, TEnum>(this ICallbackRowBuilderBase<TObj> builder, 
        string callbackId,
        CallbackData callback, 
        Func<TQuery, TObj, string, Task<TEnum>> handler) where TQuery : class
    {
        return builder.OnCallback<TQuery>(callbackId, _ => callback, async (cq, o, d) =>
        {
            var res = await handler(cq, o, d);
            return res.ToString();
        });
    }

    public static ICallbackRowBuilder<TObj> OnCallback<TObj, TQuery, TEnum >(this ICallbackRowBuilder<TObj> builder, 
        string callbackId,
        string text, 
        string data, 
        Func<TQuery, TObj,string, Task<TEnum>> handler) where TQuery : class
    {
        return builder.OnCallback(callbackId, new CallbackDataWithCommand
        {
            Text = text,
            CallbackText = data
        }, handler);
    }
}