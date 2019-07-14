using System;
using ahydrax.Servitor.Actors;
using ahydrax.Servitor.Actors.Utility;
using ahydrax.Servitor.Recurring;
using ahydrax.Servitor.Services;
using Akka.Actor;
using Akka.DI.Core;
using LiteDB;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ahydrax.Servitor
{
    public class Startup : IStartup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;
        private IServiceProvider _serviceProvider;

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/login";
                    options.LogoutPath = "/logout";
                });

            services.AddMvc();

            var db = new LiteDatabase(@"data.db");
            services.AddSingleton(db);

            var settings = _configuration.Get<Settings>();
            services.AddSingleton(settings);

            var actorSystem = ActorSystem.Create("ahydrax-servitor");
            services.AddSingleton(actorSystem);

            services.AddHostedService<TeamspeakPulseService>();
            services.AddSingleton<GreetingService>();

            services.AddTransient<TelegramMessageChannel>();
            services.AddTransient<TelegramMessageRouter>();
            services.AddTransient<TeamspeakCredentialsHolder>();
            services.AddTransient<TeamspeakActor>();
            services.AddTransient<TelegramMyIdResponder>();
            services.AddTransient<HealthActor>();
            services.AddTransient<SelfieActor>();
            services.AddTransient<TempActor>();
            services.AddTransient<FailfastActor>();

            services.AddLogging();
            _serviceProvider = services.BuildServiceProvider(true);
            actorSystem.AddDependencyResolver(new MicrosoftDependencyResolver(_serviceProvider));

            return _serviceProvider;
        }

        public void Configure(IApplicationBuilder app)
        {
            var system = _serviceProvider.GetRequiredService<ActorSystem>();
            system.CreateActor<TelegramMessageChannel>();
            system.CreateActor<TelegramMessageRouter>();
            system.CreateActor<TeamspeakCredentialsHolder>();
            system.CreateActor<TeamspeakActor>();
            system.CreateActor<TelegramMyIdResponder>();
            system.CreateActor<HealthActor>();
            system.CreateActor<SelfieActor>();
            system.CreateActor<TempActor>();
            system.CreateActor<FailfastActor>();

            if (_environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseForwardedHeaders();
                app.UseExceptionHandler("/error/500");
            }

            app.UseAuthentication();
            app.UseCookiePolicy();
            app.UseStaticFiles();
            app.UseStatusCodePagesWithReExecute("/error/{0}");
            app.UseMvc();
        }
    }
}
