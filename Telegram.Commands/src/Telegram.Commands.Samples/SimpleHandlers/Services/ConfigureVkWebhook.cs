using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using VkNet.Abstractions;
using VkNet.Utils;

namespace SimpleHandlers.Services;

/// <summary>Register only after the HTTP server starts, so VK can confirm the callback.</summary>
public sealed class ConfigureVkWebhook(IVkApi client, IConfiguration configuration) : IHostedLifecycleService
{
    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        if (!configuration.GetValue<bool>("VkBotProfile:RegisterWebhook")) return;
        var profile = BotRuntime.ReadProfile(configuration, SamplePlatform.Vk);
        var url = WebhookUrl.Build(profile.BaseUrl, profile.UpdateRoute);
        var group = configuration.GetValue<ulong>("VkBotProfile:GroupId");
        var secret = configuration["VkBotProfile:WebhookSecret"];
        var version = configuration["VkBotProfile:CallbackApiVersion"];
        if (string.IsNullOrWhiteSpace(version))
            throw new InvalidOperationException("VK automatic registration requires CallbackApiVersion.");
        // API confirmation differs from the code displayed by the VK dashboard.
        configuration["VkBotProfile:ConfirmationCode"] =
            await client.Groups.GetCallbackConfirmationCodeAsync(group, cancellationToken);
        var servers = await client.Groups.GetCallbackServersAsync(group, null, cancellationToken);
        var configuredId = configuration.GetValue<long>("VkBotProfile:CallbackServerId");
        var existing = configuredId > 0
            ? servers.SingleOrDefault(s => s.Id == configuredId)
            : servers.FirstOrDefault(s => s.Url?.ToString() == url);
        if (configuredId > 0 && existing == null)
            throw new InvalidOperationException("Configured VK CallbackServerId was not found in the community.");
        ulong id;
        if (existing != null)
        {
            id = checked((ulong)existing.Id);
            if (!await client.Groups.EditCallbackServerAsync(group, id, url, profile.BotName, secret, cancellationToken))
                throw new InvalidOperationException("VK callback server update failed.");
        }
        else id = checked((ulong)await client.Groups.AddCallbackServerAsync(group, url, profile.BotName, secret, cancellationToken));
        // VkNet's CallbackSettings does not expose message_event. Preserve that event through its native invoke API.
        var result = await client.CallAsync<bool>("groups.setCallbackSettings", new VkParameters
        {
            ["group_id"] = group.ToString(CultureInfo.InvariantCulture), ["server_id"] = id.ToString(CultureInfo.InvariantCulture),
            ["api_version"] = version,
            ["message_new"] = "1", ["message_event"] = "1"
        }, false, cancellationToken);
        if (!result) throw new InvalidOperationException("VK callback event subscription failed.");
    }

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
