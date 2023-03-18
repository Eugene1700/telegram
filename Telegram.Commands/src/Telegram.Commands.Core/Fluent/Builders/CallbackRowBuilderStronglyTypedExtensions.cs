using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders;

public static class CallbackRowBuilderStronglyTypedExtensions {
    public static ICallbackRowBuilder<TObj> ExitStateByCallback<TObj, TEnum>(this ICallbackRowBuilder<TObj> builder, string callbackId,
        string text, string data, Func<CallbackQuery, TObj, string, Task<TEnum>> handler)
    {
        return builder.ExitStateByCallback(callbackId, new CallbackDataWithCommand
        {
            Text = text,
            CallbackText = data
        }, handler);
    }
}