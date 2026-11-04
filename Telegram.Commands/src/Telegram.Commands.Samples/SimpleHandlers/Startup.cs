using EntityStorage;
using EntityStorage.DependencyInjection;
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
            services.AddCors();
            services.AddControllers();
            services.AddHttpContextAccessor();
            services.AddScoped<BotRuntime>();
            services.AddScoped<WebhookAccessFilter>();
            services.AddScoped<TelegramSampleMessageSender>();
            services.AddScoped<MaxSampleMessageSender>();
            services.AddScoped<ISampleMessageSender>(sp => sp.GetRequiredService<BotRuntime>().IsMax
                ? sp.GetRequiredService<MaxSampleMessageSender>() : sp.GetRequiredService<TelegramSampleMessageSender>());
            var sessionsConnection = Configuration.GetConnectionString("Sessions") ?? "Data Source=dev.db";
            services.AddEntityStorage(builder => builder.UseSqlite(sessionsConnection));
            services.UseEFOnlyTranslator();


            services.AddScoped<SystemClockService>();
            services.AddScoped<IClock>(sp => sp.GetService<SystemClockService>());
            services.AddScoped<EntityStorage.IClock>(sp => sp.GetService<SystemClockService>());

            services.AddScoped<IBotProfile>(sp => BotRuntime.ReadProfile(Configuration,
                sp.GetRequiredService<BotRuntime>().IsMax));

            services.AddScoped<IAuthProvider, AuthProvider>();
            services.AddScoped<ISessionsStore, SessionStore>();
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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
