using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.Abstract.Interfaces.Commands;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders;

public interface ICallbacksBuilder<TObj> : IStateBuilderBase<TObj>
{
    ICallbackRowBuilder<TObj> Row();
}

public interface ICallbackRowBuilder<TObj> : ICallbacksBuilder<TObj>
{
    ICallbackRowBuilder<TObj> ExitStateByCallback<TQuery>(string callbackId, Func<TObj, CallbackData> callbackProvider,
        Func<TQuery, TObj, Task<string>> commitExpr) where TQuery : class;
    ICallbackRowBuilder<TObj> ExitStateByCallback(string callbackId, Func<TObj, CallbackData> callbackProvider, string stateId);
    ICallbackRowBuilder<TObj> ExitStateByCallback(CallbackDataWithCommand callbackDataWithCommand);
    ICallbackRowBuilder<TObj> ExitStateByCallback(Func<TObj, CallbackData> callbackProvider,
        ITelegramCommandDescriptor telegramCommandDescriptor);

}

public static class CallbackRowBuilderExtensions
{
    public static ICallbackRowBuilder<TObj> ExitStateByCallback<TObj, TQuery>(this ICallbackRowBuilder<TObj> builder, string callbackId,
        string text,
        string data, Func<TQuery, TObj, Task<string>> commitExpr) where TQuery : class
    {
        return builder.ExitStateByCallback(callbackId, new CallbackData
        {
            Text = text,
            CallbackText = data
        }, commitExpr);
    }

    public static ICallbackRowBuilder<TObj> ExitStateByCallback<TObj, TQuery>(this ICallbackRowBuilder<TObj> builder, string callbackId, 
        CallbackData callback, Func<TQuery, TObj, Task<string>> commitExpr) where TQuery : class
    {
        return builder.ExitStateByCallback(callbackId,_ => callback, commitExpr);
    }

    public static ICallbackRowBuilder<TObj> ExitStateByCallback<TObj, TQuery, TEnum>(this ICallbackRowBuilder<TObj> builder, string callbackId,
        CallbackData callback, Func<TQuery, TObj, Task<TEnum>> commitExpr) where TQuery : class
    {
        return builder.ExitStateByCallback<TQuery>(callbackId, _ => callback, async (cq, o) =>
        {
            var res = await commitExpr(cq, o);
            return res.ToString();
        });
    }

    public static ICallbackRowBuilder<TObj> ExitStateByCallback<TObj, TQuery, TEnum >(this ICallbackRowBuilder<TObj> builder, string callbackId,
        string text, string data, Func<TQuery, TObj, Task<TEnum>> commitExpr) where TQuery : class
    {
        return builder.ExitStateByCallback(callbackId, new CallbackDataWithCommand
        {
            Text = text,
            CallbackText = data
        }, commitExpr);
    }
}

public static class CallbackRowBuilderStronglyTypedExtensions {
    public static ICallbackRowBuilder<TObj> ExitStateByCallback<TObj, TEnum>(this ICallbackRowBuilder<TObj> builder, string callbackId,
        string text, string data, Func<CallbackQuery, TObj, Task<TEnum>> handler)
    {
        return builder.ExitStateByCallback(callbackId, new CallbackDataWithCommand
        {
            Text = text,
            CallbackText = data
        }, handler);
    }
}

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

public static class CallbackRowBuilderExitCommandExtensions
{
    public static  ICallbackRowBuilder<TObj> ExitStateByCallback<TObj, TCommand>(this ICallbackRowBuilder<TObj> builder, Func<TObj, CallbackData> callbackProvider)
        where TCommand : IQueryTelegramCommand<CallbackQuery>
    {
        return builder.ExitStateByCallback(o =>
        {
            var callbackData = callbackProvider(o);
            return new CallbackData
            {
                Text = callbackData.Text,
                CallbackMode = callbackData.CallbackMode,
                CallbackText = callbackData.CallbackText,
            };
        }, TelegramCommandExtensions.GetCommandInfo<TCommand, CallbackQuery>());
    }

    public static ICallbackRowBuilder<TObj> ExitStateByCallback<TObj, TCommand>(this ICallbackRowBuilder<TObj> builder, string text, string data)
        where TCommand : IQueryTelegramCommand<CallbackQuery>
    {
        return builder.ExitStateByCallback<TObj, TCommand>(_ => new CallbackData
        {
            Text = text,
            CallbackText = data
        });
    }

}