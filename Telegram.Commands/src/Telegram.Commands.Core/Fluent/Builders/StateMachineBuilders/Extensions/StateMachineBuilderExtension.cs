﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Commands.Abstract.Messages;
using Telegram.Commands.Core.Fluent.Builders.StateBuilders;
using Telegram.Commands.Core.Messages;

namespace Telegram.Commands.Core.Fluent.Builders.StateMachineBuilders.Extensions
{

    public static class StateMachineBuilderExtension
    {
        public static IStateBuilder<TObj, TStates> State<TObj, TStates, TParseMode>(
            this IStateMachineBuilder<TObj, TStates> builder, TStates stateId,
            Func<object, TStates, TObj, ITelegramMessageTyped<TParseMode>[], Task> sender,
            uint? durationInSec = null)
        {
            Func<object, TStates, TObj, ITelegramMessage[], Task> newSender = (q, s, o, mes) =>
            {
                var mm = mes.Select(x => x as ITelegramMessageTyped<TParseMode>);
                return sender(q, s, o, mm.ToArray());
            };
            return builder.State(stateId, newSender);
        }

        public static IStateBuilder<TObj, TStates> State<TObj, TStates>(
            this IStateMachineBuilder<TObj, TStates> builder, TStates stateId,
            Func<object, TStates, TObj, ITelegramMessageTyped<TelegramParseMode>[], Task> sender,
            uint? durationInSec = null)
        {
            Func<object, TStates, TObj, ITelegramMessage[], Task> newSender = (q, s, o, mes) =>
            {
                var mm = mes.Select(x => new TelegramMessageTyped<TelegramParseMode>(x));
                return sender(q, s, o, mm.Cast<ITelegramMessageTyped<TelegramParseMode>>().ToArray());
            };
            return builder.State(stateId, newSender);
        }
    }
}