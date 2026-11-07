using System.Threading;
using System.Threading.Tasks;
using WhatsApp.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SimpleHandlers.Services;

/// <summary>The app callback URL/verify token must first be configured in Meta's dashboard.</summary>
public sealed class ConfigureWhatsAppWebhook(IWhatsAppClient client, IConfiguration configuration,
    ILogger<ConfigureWhatsAppWebhook> logger) : IHostedLifecycleService
{
    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        if (!configuration.GetValue<bool>("WhatsAppBotProfile:RegisterWebhook")) return;
        var profile = BotRuntime.ReadProfile(configuration, SamplePlatform.WhatsApp);
        var url = WebhookUrl.Build(profile.BaseUrl, profile.UpdateRoute);
        await client.SubscribeToMessagingAsync(cancellationToken);
        logger.LogInformation("WhatsApp account subscribed. Configure the app callback URL {CallbackUrl} and VerifyToken in Meta's dashboard.", url);
    }
    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
