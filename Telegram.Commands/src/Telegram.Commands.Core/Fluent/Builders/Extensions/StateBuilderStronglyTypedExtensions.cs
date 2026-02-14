using System;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Messages;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;

namespace Telegram.Commands.Core.Fluent.Builders.Extensions
{

    public static class StateBuilderStronglyTypedExtensions
    {
        public static IMessageBuilder<TObj, TStates> WithMessage<TObj, TStates>(
            this IStateBuilder<TObj, TStates> stateBuilder, string message,
            Func<object, TStates, TObj, IBotMessage, Task> sender)
        {
            return stateBuilder.WithMessage((s, o) =>
            {
                ITextMessage textMessage =
                    new BotMessage(message, [], BotMessageParseMode.Plain, null);
                return Task.FromResult(textMessage);
            }, sender);
        }

        public static IMessageBuilder<TObj, TStates> WithMessage<TObj, TStates>(
            this IStateBuilder<TObj, TStates> stateBuilder, Func<TStates, TObj, Task<string>> message,
            Func<object, TStates, TObj, IBotMessage, Task> sender)
        {
            return stateBuilder.WithMessage(async (s, o) =>
            {
                var m = await message(s, o);
                ITextMessage
                    textMessage = new BotMessage(m, [], BotMessageParseMode.Plain, null);
                return textMessage;
            }, sender);
        }
    }
}