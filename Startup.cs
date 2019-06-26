using ahydrax.Servitor.Actors;
using Akka.Actor;
using LiteDB;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ahydrax.Servitor
{
    public class Startup
    {
        private const string AkkaConfig = @"
akka {
    loggers = [""ahydrax.Servitor.Actors.LoggingActor, ahydrax-servitor""]
    loglevel = DEBUG
    actor { 
        debug {  
              receive = on 
              autoreceive = on
              lifecycle = on
              event-stream = on
              unhandled = on
        }
}
";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/login";
                    options.LogoutPath = "/logout";
                });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSingleton(Configuration.Get<Settings>());

            var db = new LiteDatabase(@"data.db");
            var settings = Configuration.Get<Settings>();
            var actorSystem = ActorSystem.Create("ahydrax-servitor", AkkaConfig);

            actorSystem.ActorOf(
                Props.Create(() => new TelegramMessageChannel(settings)),
                nameof(TelegramMessageChannel));

            actorSystem.ActorOf(
                Props.Create(() => new TelegramMessageRouter(settings, db)),
                nameof(TelegramMessageRouter));

            actorSystem.ActorOf(
                Props.Create(() => new TeamspeakCredentialsHolder(settings)),
                nameof(TeamspeakCredentialsHolder));

            actorSystem.ActorOf(
                Props.Create(() => new TeamspeakActor(settings, db)),
                nameof(TeamspeakActor));

            actorSystem.ActorOf(
                Props.Create(() => new TelegramMyIdResponder()),
                nameof(TelegramMyIdResponder));

            actorSystem.ActorOf(
                Props.Create(() => new RestartingActor(settings, db)),
                nameof(RestartingActor));

            services.AddSingleton(actorSystem);
            services.AddSingleton(db);
            services.AddLogging();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            // Incredibly stupid akka api can't do DI.
            LoggingActor.Logger = loggerFactory.CreateLogger("akka");

            if (env.IsDevelopment())
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
