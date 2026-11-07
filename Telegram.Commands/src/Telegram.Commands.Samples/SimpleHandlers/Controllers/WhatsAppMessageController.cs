using System;
using System.Threading.Tasks;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.Core.Exceptions;
using WhatsApp.Messaging;
using WhatsApp.Messaging.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleHandlers.Services;
using SimpleHandlers.Services.Commands;

namespace SimpleHandlers.Controllers;

[ApiController]
[ServiceFilter(typeof(WhatsAppWebhookAccessFilter))]
[AllowAnonymous]
public sealed class WhatsAppMessageController(IConfiguration configuration,
    WhatsAppOptions options, IServiceScopeFactory scopes, ILogger<WhatsAppMessageController> logger) : ControllerBase
{
    [HttpGet(Settings.WhatsAppRouteToUpdate)]
    public IActionResult Verify([FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string token, [FromQuery(Name = "hub.challenge")] string challenge)
    {
        var expected = configuration["WhatsAppBotProfile:VerifyToken"];
        return mode == "subscribe" && !string.IsNullOrEmpty(expected) && token == expected && !string.IsNullOrEmpty(challenge)
            ? Content(challenge, "text/plain") : Unauthorized();
    }
    [HttpPost(Settings.WhatsAppRouteToUpdate)]
    [RequestSizeLimit(1024 * 1024)]
    public async Task<IActionResult> Update([FromBody] WhatsAppWebhookUpdate update)
    {
        if (update.Object != "whatsapp_business_account") return Content("EVENT_RECEIVED", "text/plain");
        try
        {
            foreach (var entry in update.Entries ?? Array.Empty<WhatsAppWebhookEntry>())
            {
                if (entry?.Id != options.BusinessAccountId) continue;
                foreach (var change in entry.Changes ?? Array.Empty<WhatsAppWebhookChange>())
                {
                    var value = change?.Value;
                    if (change?.Field != "messages" || value?.MessagingProduct != "whatsapp" ||
                        value.Metadata?.PhoneNumberId != options.PhoneNumberId) continue;
                    foreach (var message in value.Messages ?? Array.Empty<WhatsAppMessage>())
                    {
                        if (message == null) continue;
                        using var scope = scopes.CreateScope();
                        try
                        {
                            await scope.ServiceProvider.GetRequiredService<IBotCommandResolver<WhatsAppMessage>>().Handle(message);
                        }
                        catch (CommandExtractionException exception)
                        {
                            await scope.ServiceProvider.GetRequiredService<ISampleMessageSender>().Send(exception.ChatId,
                                SampleMessages.Text("Unknown command. Try /start, /ask or /profile."));
                        }
                    }
                }
            }
            return Content("EVENT_RECEIVED", "text/plain");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to handle WhatsApp update");
            return StatusCode(500);
        }
    }
}
