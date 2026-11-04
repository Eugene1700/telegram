using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using global::Telegram.Bot;
using Bot.Commands.Abstract;
using Bot.Commands.Abstract.Interfaces;

namespace SimpleHandlers.Services
{
    public class ConfigureWebhook : IHostedService
    {
        private readonly ILogger<ConfigureWebhook> _logger;
        private readonly ITelegramBotClient _botClient;
        private readonly IConfiguration _configuration;

        public ConfigureWebhook(ILogger<ConfigureWebhook> logger,
            ITelegramBotClient botClient, IConfiguration configuration)
        {
            _logger = logger;
            _botClient = botClient;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_configuration.GetValue("BotProfile:RegisterWebhook", true))
                return;
            var botProfile = BotRuntime.ReadProfile(_configuration, false);
            
            _logger.LogInformation("Setting webhook: {webhookAddress}", botProfile.GetFullUrl());
            await _botClient.SetWebhook(
                url: botProfile.GetFullUrl(),
                allowedUpdates: [],
                cancellationToken: cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (!_configuration.GetValue("BotProfile:RegisterWebhook", true))
                return;
            
            _logger.LogInformation("Removing webhook");
            await _botClient.DeleteWebhook(cancellationToken: cancellationToken);
        }
    }
}
