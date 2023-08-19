using System;
using System.Threading.Tasks;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.Extensions
{

    public static class StateBuilderStronglyTypedExtensions
    {
        public static IMessageBuilder<TObj, TStates> WithMessage<TObj, TStates>(
            this IStateBuilder<TObj, TStates> stateBuilder, string message,
            Func<object, TStates, TObj, ITelegramMessage, Task> sender)
        {
            return stateBuilder.WithMessage((s, o) =>
            {
                IMessageText textMessage =
                    new TextMessage(message, TelegramParseMode.Plain);
                return Task.FromResult(textMessage);
            }, sender);
        }

        public static IMessageBuilder<TObj, TStates> WithMessage<TObj, TStates>(
            this IStateBuilder<TObj, TStates> stateBuilder, Func<TStates, TObj, Task<string>> message,
            Func<object, TStates, TObj, ITelegramMessage, Task> sender)
        {
            return stateBuilder.WithMessage(async (s, o) =>
            {
                var m = await message(s, o);
                IMessageText
                    textMessage = new TextMessage(m, TelegramParseMode.Plain);
                return textMessage;
            }, sender);
        }
        
        public static IMessageBuilder<TObj, TStates> WithMessage<TObj, TStates>(
            this IStateBuilder<TObj, TStates> stateBuilder, string message,
            Func<object, TStates, TObj, ITelegramMessageTyped<TelegramParseMode>, Task> sender)
        {
            return stateBuilder.WithMessage((s, o) =>
            {
                IMessageTextTyped<TelegramParseMode> textMessage =
                    new TextMessageTyped<TelegramParseMode>(new TextMessage(message, TelegramParseMode.Plain));
                return Task.FromResult(textMessage);
            }, sender);
        }

        public static IMessageBuilderBase<TObj, TStates> WithMessage<TObj, TStates>(
            this IStateBuilderBase<TObj, TStates> stateBuilder, string message,
            Func<object, TStates, TObj, ITelegramMessage, Task> sender)
        {
            return stateBuilder.WithMessage((s, o) =>
            {
                IMessageText textMessage =
                    new TextMessage(message, TelegramParseMode.Plain);
                return Task.FromResult(textMessage);
            }, sender);
        }

        public static IMessageBuilderBase<TObj, TStates> WithMessage<TObj, TStates>(
            this IStateBuilderBase<TObj, TStates> stateBuilder, Func<TStates, TObj, Task<string>> message,
            Func<object, TStates, TObj, ITelegramMessage, Task> sender)
        {
            return stateBuilder.WithMessage(async (s, o) =>
            {
                var m = await message(s, o);
                IMessageText
                    textMessage = new TextMessage(m, TelegramParseMode.Plain);
                return textMessage;
            }, sender);
        }
        
        public static IMessageBuilderBase<TObj, TStates> WithMessage<TObj, TStates>(
            this IStateBuilderBase<TObj, TStates> stateBuilder, string message,
            Func<object, TStates, TObj, ITelegramMessageTyped<TelegramParseMode>, Task> sender)
        {
            return stateBuilder.WithMessage((s, o) =>
            {
                IMessageTextTyped<TelegramParseMode> textMessage =
                    new TextMessageTyped<TelegramParseMode>(new TextMessage(message, TelegramParseMode.Plain));
                return Task.FromResult(textMessage);
            }, sender);
        }
    }
}