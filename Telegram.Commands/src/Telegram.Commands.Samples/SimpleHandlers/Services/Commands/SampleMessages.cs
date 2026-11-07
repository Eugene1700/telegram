using System;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core.Fluent;
using Bot.Commands.Core.Fluent.Builders.Extensions;
using Bot.Commands.Core.Fluent.Builders.StateMachineBuilders;
using Bot.Commands.Core.Fluent.StateMachine;
using Bot.Commands.Core.Models;
using Tg = global::Telegram.Bot.Types;
using Mx = global::Max.Bot.Types;

namespace SimpleHandlers.Services.Commands;

public static class SampleMessages
{
    public static IBotMessage Text(string text) => new BotMessage(text, null, null, null);
    public static IBotMessage WithKeyboard(string text, Bot.Commands.Core.Services.InlineMarkupQueryBuilder builder)
        => new BotMessage(text, null, null, builder.GetCallbacks());
}
