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

[Command(Name = "getphotosize_session", Authorized = true)]
public abstract class GetPhotoSizeSession(ISampleMessageSender sender)
{
    public async Task<ICommandExecutionResult> Execute<TQuery>(QueryContext<TQuery> query,
        List<PhotoSessionObject> data)
    {
        var photo = SampleQuery.Photo(query.Query);
        if (photo == null)
        {
            await sender.Send(query.GetChatId(), SampleMessages.Text("Please send a photo."));
            return CommandExecutionResult.Freeze();
        }

        data.Add(photo);
        await sender.Send(query.GetChatId(),
            SampleMessages.WithKeyboard(
                $"Your photo size=[{photo.PhotoSize?.ToString() ?? "not provided"}]. What should I do next?",
                new InlineMarkupQueryBuilder().AddMenu()));
        return SampleCommandResult.Next<GetPhotoMenuBehavior>(data, null);
    }
}
