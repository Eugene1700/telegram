using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Bot.Commands.Abstract.Interfaces;
using global::WhatsApp.Messaging;
using global::WhatsApp.Messaging.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SimpleHandlers.Controllers;
using SimpleHandlers.Services;

namespace Bot.Commands.Core.Tests;

public class WhatsAppWebhookTests
{
    internal static IConfiguration Config(bool enabled = true) => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string> {
            ["WhatsAppBotProfile:Enabled"] = enabled.ToString(),
            ["WhatsAppBotProfile:AppSecret"] = "secret", ["WhatsAppBotProfile:VerifyToken"] = "verify"
        }).Build();

    [TestCase(true, false, 0)]
    [TestCase(true, true, 401)]
    [TestCase(false, false, 404)]
    public async Task SignatureFilterChecksRawBodyBeforeBindingAndRewinds(bool enabled, bool tampered, int status)
    {
        var bytes = Encoding.UTF8.GetBytes("""{"object":"instagram","entry":[]}""");
        var signature = "sha256=" + Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes("secret"), bytes)).ToLowerInvariant();
        var http = new DefaultHttpContext();
        http.Request.Method = "POST";
        http.Request.Headers["X-Hub-Signature-256"] = signature;
        http.Request.Body = new MemoryStream(tampered ? bytes.Concat(new byte[] { 32 }).ToArray() : bytes);
        var action = new ActionContext(http, new Microsoft.AspNetCore.Routing.RouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
        var context = new ResourceExecutingContext(action, new List<IFilterMetadata>(),
            new List<Microsoft.AspNetCore.Mvc.ModelBinding.IValueProviderFactory>());
        var called = false;
        await new WhatsAppWebhookAccessFilter(Config(enabled)).OnResourceExecutionAsync(context, () =>
        {
            called = true;
            Assert.That(http.Request.Body.Position, Is.Zero);
            return Task.FromResult(new ResourceExecutedContext(action, new List<IFilterMetadata>()));
        });
        Assert.That(called, Is.EqualTo(status == 0));
        Assert.That((context.Result as StatusCodeResult)?.StatusCode ?? 0, Is.EqualTo(status));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("sha256=bad")]
    [TestCase("sha256=zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz")]
    public void MissingOrMalformedSignaturesFailClosed(string signature)
        => Assert.That(WhatsAppWebhookSignature.IsValid(Encoding.UTF8.GetBytes("{}"), signature, "secret"), Is.False);

    [Test]
    public async Task ControllerVerifiesChallengeFiltersAccountAndCreatesScopePerEvent()
    {
        var instances = new List<IBotCommandResolver<WhatsAppMessage>>();
        var senders = new List<string>();
        var services = new ServiceCollection();
        services.AddScoped<IBotCommandResolver<WhatsAppMessage>>(_ =>
        {
            var resolver = new Mock<IBotCommandResolver<WhatsAppMessage>>();
            resolver.Setup(r => r.Handle(It.IsAny<WhatsAppMessage>()))
                .Callback<WhatsAppMessage>(e => senders.Add(e.From!)).Returns(Task.CompletedTask);
            instances.Add(resolver.Object);
            return resolver.Object;
        });
        using var provider = services.BuildServiceProvider();
        var controller = new WhatsAppMessageController(Config(), new WhatsAppOptions { PhoneNumberId = "99", BusinessAccountId = "88" },
            provider.GetRequiredService<IServiceScopeFactory>(), NullLogger<WhatsAppMessageController>.Instance);
        Assert.That(((ContentResult)controller.Verify("subscribe", "verify", "123")).Content, Is.EqualTo("123"));
        Assert.That(controller.Verify("subscribe", "wrong", "123"), Is.TypeOf<UnauthorizedResult>());
        var update = JsonSerializer.Deserialize<WhatsAppWebhookUpdate>("""
            {"object":"whatsapp_business_account","entry":[
              {"id":"wrong","changes":[{"field":"messages","value":{"messaging_product":"whatsapp","metadata":{"phone_number_id":"99"},"messages":[{"from":"7","type":"text","text":{"body":"ignore"}}]}}]},
              {"id":"88","changes":[
                {"field":"messages","value":{"messaging_product":"whatsapp","metadata":{"phone_number_id":"99"},"messages":[
                  {"from":"2","type":"text","text":{"body":"/ask"}},
                  {"from":"3","type":"text","text":{"body":"/ask"}}]}},
                {"field":"messages","value":{"messaging_product":"whatsapp","metadata":{"phone_number_id":"wrong"},"messages":[{"from":"4"}]}},
                {"field":"messages","value":{"messaging_product":"whatsapp","metadata":{"phone_number_id":"99"},"statuses":[{"id":"out","status":"read"}]}}
              ]}]}
            """)!;
        Assert.That(((ContentResult)await controller.Update(update)).Content, Is.EqualTo("EVENT_RECEIVED"));
        Assert.That(senders, Is.EqualTo(new[] { "2", "3" }));
        Assert.That(instances.Count, Is.EqualTo(2));
        Assert.That(instances[0], Is.Not.SameAs(instances[1]));
    }
}
