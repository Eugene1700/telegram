using System.Threading.Tasks;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core;
using Bot.Commands.Core.Models;

namespace SimpleHandlers.Services.Commands
{
    [Command(Name = "start", Authorized = false)]
    public class StartCommand: IQueryCommand<Message>, IQueryCommand<global::Max.Bot.Types.Message>
    {
        private readonly ISampleMessageSender _sender;

        public StartCommand(ISampleMessageSender sender)
        {
            _sender = sender;
        }
        public async Task<ICommandExecutionResult> Execute(QueryContext<Message> context)
        {
            var query = context.Query;
            await _sender.Send(context.GetChatId(), SampleMessages.Text(
                $"Hello, {query.From?.Username ?? "User"}! Try /ask or /profile."));
            return CommandExecutionResult.Break();
        }

        public async Task<ICommandExecutionResult> Execute(QueryContext<global::Max.Bot.Types.Message> context)
        {
            await _sender.Send(context.GetChatId(), SampleMessages.Text("Hello! Try /ask or /profile."));
            return CommandExecutionResult.Break();
        }
    }
}
