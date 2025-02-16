using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Services;

namespace Telegram.Commands.Core.Fluent.Builders.CallbackBuilders.Extensions
{
    public static class CallbackRowBuilderExtensions
    {
        public static ICallbackRowBuilder<TObj, TStates> OnCallback<TObj, TStates, TQuery>(this ICallbackRowBuilder<TObj, TStates> builder,
            string text,
            string data, Func<TQuery, TStates, TObj, string, Task<TStates>> handler, bool forceRerender = true) where TQuery : class
        {
            return builder.OnCallback(new CallbackData
            {
                Text = text,
                CallbackText = data
            }, handler, forceRerender);
        }

        public static ICallbackRowBuilder<TObj, TStates> OnCallback<TObj, TStates, TQuery>(this ICallbackRowBuilder<TObj, TStates> builder,
            CallbackData callback, 
            Func<TQuery, TStates, TObj, string, Task<TStates>> handler, bool forceRerender = true) where TQuery : class
        {
            return builder.OnCallback((_,_) => callback, handler, forceRerender);
        }
    
        public static ICallbackRowBuilderBase<TObj, TStates> OnCallback<TObj, TStates, TQuery>(this ICallbackRowBuilderBase<TObj, TStates> builder, 
            CallbackData callback, 
            Func<TQuery, TStates, TObj, string, Task<TStates>> handler, bool forceRerender = true) where TQuery : class
        {
            return builder.OnCallback<TQuery>( (_,_) => callback, async (cq, s, o, d) =>
            {
                var res = await handler(cq, s, o, d);
                return res;
            }, forceRerender);
        }
    }
}