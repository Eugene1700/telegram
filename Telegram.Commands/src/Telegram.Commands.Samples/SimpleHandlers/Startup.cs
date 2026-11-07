using System;
using System.Linq;
using EntityStorage;
using EntityStorage.DependencyInjection;
using EntityStorage.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleHandlers.Domain;
using SimpleHandlers.Services;
using Bot.Commands.Abstract.Interfaces;
using Bot.Commands.DependencyInjection;
using IClock = Bot.Commands.Abstract.Interfaces.IClock;

namespace SimpleHandlers
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var telegramEnabled = Configuration.GetValue("BotProfile:Enabled", true);
            var maxEnabled = Configuration.GetValue<bool>("MaxBotProfile:Enabled");
            var vkEnabled = Configuration.GetValue<bool>("VkBotProfile:Enabled");
            var instagramEnabled = Configuration.GetValue<bool>("InstagramBotProfile:Enabled");
            var whatsappEnabled = Configuration.GetValue<bool>("WhatsAppBotProfile:Enabled");
            var names = new[] { "BotProfile", "MaxBotProfile", "VkBotProfile", "InstagramBotProfile", "WhatsAppBotProfile" }
                .Where(section => Configuration.GetValue(section + ":Enabled", section == "BotProfile"))
                .Select(section => Configuration[section + ":BotName"]).ToArray();
            if (names.Any(string.IsNullOrWhiteSpace))
                throw new System.InvalidOperationException("Every enabled bot must have a BotName for session isolation.");
            if (names.Distinct(StringComparer.OrdinalIgnoreCase).Count() != names.Length)
                throw new System.InvalidOperationException("Enabled bots must have distinct BotName values.");
            if (vkEnabled && (string.IsNullOrWhiteSpace(Configuration["VkBotProfile:Token"]) ||
                Configuration.GetValue<long>("VkBotProfile:GroupId") <= 0 ||
                string.IsNullOrWhiteSpace(Configuration["VkBotProfile:WebhookSecret"]) ||
                (!Configuration.GetValue<bool>("VkBotProfile:RegisterWebhook") &&
                 string.IsNullOrWhiteSpace(Configuration["VkBotProfile:ConfirmationCode"]))))
                throw new InvalidOperationException("VK requires Token, GroupId, WebhookSecret and ConfirmationCode.");
            services.AddCors();
            services.AddControllers();
            services.AddHttpContextAccessor();
            services.AddScoped<BotRuntime>();
            services.AddScoped<WebhookAccessFilter>();
            services.AddScoped<InstagramWebhookAccessFilter>();
            services.AddScoped<WhatsAppWebhookAccessFilter>();
            services.AddScoped<WhatsAppSampleMessageSender>();
            services.AddScoped<TelegramSampleMessageSender>();
            services.AddScoped<MaxSampleMessageSender>();
            services.AddHttpClient<VkSampleMessageSender>();
            services.AddScoped<InstagramSampleMessageSender>();
            services.AddScoped<ISampleMessageSender>(sp => sp.GetRequiredService<BotRuntime>().Platform switch
            {
                SamplePlatform.WhatsApp => sp.GetRequiredService<WhatsAppSampleMessageSender>(),
                SamplePlatform.Instagram => sp.GetRequiredService<InstagramSampleMessageSender>(),
                SamplePlatform.Vk => sp.GetRequiredService<VkSampleMessageSender>(),
                SamplePlatform.Max => sp.GetRequiredService<MaxSampleMessageSender>(),
                _ => sp.GetRequiredService<TelegramSampleMessageSender>()
            });
            var sessionsConnection = Configuration.GetConnectionString("Sessions") ?? "Data Source=dev2.db";
            services.AddEntityStorage(builder => builder.UseSqlite(sessionsConnection));
            services.UseEFOnlyTranslator();


            services.AddScoped<SystemClockService>();
            services.AddScoped<IClock>(sp => sp.GetService<SystemClockService>());
            services.AddScoped<EntityStorage.IClock>(sp => sp.GetService<SystemClockService>());

            services.AddScoped<IBotProfile>(sp => BotRuntime.ReadProfile(Configuration,
                sp.GetRequiredService<BotRuntime>().Platform));

            services.AddScoped<IAuthProvider, AuthProvider>();
            services.AddScoped<ISessionsStore, SessionStore>();
            services.AddScoped<ICommandMatcher<global::Telegram.Bot.Types.Message>, ProfileFunnelMatcher<global::Telegram.Bot.Types.Message>>();
            services.AddScoped<ICommandMatcher<global::Max.Bot.Types.Message>, ProfileFunnelMatcher<global::Max.Bot.Types.Message>>();
            services.AddScoped<ICommandMatcher<VkNet.Model.Message>, ProfileFunnelMatcher<VkNet.Model.Message>>();
            services.AddScoped<ICommandMatcher<Instagram.Messaging.Models.InstagramMessage>,
                ProfileFunnelMatcher<Instagram.Messaging.Models.InstagramMessage>>();
            services.AddScoped<ICommandMatcher<WhatsApp.Messaging.Models.WhatsAppMessage>,
                ProfileFunnelMatcher<WhatsApp.Messaging.Models.WhatsAppMessage>>();
            services.AddSingleton<global::Telegram.Bot.ITelegramBotClient>(_ =>
                new global::Telegram.Bot.TelegramBotClient(Configuration["BotProfile:Token"]));
            services.UseTelegramCommandsServices();
            if (telegramEnabled)
                services.AddHostedService<ConfigureWebhook>();
            if (maxEnabled)
            {
                services.UseMaxCommandsServices();
                services.AddSingleton(_ => new global::Max.Bot.MaxClient(Configuration["MaxBotProfile:Token"]));
                services.AddHostedService<ConfigureMaxWebhook>();
            }
            if (vkEnabled)
            {
                services.AddHostedService<ConfigureVkWebhook>();
                services.UseVkCommandsServices();
                services.AddSingleton<VkNet.Abstractions.IVkApi>(_ =>
                {
                    var client = new VkNet.VkApi();
                    client.Authorize(new VkNet.Model.ApiAuthParams { AccessToken = Configuration["VkBotProfile:Token"] });
                    return client;
                });
            }
            if (whatsappEnabled)
            {
                var options = new WhatsApp.Messaging.WhatsAppOptions
                {
                    PhoneNumberId = Configuration["WhatsAppBotProfile:PhoneNumberId"],
                    BusinessAccountId = Configuration["WhatsAppBotProfile:BusinessAccountId"],
                    AccessToken = Configuration["WhatsAppBotProfile:Token"],
                    ApiVersion = Configuration["WhatsAppBotProfile:ApiVersion"]
                };
                options.Validate();
                WhatsApp.Messaging.WhatsAppOptions.ValidateId(options.BusinessAccountId);
                if (string.IsNullOrWhiteSpace(Configuration["WhatsAppBotProfile:AppSecret"]) ||
                    string.IsNullOrWhiteSpace(Configuration["WhatsAppBotProfile:VerifyToken"]))
                    throw new InvalidOperationException("WhatsApp requires AppSecret and VerifyToken.");
                if (Configuration.GetValue<bool>("WhatsAppBotProfile:RegisterWebhook"))
                    WebhookUrl.Build(Configuration["WhatsAppBotProfile:BaseUrl"], Settings.WhatsAppRouteToUpdate);
                services.AddSingleton(options);
                services.UseWhatsAppCommandsServices();
                services.AddHttpClient<WhatsApp.Messaging.IWhatsAppClient, WhatsApp.Messaging.WhatsAppClient>();
                services.AddHostedService<ConfigureWhatsAppWebhook>();
            }
            if (instagramEnabled)
            {
                var instagramOptions = new Instagram.Messaging.InstagramOptions
                {
                    AccountId = Configuration["InstagramBotProfile:AccountId"],
                    AccessToken = Configuration["InstagramBotProfile:Token"],
                    ApiVersion = Configuration["InstagramBotProfile:ApiVersion"]
                };
                instagramOptions.Validate();
                Bot.Commands.Instagram.InstagramIdentity.ToSessionId(instagramOptions.AccountId);
                if (string.IsNullOrWhiteSpace(Configuration["InstagramBotProfile:AppSecret"]) ||
                    string.IsNullOrWhiteSpace(Configuration["InstagramBotProfile:VerifyToken"]))
                    throw new InvalidOperationException("Instagram requires AppSecret and VerifyToken.");
                services.AddSingleton(instagramOptions);
                services.UseInstagramCommandsServices();
                services.AddHttpClient<Instagram.Messaging.IInstagramClient, Instagram.Messaging.InstagramClient>();
                services.AddHostedService<ConfigureInstagramWebhook>();
            }

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, EfDbContext dbContext)
        {
            // This sample starts with an empty database on every run.
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
