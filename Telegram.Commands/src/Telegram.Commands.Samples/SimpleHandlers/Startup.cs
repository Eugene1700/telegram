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
            if ((telegramEnabled && string.IsNullOrWhiteSpace(Configuration["BotProfile:BotName"])) ||
                (maxEnabled && string.IsNullOrWhiteSpace(Configuration["MaxBotProfile:BotName"])))
                throw new System.InvalidOperationException("Every enabled bot must have a BotName for session isolation.");
            if (telegramEnabled && maxEnabled && string.Equals(Configuration["BotProfile:BotName"],
                Configuration["MaxBotProfile:BotName"], System.StringComparison.OrdinalIgnoreCase))
                throw new System.InvalidOperationException("Enabled bots must have distinct BotName values.");
            services.AddCors();
            services.AddControllers();
            services.AddHttpContextAccessor();
            services.AddScoped<BotRuntime>();
            services.AddScoped<WebhookAccessFilter>();
            services.AddScoped<TelegramSampleMessageSender>();
            services.AddScoped<MaxSampleMessageSender>();
            services.AddScoped<ISampleMessageSender>(sp => sp.GetRequiredService<BotRuntime>().IsMax
                ? sp.GetRequiredService<MaxSampleMessageSender>() : sp.GetRequiredService<TelegramSampleMessageSender>());
            var sessionsConnection = Configuration.GetConnectionString("Sessions") ?? "Data Source=dev2.db";
            services.AddEntityStorage(builder => builder.UseSqlite(sessionsConnection));
            services.UseEFOnlyTranslator();


            services.AddScoped<SystemClockService>();
            services.AddScoped<IClock>(sp => sp.GetService<SystemClockService>());
            services.AddScoped<EntityStorage.IClock>(sp => sp.GetService<SystemClockService>());

            services.AddScoped<IBotProfile>(sp => BotRuntime.ReadProfile(Configuration,
                sp.GetRequiredService<BotRuntime>().IsMax));

            services.AddScoped<IAuthProvider, AuthProvider>();
            services.AddScoped<ISessionsStore, SessionStore>();
            services.AddScoped<ICommandMatcher, ProfileFunnelMatcher>();
            services.AddSingleton<global::Telegram.Bot.ITelegramBotClient>(_ =>
                new global::Telegram.Bot.TelegramBotClient(Configuration["BotProfile:Token"]));
            services.UseTelegramCommandsServices();
            if (Configuration.GetValue("BotProfile:Enabled", true))
                services.AddHostedService<ConfigureWebhook>();
            if (Configuration.GetValue<bool>("MaxBotProfile:Enabled"))
            {
                services.UseMaxCommandsServices();
                services.AddSingleton(_ => new global::Max.Bot.MaxClient(Configuration["MaxBotProfile:Token"]));
                services.AddHostedService<ConfigureMaxWebhook>();
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
