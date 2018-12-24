using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
                .ConfigureAppConfiguration(x => x.AddJsonFile("appsecrets.json"))
                .ConfigureLogging(x =>
                {
                    x.SetMinimumLevel(LogLevel.Trace).AddConsole();
                    x.SetMinimumLevel(LogLevel.Debug).AddFile(options =>
                    {
                        options.LogDirectory = "logs";
                        options.FileName = "log-";
                        options.FileSizeLimit = 20 * 1024 * 1024;
                    });
                    x.SetMinimumLevel(LogLevel.Warning)
                        .AddFile(options =>
                        {
                            options.LogDirectory = "logs";
                            options.FileName = "errors-";
                            options.FileSizeLimit = 20 * 1024 * 1024;
                        });
                })
                .UseStartup<Startup>()
                .UseKestrel((context, options) =>
                {
                    var settings = context.Configuration.Get<Settings>();
                    var listenAddress = settings.BindAddress == "*" ? IPAddress.Any : IPAddress.Parse(settings.BindAddress);
                    options.Listen(listenAddress, settings.BindPort);
                });
    }
}
