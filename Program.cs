using System;
using System.Net;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ahydrax_servitor
{
    public static class Program
    {
        private static readonly ManualResetEvent ProgramExitEvent = new ManualResetEvent(false);
        private static ILogger _logger;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;

            var loggerFactory = new LoggerFactory()
                .AddConsole()
                .AddDebug();

            _logger = loggerFactory.CreateLogger("main");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsecrets.json")
                .Build();

            var settings = new BotSettings
            {
                AllowedChatId = long.Parse(configuration[nameof(BotSettings.AllowedChatId)]),
                TelegramBotApiKey = configuration[nameof(BotSettings.TelegramBotApiKey)],
                TeamspeakHost = configuration[nameof(BotSettings.TeamspeakHost)],
                TeamspeakUsername = configuration[nameof(BotSettings.TeamspeakUsername)],
                TeamspeakPassword = configuration[nameof(BotSettings.TeamspeakPassword)]
            };
            _logger.LogInformation("Settings initialized");

            var communicator = new Communicator();

            var telegramBot = new TelegramBot(communicator, settings, loggerFactory.CreateLogger<TelegramBot>());
            var teamspeakBot = new TeamspeakBot(communicator, settings, loggerFactory.CreateLogger<TeamspeakBot>());

            _logger.LogInformation("Starting bots...");
            communicator.StartBotsAndCommunication(telegramBot, teamspeakBot);
            _logger.LogInformation("Bots started");

            ProgramExitEvent.WaitOne();
        }

        private static void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            ProgramExitEvent.Set();
        }
    }
}
