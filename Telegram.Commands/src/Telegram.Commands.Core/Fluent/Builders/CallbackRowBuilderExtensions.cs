using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders;

public static class CallbackRowBuilderExtensions
{
    public static ICallbackRowBuilder<TObj> ExitStateByCallback<TObj, TQuery>(this ICallbackRowBuilder<TObj> builder, string callbackId,
        string text,
        string data, Func<TQuery, TObj, string, Task<string>> commitExpr) where TQuery : class
    {
        return builder.ExitStateByCallback(callbackId, new CallbackData
        {
            Text = text,
            CallbackText = data
        }, commitExpr);
    }

    public static ICallbackRowBuilder<TObj> ExitStateByCallback<TObj, TQuery>(this ICallbackRowBuilder<TObj> builder, string callbackId, 
        CallbackData callback, Func<TQuery, TObj, string, Task<string>> commitExpr) where TQuery : class
    {
        return builder.ExitStateByCallback(callbackId,_ => callback, commitExpr);
    }

    public static ICallbackRowBuilder<TObj> ExitStateByCallback<TObj, TQuery, TEnum>(this ICallbackRowBuilder<TObj> builder, string callbackId,
        CallbackData callback, Func<TQuery, TObj, string, Task<TEnum>> commitExpr) where TQuery : class
    {
        return builder.ExitStateByCallback<TQuery>(callbackId, _ => callback, async (cq, o, d) =>
        {
            var res = await commitExpr(cq, o, d);
            return res.ToString();
        });
    }

    public static ICallbackRowBuilder<TObj> ExitStateByCallback<TObj, TQuery, TEnum >(this ICallbackRowBuilder<TObj> builder, string callbackId,
        string text, string data, Func<TQuery, TObj,string, Task<TEnum>> commitExpr) where TQuery : class
    {
        return builder.ExitStateByCallback(callbackId, new CallbackDataWithCommand
        {
            Text = text,
            CallbackText = data
        }, commitExpr);
    }
}