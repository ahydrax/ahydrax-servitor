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
                    x.AddConsole();
                    x.AddFile(options =>
                    {
                        options.LogDirectory = "logs";
                        options.FileName = "log-";
                        options.FileSizeLimit = 20 * 1024 * 1024;
                    });
                    x.AddFilter(level =>
                        level == LogLevel.Error ||
                        level == LogLevel.Critical ||
                        level == LogLevel.Warning)
                        .AddFile(options =>
                        {
                            options.LogDirectory = "logs";
                            options.FileName = "errors-";
                            options.FileSizeLimit = 20 * 1024 * 1024;
                        });
                })
                .UseStartup<Startup>()
                .UseKestrel(options => options.ListenLocalhost(8088));
    }
}
