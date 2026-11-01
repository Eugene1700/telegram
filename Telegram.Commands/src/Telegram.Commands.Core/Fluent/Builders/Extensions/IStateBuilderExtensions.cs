using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Commands;
using Telegram.Commands.Abstract.Messages;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.Extensions
{
    public static class StateBuilderExtensions
    {
        public static IMessageBuilder<TObj, TStates> WithMessage<TObj, TStates, TQuery>(
            this IStateBuilder<TObj, TStates> stateBuilder, ITextMessage textMessage,
            Func<QueryContext<TQuery>, TStates, TObj, IBotMessage, Task> sender)
        {
            return stateBuilder.WithMessage((s, o) => Task.FromResult(textMessage), sender);
        }
        
        public static IMessageBuilder<TObj, TStates> WithMessage<TObj, TStates, TQuery>(
            this IStateBuilder<TObj, TStates> stateBuilder, string textMessage,
            Func<QueryContext<TQuery>, TStates, TObj, IBotMessage, Task> sender)
        {
            return stateBuilder.WithMessage((s, o) => Task.FromResult(textMessage), sender);
        }
        
        public static IMessageBuilderBase<TObj, TStates> WithMessage<TObj, TStates>(
            this IStateBuilderBase<TObj, TStates> stateBuilder, Func<TStates, TObj, Task<ITextMessage>> messageProvider,
            Func<object, TStates, TObj, IBotMessage, Task> sender)
        {
            return stateBuilder.WithMessage(NewProvider, NewSender);
            async Task<ITextMessage> NewProvider(TStates s, TObj o) => await messageProvider(s, o);
            Task NewSender(object q, TStates s, TObj o, IBotMessage m) => sender(q, s, o, m);
        }
    }
}