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

[Command(Name = "ask", Authorized = false)]
public abstract class AskCommand(ISampleMessageSender sender) : IQueryCommand<Tg.Message>, IQueryCommand<Mx.Message>
{
    public async Task<ICommandExecutionResult> Execute(QueryContext<Tg.Message> query)
    {
        await sender.Send(query.GetChatId(), SampleMessages.Text("What should I call you?"));
        return CommandExecutionResult.Ahead<NameReplyCommand, Tg.Message, NameReplyData>(new NameReplyData());
    }

    public async Task<ICommandExecutionResult> Execute(QueryContext<Mx.Message> query)
    {
        await sender.Send(query.GetChatId(), SampleMessages.Text("What should I call you?"));
        return CommandExecutionResult.Ahead<NameReplyCommand, Mx.Message, NameReplyData>(new NameReplyData());
    }
}
