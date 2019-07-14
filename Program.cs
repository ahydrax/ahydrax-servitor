using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace ahydrax.Servitor
{
    public static class Program
    {
        public static void Main(string[] args) =>
            CreateWebHostBuilder(args)
                .Build()
                .Run();

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseSerilog((context, configuration) =>
                {
                    if (Environment.UserInteractive)
                    {
                        configuration.MinimumLevel.Debug().WriteTo.ColoredConsole();
                    }

                    var logPathTemplate = Path.Join(context.HostingEnvironment.ContentRootPath, "logs", ".txt");

                    configuration.MinimumLevel.Information().WriteTo
                        .File(logPathTemplate, rollingInterval: RollingInterval.Day);
                })
                .UseStartup<Startup>()
                .UseKestrel((context, options) =>
                {
                    var settings = context.Configuration.Get<Settings>();
                    var listenAddress = settings.WebServer.IpAddress == "*"
                        ? IPAddress.Any
                        : IPAddress.Parse(settings.WebServer.IpAddress);
                    options.Listen(listenAddress, settings.WebServer.Port);
                });
    }
}
