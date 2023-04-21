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
using Telegram.Commands.Abstract.Interfaces;
using Telegram.Commands.DependencyInjection;
using IClock = Telegram.Commands.Abstract.Interfaces.IClock;

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
            services.AddControllers().AddNewtonsoftJson();;
            
            services.AddEntityStorage(builder =>
                builder
                    .UseSqlite($"Data Source=dev.db"));
            services.UseEFOnlyTranslator();
            
            services.AddScoped<SystemClockService>();
            services.AddScoped<IClock>(sp => sp.GetService<SystemClockService>());
            services.AddScoped<EntityStorage.IClock>(sp => sp.GetService<SystemClockService>());

            var botSettings = Configuration.GetSection("BotProfile");
            services.AddScoped<ITelegramBotProfile>(sp => new TelegramBotProfile
            {
                BaseUrl = botSettings["BaseUrl"],
                Key = botSettings["Token"],
                BotName = botSettings["BotName"],
            });

            services.AddScoped<IAuthProvider, AuthProvider>();
            services.AddScoped<ISessionsStore, SessionStore>();
            services.UseTelegramCommandsServices();
            services.AddHostedService<ConfigureWebhook>();

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