using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions
{
    public static class CallbackRowBuilderExtensions
    {
        public static ICallbackRowBuilder<TObj, TStates> OnCallback<TObj, TStates, TQuery>(this ICallbackRowBuilder<TObj, TStates> builder,
            string text,
            string data, Func<TQuery, TObj, string, Task<TStates>> handler, bool force) where TQuery : class
        {
            return builder.OnCallback(new CallbackData
            {
                Text = text,
                CallbackText = data
            }, handler, force);
        }

        public static ICallbackRowBuilder<TObj, TStates> OnCallback<TObj, TStates, TQuery>(this ICallbackRowBuilder<TObj, TStates> builder,
            CallbackData callback, 
            Func<TQuery, TObj, string, Task<TStates>> handler, bool force) where TQuery : class
        {
            return builder.OnCallback(_ => callback, handler, force);
        }
    
        public static ICallbackRowBuilderBase<TObj, TStates> OnCallback<TObj, TStates, TQuery>(this ICallbackRowBuilderBase<TObj, TStates> builder, 
            CallbackData callback, 
            Func<TQuery, TObj, string, Task<TStates>> handler, bool force) where TQuery : class
        {
            return builder.OnCallback<TQuery>( _ => callback, async (cq, o, d) =>
            {
                var res = await handler(cq, o, d);
                return res;
            }, force);
        }
    }
}