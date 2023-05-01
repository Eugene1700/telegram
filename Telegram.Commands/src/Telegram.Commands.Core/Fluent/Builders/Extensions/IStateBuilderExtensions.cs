using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.Extensions
{
    public static class StateBuilderExtensions
    {
        public static IMessageBuilder<TObj, TStates, TCallbacks> WithMessage<TObj, TStates, TCallbacks>(this IStateBuilder<TObj, TStates, TCallbacks> stateBuilder, string message,
            Func<object, TObj, ITelegramMessage, Task> sender)
        {
            return stateBuilder.WithMessage(_ => Task.FromResult(message), sender);
        }
        
        public static IMessageBuilderBase<TObj, TStates, TCallbacks> WithMessage<TObj, TStates, TCallbacks>(this IStateBuilderBase<TObj, TStates, TCallbacks> stateBuilder, 
            string message,
            Func<object, TObj, ITelegramMessage, Task> sender)
        {
            return stateBuilder.WithMessage(_ => Task.FromResult(message), sender);
        }
    }
}