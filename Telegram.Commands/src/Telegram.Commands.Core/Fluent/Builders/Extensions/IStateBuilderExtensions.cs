using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.Extensions
{
    public static class StateBuilderExtensions
    {
        public static IMessageBuilder<TObj, TStates> WithMessage<TObj, TStates>(this IStateBuilder<TObj, TStates> stateBuilder, string message,
            Func<object, TObj, ITelegramMessage, Task> sender)
        {
            return stateBuilder.WithMessage(_ => Task.FromResult(message), sender);
        }
        
        public static IMessageBuilderBase<TObj, TStates> WithMessage<TObj, TStates>(this IStateBuilderBase<TObj, TStates> stateBuilder, 
            string message,
            Func<object, TObj, ITelegramMessage, Task> sender)
        {
            return stateBuilder.WithMessage(_ => Task.FromResult(message), sender);
        }
    }
}