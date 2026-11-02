using System;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Fluent.Builders.StateBuilders;

namespace Bot.Commands.Core.Fluent.Builders.Extensions
{
    public static class StateBuilderExtensions
    {
        public static IMessageBuilder<TObj, TStates> WithMessage<TObj, TStates>(
            this IStateBuilder<TObj, TStates> builder, string text,
            Func<object, TStates, TObj, IBotMessage, Task> sender) =>
            builder.WithMessage((s, o) => Task.FromResult<ITextMessage>(new BotMessage(text, null, BotMessageParseMode.Plain, null)), sender);

        public static IMessageBuilder<TObj, TStates> WithMessage<TObj, TStates>(
            this IStateBuilder<TObj, TStates> builder, Func<TStates, TObj, Task<string>> provider,
            Func<object, TStates, TObj, IBotMessage, Task> sender) =>
            builder.WithMessage(async (s, o) => new BotMessage(await provider(s, o), null, BotMessageParseMode.Plain, null), sender);

        public static IMessageBuilderBase<TObj, TStates> WithMessage<TObj, TStates>(
            this IStateBuilderBase<TObj, TStates> builder, string text,
            Func<object, TStates, TObj, IBotMessage, Task> sender) =>
            builder.WithMessage((s, o) => Task.FromResult<ITextMessage>(new BotMessage(text, null, BotMessageParseMode.Plain, null)), sender);

        public static IMessageBuilder<TObj, TStates> WithMessage<TObj, TStates, TQuery>(
            this IStateBuilder<TObj, TStates> stateBuilder, ITextMessage textMessage,
            Func<QueryContext<TQuery>, TStates, TObj, IBotMessage, Task> sender)
        {
            return stateBuilder.WithMessage((s, o) => Task.FromResult(textMessage), sender);
        }

    }
}
