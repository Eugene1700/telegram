using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Attributes;
using Bot.Commands.Abstract.Commands;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Abstract.Messages;
using Bot.Commands.Core;
using Bot.Commands.Core.Models;
using Bot.Commands.Core.Services;
using Bot.Commands.Core.Exceptions;
using Bot.Commands.UI.DropDown;
using SimpleHandlers.Services.Commands.Models;
namespace SimpleHandlers.Services.Commands;
[Command(Name = "showresult_callback", Authorized = true)]
public abstract class ShowResultCallback(ISampleMessageSender sender)
{
    public async Task<ICommandExecutionResult> Execute<TQuery>(QueryContext<TQuery> query, List<PhotoSessionObject> data)
    {
        var text = "Your result:\n" + string.Join("\n", data.Select(p => $"MessageId: {p.MessageId}, FileSize: {p.PhotoSize?.ToString() ?? "not provided"}"));
await sender.Send(query.GetChatId(), SampleMessages.WithKeyboard(text, new InlineMarkupQueryBuilder().AddMenu()));
return SampleCommandResult.Next<GetPhotoMenuBehavior>(data, null);
    }
}
