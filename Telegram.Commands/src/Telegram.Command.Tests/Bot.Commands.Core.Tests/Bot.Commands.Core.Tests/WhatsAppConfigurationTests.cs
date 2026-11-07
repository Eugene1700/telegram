using Bot.Commands.Abstract.Interfaces;
using global::WhatsApp.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SimpleHandlers;
using SimpleHandlers.Services;

namespace Bot.Commands.Core.Tests;

public class WhatsAppConfigurationTests
{
    [TestCase(false)]
    [TestCase(true)]
    public async Task SubscriptionIsOptInAndRunsAfterHttpStartup(bool enabled)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> {
            ["WhatsAppBotProfile:RegisterWebhook"] = enabled.ToString(), ["WhatsAppBotProfile:BaseUrl"] = "https://example.com"
        }).Build();
        var client = new Mock<IWhatsAppClient>(MockBehavior.Strict);
        client.Setup(c => c.SubscribeToMessagingAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var service = new ConfigureWhatsAppWebhook(client.Object, config, NullLogger<ConfigureWhatsAppWebhook>.Instance);
        await service.StartAsync(default);
        client.Verify(c => c.SubscribeToMessagingAsync(It.IsAny<CancellationToken>()), Times.Never);
        await service.StartedAsync(default);
        await service.StopAsync(default);
        client.Verify(c => c.SubscribeToMessagingAsync(It.IsAny<CancellationToken>()), enabled ? Times.Once() : Times.Never());
        client.VerifyNoOtherCalls();
    }

    internal static IConfiguration Profile(string name = "whatsapp_sample") => new ConfigurationBuilder()
        .AddConfiguration(VkWebhookTests.Config())
        .AddInMemoryCollection(new Dictionary<string, string> {
            ["WhatsAppBotProfile:Enabled"] = "true", ["WhatsAppBotProfile:BotName"] = name,
            ["WhatsAppBotProfile:PhoneNumberId"] = "99", ["WhatsAppBotProfile:BusinessAccountId"] = "88",
            ["WhatsAppBotProfile:Token"] = "dummy", ["WhatsAppBotProfile:ApiVersion"] = "v23.0",
            ["WhatsAppBotProfile:AppSecret"] = "secret", ["WhatsAppBotProfile:VerifyToken"] = "verify"
        }).Build();

    [Test]
    public void RouteSelectsProfileAndSenderAndDuplicateNamesAreRejected()
    {
        var services = new ServiceCollection();
        new Startup(Profile()).ConfigureServices(services);
        services.AddSingleton(Mock.Of<IWhatsAppClient>());
        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IHttpContextAccessor>().HttpContext = new DefaultHttpContext();
        provider.GetRequiredService<IHttpContextAccessor>().HttpContext!.Request.Path = "/" + Settings.WhatsAppRouteToUpdate;
        using var scope = provider.CreateScope();
        Assert.That(scope.ServiceProvider.GetRequiredService<ISampleMessageSender>(), Is.TypeOf<WhatsAppSampleMessageSender>());
        Assert.That(scope.ServiceProvider.GetRequiredService<IBotProfile>().BotName, Is.EqualTo("whatsapp_sample"));
        Assert.That(scope.ServiceProvider.GetRequiredService<IBotProfile>().Swarms, Does.Contain("whatsapp"));
        Assert.Throws<InvalidOperationException>(() => new Startup(Profile("TELEGRAM_SAMPLE")).ConfigureServices(new ServiceCollection()));
    }
}
