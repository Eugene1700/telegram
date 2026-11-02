using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using global::Telegram.Bot;
using Bot.Commands.Abstract;
using Bot.Commands.Abstract.Interfaces;

namespace SimpleHandlers.Services
{
    public class ConfigureWebhook : IHostedService
    {
        private readonly ILogger<ConfigureWebhook> _logger;
        private readonly IServiceProvider _services;

        public ConfigureWebhook(ILogger<ConfigureWebhook> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _services = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
            var botProfile = scope.ServiceProvider.GetRequiredService<IBotProfile>();
            
            _logger.LogInformation("Setting webhook: {webhookAddress}", botProfile.GetFullUrl());
            await botClient.SetWebhook(
                url: botProfile.GetFullUrl(),
                allowedUpdates: [],
                cancellationToken: cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
            
            _logger.LogInformation("Removing webhook");
            await botClient.DeleteWebhook(cancellationToken: cancellationToken);
        }
    }
}