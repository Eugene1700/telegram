using System;
using System.Text.Json;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SimpleHandlers.Services;
using SimpleHandlers.Services.Commands;
using VkNet.Abstractions;
using VkNet.Enums.StringEnums;
using VkNet.Model;

namespace SimpleHandlers.Controllers;

[ApiController]
public sealed class VkMessageController(ILogger<VkMessageController> logger, IConfiguration configuration) : ControllerBase
{
    [HttpPost(Settings.VkRouteToUpdate)]
    [RequestSizeLimit(1024 * 1024)]
    [ServiceFilter(typeof(WebhookAccessFilter))]
    [AllowAnonymous]
    public async Task<IActionResult> Update([FromBody] JsonElement body,
        [FromServices] IVkApi client, [FromServices] IBotCommandResolver<GroupUpdate> resolver,
        [FromServices] VkSampleMessageSender sender)
    {
        // The resource filter validates group and secret before resolving action services.
        if (body.ValueKind != JsonValueKind.Object || !body.TryGetProperty("type", out var type) ||
            type.ValueKind != JsonValueKind.String) return BadRequest();
        if (type.GetString() == "confirmation")
            return Content(configuration["VkBotProfile:ConfirmationCode"], "text/plain");
        try
        {
            var update = JsonConvert.DeserializeObject<GroupUpdate>(body.GetRawText())
                ?? throw new InvalidOperationException("Empty VK update.");
            if (update.Type?.Value == GroupUpdateType.MessageEvent && update.Instance is MessageEvent callback)
                await client.Messages.SendMessageEventAnswerAsync(callback.EventId,
                    callback.UserId ?? throw new InvalidOperationException("Missing callback user."),
                    callback.PeerId ?? throw new InvalidOperationException("Missing callback peer."),
                    new EventData { Type = MessageEventType.ShowSnackbar, Text = "Received" });
            await resolver.Handle(update);
            return Content("ok", "text/plain");
        }
        catch (CommandExtractionException exception)
        {
            await sender.Send(exception.ChatId, SampleMessages.Text("Unknown command. Try /start, /ask or /profile."));
            return Content("ok", "text/plain");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to handle VK update");
            return StatusCode(500);
        }
    }
}
