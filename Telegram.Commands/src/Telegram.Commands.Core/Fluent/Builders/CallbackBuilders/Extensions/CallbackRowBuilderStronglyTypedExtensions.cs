﻿using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions;

public static class CallbackRowBuilderStronglyTypedExtensions {
    public static ICallbackRowBuilder<TObj> ExitStateFromCallback<TObj, TEnum>(this ICallbackRowBuilder<TObj> builder, string callbackId,
        string text, string data, Func<CallbackQuery, TObj, string, Task<TEnum>> handler)
    {
        return builder.NextStateFromCallback(callbackId, new CallbackDataWithCommand
        {
            Text = text,
            CallbackText = data
        }, handler);
    }
    
    public static ICallbackRowBuilderBase<TObj> ExitStateFromCallback<TObj, TEnum>(this ICallbackRowBuilderBase<TObj> builder, string callbackId,
        string text, string data, Func<CallbackQuery, TObj, string, Task<TEnum>> handler)
    {
        return builder.NextStateFromCallback(callbackId, new CallbackDataWithCommand
        {
            Text = text,
            CallbackText = data
        }, handler);
    }
}