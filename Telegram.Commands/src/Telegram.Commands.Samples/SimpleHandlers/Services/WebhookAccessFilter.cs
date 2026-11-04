using System.Threading.Tasks;
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
        if (!configuration.GetValue(max ? "MaxBotProfile:Enabled" : "BotProfile:Enabled", !max))
        {
            context.Result = new NotFoundResult();
            return;
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
