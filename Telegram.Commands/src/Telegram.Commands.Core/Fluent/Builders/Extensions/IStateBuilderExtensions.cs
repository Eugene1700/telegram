using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.Extensions
{
    public static class StateBuilderExtensions
    {
        public static IMessageBuilder<TObj, TStates> WithMessage<TObj, TStates>(
            this IStateBuilder<TObj, TStates> stateBuilder, IMessageText message,
            Func<object, TStates, TObj, ITelegramMessage, Task> sender)
        {
            return stateBuilder.WithMessage((s, o) => Task.FromResult(message), sender);
        }
        
        public static IMessageBuilder<TObj, TStates> WithMessage<TObj, TStates, TParseMode>(
            this IStateBuilder<TObj, TStates> stateBuilder, IMessageTextTyped<TParseMode> message,
            Func<object, TStates, TObj, ITelegramMessageTyped<TParseMode>, Task> sender)
        {
            Task NewSender(object q, TStates s, TObj o, ITelegramMessage m) => sender(q, s, o, m.Cast<TParseMode>());
            return stateBuilder.WithMessage((s, o) => Task.FromResult(message as IMessageText), NewSender);
        }
        
        public static IMessageBuilder<TObj, TStates> WithMessage<TObj, TStates, TParseMode>(
            this IStateBuilder<TObj, TStates> stateBuilder, Func<TStates, TObj, Task<IMessageTextTyped<TParseMode>>> messageProvider,
            Func<object, TStates, TObj, ITelegramMessageTyped<TParseMode>, Task> sender)
        {
            Task NewSender(object q, TStates s, TObj o, ITelegramMessage m) => sender(q, s, o, m.Cast<TParseMode>());
            async Task<IMessageText> NewProvider(TStates s, TObj o) => await messageProvider(s, o);
            return stateBuilder.WithMessage(NewProvider, NewSender);
        }
        
        public static IMessageBuilder<TObj, TStates> WithMessage<TObj, TStates, TParseMode>(
            this IStateBuilder<TObj, TStates> stateBuilder, Func<TStates, TObj, Task<(string, TParseMode)>> messageProvider,
            Func<object, TStates, TObj, ITelegramMessageTyped<TParseMode>, Task> sender)
        {
            Task NewSender(object q, TStates s, TObj o, ITelegramMessage m) => sender(q, s, o, m.Cast<TParseMode>());
            async Task<IMessageText> NewProvider(TStates s, TObj o)
            {
                var (mes, parseMode) = await messageProvider(s, o);
                return new TextMessage(mes, parseMode);
            }

            return stateBuilder.WithMessage(NewProvider, NewSender);
        }

        public static IMessageBuilderBase<TObj, TStates> WithMessage<TObj, TStates>(
            this IStateBuilderBase<TObj, TStates> stateBuilder,
            IMessageText message,
            Func<object, TStates, TObj, ITelegramMessage, Task> sender)
        {
            return stateBuilder.WithMessage((s, o) => Task.FromResult(message), sender);
        }
        
        public static IMessageBuilderBase<TObj, TStates> WithMessage<TObj, TStates, TParseMode>(
            this IStateBuilderBase<TObj, TStates> stateBuilder, IMessageTextTyped<TParseMode> message,
            Func<object, TStates, TObj, ITelegramMessageTyped<TParseMode>, Task> sender)
        {
            Task NewSender(object q, TStates s, TObj o, ITelegramMessage m) => sender(q, s, o, m.Cast<TParseMode>());
            return stateBuilder.WithMessage((s, o) => Task.FromResult(message as IMessageText), NewSender);
        }
        
        public static IMessageBuilderBase<TObj, TStates> WithMessage<TObj, TStates, TParseMode>(
            this IStateBuilderBase<TObj, TStates> stateBuilder, Func<TStates, TObj, Task<IMessageTextTyped<TParseMode>>> messageProvider,
            Func<object, TStates, TObj, ITelegramMessageTyped<TParseMode>, Task> sender)
        {
            Task NewSender(object q, TStates s, TObj o, ITelegramMessage m) => sender(q, s, o, m.Cast<TParseMode>());
            async Task<IMessageText> NewProvider(TStates s, TObj o) => await messageProvider(s, o) as IMessageText;
            return stateBuilder.WithMessage(NewProvider, NewSender);
        }
        
        public static IMessageBuilderBase<TObj, TStates> WithMessage<TObj, TStates, TParseMode>(
            this IStateBuilderBase<TObj, TStates> stateBuilder, Func<TStates, TObj, Task<(string, TParseMode)>> messageProvider,
            Func<object, TStates, TObj, ITelegramMessageTyped<TParseMode>, Task> sender)
        {
            Task NewSender(object q, TStates s, TObj o, ITelegramMessage m) => sender(q, s, o, m.Cast<TParseMode>());
            async Task<IMessageText> NewProvider(TStates s, TObj o)
            {
                var (mes, parseMode) = await messageProvider(s, o);
                return new TextMessage(mes, parseMode);
            }

            return stateBuilder.WithMessage(NewProvider, NewSender);
        }
    }
}