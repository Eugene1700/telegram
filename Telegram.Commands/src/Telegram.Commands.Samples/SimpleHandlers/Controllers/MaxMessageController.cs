using System;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleHandlers.Services;
using Microsoft.Extensions.Logging;
using global::Max.Bot;
using global::Max.Bot.Types;
using global::Max.Bot.Types.Enums;
using global::Max.Bot.Types.Requests;

namespace SimpleHandlers.Controllers;

[ApiController]
public sealed class MaxMessageController(
    ILogger<MaxMessageController> logger) : ControllerBase
{
    [HttpPost(Settings.MaxRouteToUpdate)]
    [ServiceFilter(typeof(WebhookAccessFilter))]
    [AllowAnonymous]
    public async Task<IActionResult> Update([FromBody] Update update, [FromServices] MaxClient client,
        [FromServices] IBotCommandResolver<Update> resolver)
    {
        try
        {
            if (update.Type == UpdateType.BotStarted)
            {
                await client.Messages.SendMessageAsync(update.ChatId.Value, "Hello! Try /start, /ask or /profile.");
                return Ok();
            }
            if (update.Callback?.CallbackId is { Length: > 0 } callbackId)
                await client.Messages.AnswerCallbackQueryAsync(callbackId,
                    new AnswerCallbackQueryRequest { Notification = "Received" });
            await resolver.Handle(update);
            return Ok();
        }
        catch (CommandExtractionException e)
        {
            await client.Messages.SendMessageAsync(e.ChatId,
                "Unknown command. Try /start, /ask or /profile.");
            return Ok();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to handle Max update");
            return StatusCode(500);
        }
    }
}
