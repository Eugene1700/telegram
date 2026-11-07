using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace SimpleHandlers.Services;

// Resource filters run before action arguments (including SDK clients) are resolved.
public sealed class WebhookAccessFilter(IConfiguration configuration, BotRuntime runtime) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var max = runtime.IsMax;
        if (!configuration.GetValue(runtime.ProfileSection + ":Enabled", runtime.Platform == SamplePlatform.Telegram))
        {
            context.Result = new NotFoundResult();
            return;
        }
        if (runtime.Platform == SamplePlatform.Vk)
        {
            var request = context.HttpContext.Request;
            request.EnableBuffering();
            try
            {
                using var document = await JsonDocument.ParseAsync(request.Body);
                var body = document.RootElement;
                var expectedSecret = configuration["VkBotProfile:WebhookSecret"];
                if (body.ValueKind != JsonValueKind.Object ||
                    !body.TryGetProperty("secret", out var supplied) || supplied.ValueKind != JsonValueKind.String ||
                    string.IsNullOrEmpty(expectedSecret) || supplied.GetString() != expectedSecret ||
                    !body.TryGetProperty("group_id", out var group) || group.ValueKind != JsonValueKind.Number ||
                    !group.TryGetInt64(out var groupId) || groupId <= 0 ||
                    groupId != configuration.GetValue<long>("VkBotProfile:GroupId"))
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }
            }
            catch (JsonException)
            {
                context.Result = new BadRequestResult();
                return;
            }
            finally { request.Body.Position = 0; }
        }
        var secret = configuration["MaxBotProfile:WebhookSecret"];
        if (max && !string.IsNullOrEmpty(secret) && context.HttpContext.Request.Headers["X-Max-Bot-Api-Secret"] != secret)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        await next();
    }
}
