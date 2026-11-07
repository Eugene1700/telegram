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

namespace SimpleHandlers.Services.Commands;

[Command(Name = "name_reply", Authorized = false)]
public abstract class NameReplyCommand(ISampleMessageSender sender)
{
    public Task<ICommandExecutionResult> Execute<TQuery>(IQueryContext<TQuery> query, NameReplyData data) => Reply(query.GetChatId(), query.GetData());

    private async Task<ICommandExecutionResult> Reply(long chatId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            await sender.Send(chatId, SampleMessages.Text("Please send your name as text."));
            return CommandExecutionResult.Freeze();
        }
        await sender.Send(chatId, SampleMessages.Text($"Nice to meet you, {text}!"));
        return CommandExecutionResult.Break();
    }
}
