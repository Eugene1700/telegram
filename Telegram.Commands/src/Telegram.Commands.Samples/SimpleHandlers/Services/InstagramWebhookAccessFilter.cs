using System.IO;
using System.Threading.Tasks;
using Instagram.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace SimpleHandlers.Services;

public sealed class InstagramWebhookAccessFilter(IConfiguration configuration) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        if (!configuration.GetValue<bool>("InstagramBotProfile:Enabled"))
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
                if (!InstagramWebhookSignature.IsValid(buffer.ToArray(), request.Headers["X-Hub-Signature-256"].ToString(),
                    configuration["InstagramBotProfile:AppSecret"]))
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
