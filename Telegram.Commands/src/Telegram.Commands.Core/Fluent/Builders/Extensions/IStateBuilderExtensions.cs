using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Messages;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;
using Telegram.Commands.Core.Messages;

namespace Telegram.Commands.Core.Fluent.Builders.Extensions
{
    public static class StateBuilderExtensions
    {
        public static IMessageBuilder<TObj, TStates> WithMessage<TObj, TStates>(
            this IStateBuilder<TObj, TStates> stateBuilder, ITextMessage textMessage,
            Func<object, TStates, TObj, ITelegramMessage, Task> sender)
        {
            return stateBuilder.WithMessage((s, o) => Task.FromResult(textMessage), sender);
        }
        
        public static IMessageBuilder<TObj, TStates> WithMessage<TObj, TStates, TParseMode>(
            this IStateBuilder<TObj, TStates> stateBuilder, ITextMessageTyped<TParseMode> textMessage,
            Func<object, TStates, TObj, ITelegramMessageTyped<TParseMode>, Task> sender)
        {
            Task NewSender(object q, TStates s, TObj o, ITelegramMessage m) => sender(q, s, o, m.Cast<TParseMode>());
            return stateBuilder.WithMessage((s, o) => Task.FromResult(textMessage as ITextMessage), NewSender);
        }
        
        public static IMessageBuilder<TObj, TStates> WithMessage<TObj, TStates, TParseMode>(
            this IStateBuilder<TObj, TStates> stateBuilder, Func<TStates, TObj, Task<ITextMessageTyped<TParseMode>>> messageProvider,
            Func<object, TStates, TObj, ITelegramMessageTyped<TParseMode>, Task> sender)
        {
            Task NewSender(object q, TStates s, TObj o, ITelegramMessage m) => sender(q, s, o, m.Cast<TParseMode>());
            async Task<ITextMessage> NewProvider(TStates s, TObj o) => await messageProvider(s, o);
            return stateBuilder.WithMessage(NewProvider, NewSender);
        }
        
        public static IMessageBuilder<TObj, TStates> WithMessage<TObj, TStates, TParseMode>(
            this IStateBuilder<TObj, TStates> stateBuilder, Func<TStates, TObj, Task<(string, TParseMode)>> messageProvider,
            Func<object, TStates, TObj, ITelegramMessageTyped<TParseMode>, Task> sender)
        {
            Task NewSender(object q, TStates s, TObj o, ITelegramMessage m) => sender(q, s, o, m.Cast<TParseMode>());
            async Task<ITextMessage> NewProvider(TStates s, TObj o)
            {
                var (mes, parseMode) = await messageProvider(s, o);
                return new TextMessage(mes, parseMode);
            }

            return stateBuilder.WithMessage(NewProvider, NewSender);
        }

        public static IMessageBuilderBase<TObj, TStates> WithMessage<TObj, TStates>(
            this IStateBuilderBase<TObj, TStates> stateBuilder,
            ITextMessage textMessage,
            Func<object, TStates, TObj, ITelegramMessage, Task> sender)
        {
            return stateBuilder.WithMessage((s, o) => Task.FromResult(textMessage), sender);
        }
        
        public static IMessageBuilderBase<TObj, TStates> WithMessage<TObj, TStates, TParseMode>(
            this IStateBuilderBase<TObj, TStates> stateBuilder, ITextMessageTyped<TParseMode> textMessage,
            Func<object, TStates, TObj, ITelegramMessageTyped<TParseMode>, Task> sender)
        {
            Task NewSender(object q, TStates s, TObj o, ITelegramMessage m) => sender(q, s, o, m.Cast<TParseMode>());
            return stateBuilder.WithMessage((s, o) => Task.FromResult(textMessage as ITextMessage), NewSender);
        }
        
        public static IMessageBuilderBase<TObj, TStates> WithMessage<TObj, TStates, TParseMode>(
            this IStateBuilderBase<TObj, TStates> stateBuilder, Func<TStates, TObj, Task<ITextMessageTyped<TParseMode>>> messageProvider,
            Func<object, TStates, TObj, ITelegramMessageTyped<TParseMode>, Task> sender)
        {
            Task NewSender(object q, TStates s, TObj o, ITelegramMessage m) => sender(q, s, o, m.Cast<TParseMode>());
            async Task<ITextMessage> NewProvider(TStates s, TObj o) => await messageProvider(s, o) as ITextMessage;
            return stateBuilder.WithMessage(NewProvider, NewSender);
        }
        
        public static IMessageBuilderBase<TObj, TStates> WithMessage<TObj, TStates, TParseMode>(
            this IStateBuilderBase<TObj, TStates> stateBuilder, Func<TStates, TObj, Task<(string, TParseMode)>> messageProvider,
            Func<object, TStates, TObj, ITelegramMessageTyped<TParseMode>, Task> sender)
        {
            Task NewSender(object q, TStates s, TObj o, ITelegramMessage m) => sender(q, s, o, m.Cast<TParseMode>());
            async Task<ITextMessage> NewProvider(TStates s, TObj o)
            {
                var (mes, parseMode) = await messageProvider(s, o);
                return new TextMessage(mes, parseMode);
            }

            return stateBuilder.WithMessage(NewProvider, NewSender);
        }
    }
}