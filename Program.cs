using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using DryIoc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace ahydrax_servitor
{
    public static class Program
    {
        private static readonly AutoResetEvent ProgramExitEvent = new AutoResetEvent(false);
        private static ILogger _logger;
        private static TelegramBot _telegramActor;
        private static TeamspeakBot _teamspeakBot;

        static void Main(string[] args)
        {
            var container = SetupContainer();

            Console.CancelKeyPress += ConsoleOnCancelKeyPress;

            var loggerFactory = new LoggerFactory()
                .AddConsole()
#if DEBUG
                .AddDebug()
#endif
                ;

            _logger = loggerFactory.CreateLogger("main");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsecrets.json")
                .Build();

            var settings = new BotSettings
            {
                AllowedChatId = long.Parse(configuration[nameof(BotSettings.AllowedChatId)]),
                LarisId = long.Parse(configuration[nameof(BotSettings.LarisId)]),
                TelegramBotApiKey = configuration[nameof(BotSettings.TelegramBotApiKey)],
                TeamspeakHost = configuration[nameof(BotSettings.TeamspeakHost)],
                TeamspeakPort = int.Parse(configuration[nameof(BotSettings.TeamspeakPort)]),
                TeamspeakUsername = configuration[nameof(BotSettings.TeamspeakUsername)],
                TeamspeakPassword = configuration[nameof(BotSettings.TeamspeakPassword)]
            };
            _logger.LogInformation("Settings initialized");

            var communicator = new Communicator();

            _telegramActor = new TelegramBot(communicator, settings, loggerFactory.CreateLogger<TelegramBot>());
            _teamspeakBot = new TeamspeakBot(communicator, settings, loggerFactory.CreateLogger<TeamspeakBot>());

            _logger.LogInformation("Starting bots...");
            Task.Run(() =>
            {
                communicator.StartBotsAndCommunication(_telegramActor, _teamspeakBot);
                _logger.LogInformation("Bots started");
            });

            using (var system = ActorSystem.Create("ahydrax-servitor"))
            {
                var a = system.ActorOf<TelegramActor>();
                a.Tell(new TelegramStart { ApiKey = settings.TelegramBotApiKey });


                ProgramExitEvent.WaitOne();
            }

        }

        private static Container SetupContainer()
        {
            var c = new Container();


            return c;
        }

        private static void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            _telegramActor.Stop();
            ProgramExitEvent.Set();
            e.Cancel = true;
        }
    }


    internal class TelegramStop
    {
    }

    internal class TelegramStart
    {
        public string ApiKey { get; set; }
    }
}
