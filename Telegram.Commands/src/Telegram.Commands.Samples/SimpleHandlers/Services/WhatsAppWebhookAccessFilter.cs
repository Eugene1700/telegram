using System.IO;
using System.Threading.Tasks;
using WhatsApp.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace SimpleHandlers.Services;

public sealed class WhatsAppWebhookAccessFilter(IConfiguration configuration) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        if (!configuration.GetValue<bool>("WhatsAppBotProfile:Enabled"))
        {
            context.Result = new NotFoundResult();
            return;
        }
        var request = context.HttpContext.Request;
        if (HttpMethods.IsPost(request.Method))
        {
            request.EnableBuffering();
            using var buffer = new MemoryStream();
            try
            {
                await request.Body.CopyToAsync(buffer, context.HttpContext.RequestAborted);
                if (!WhatsAppWebhookSignature.IsValid(buffer.ToArray(), request.Headers["X-Hub-Signature-256"].ToString(),
                    configuration["WhatsAppBotProfile:AppSecret"]))
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }
            }
            finally { request.Body.Position = 0; }
        }
        await next();
    }
}
