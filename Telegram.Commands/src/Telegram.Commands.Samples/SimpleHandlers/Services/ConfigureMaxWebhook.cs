using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using global::Max.Bot;
using global::Max.Bot.Types.Requests;

namespace SimpleHandlers.Services;

public sealed class ConfigureMaxWebhook(MaxClient client, IConfiguration configuration) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!configuration.GetValue("MaxBotProfile:RegisterWebhook", true))
            return Task.CompletedTask;
        var profile = BotRuntime.ReadProfile(configuration, true);
        var url = new Uri(new Uri(profile.BaseUrl.TrimEnd('/') + "/"), profile.UpdateRoute).AbsoluteUri;
        return client.Subscriptions.SetWebhookAsync(new SetWebhookRequest
        {
            Url = url, Secret = configuration["MaxBotProfile:WebhookSecret"],
            UpdateTypes = new() { "message_created", "message_callback", "bot_started" }
        }, cancellationToken);
    }

    // Keep the subscription on restart; deleting it can interrupt another instance.
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
