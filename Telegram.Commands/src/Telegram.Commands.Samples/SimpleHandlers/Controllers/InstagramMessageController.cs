using System;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Exceptions;
using Instagram.Messaging;
using Instagram.Messaging.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleHandlers.Services;
using SimpleHandlers.Services.Commands;

namespace SimpleHandlers.Controllers;

[ApiController]
[ServiceFilter(typeof(InstagramWebhookAccessFilter))]
[AllowAnonymous]
public sealed class InstagramMessageController(IConfiguration configuration,
    InstagramOptions options, IServiceScopeFactory scopes, ILogger<InstagramMessageController> logger) : ControllerBase
{
    [HttpGet(Settings.InstagramRouteToUpdate)]
    public IActionResult Verify([FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string token, [FromQuery(Name = "hub.challenge")] string challenge)
    {
        var expected = configuration["InstagramBotProfile:VerifyToken"];
        return mode == "subscribe" && !string.IsNullOrEmpty(expected) && token == expected && !string.IsNullOrEmpty(challenge)
            ? Content(challenge, "text/plain") : Unauthorized();
    }

    [HttpPost(Settings.InstagramRouteToUpdate)]
    [RequestSizeLimit(1024 * 1024)]
    public async Task<IActionResult> Update([FromBody] InstagramWebhookUpdate update)
    {
        if (update.Object != "instagram") return Content("EVENT_RECEIVED", "text/plain");
        try
        {
            foreach (var entry in update.Entries ?? Array.Empty<InstagramWebhookEntry>())
            {
                if (entry?.Id != options.AccountId) continue;
                foreach (var item in entry.Messaging ?? Array.Empty<InstagramMessagingEvent>())
                {
                    if (item?.Recipient?.Id != options.AccountId || item.Sender?.Id == options.AccountId) continue;
                    // Meta batches events for several people. A scoped command must never be reused across them.
                    using var scope = scopes.CreateScope();
                    try
                    {
                        await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<InstagramMessagingEvent>>().Handle(item);
                    }
                    catch (CommandExtractionException exception)
                    {
                        await scope.ServiceProvider.GetRequiredService<ISampleMessageSender>().Send(exception.ChatId,
                            SampleMessages.Text("Unknown command. Try /start, /ask or /profile."));
                    }
                }
            }
            return Content("EVENT_RECEIVED", "text/plain");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to handle Instagram update");
            return StatusCode(500);
        }
    }
}
