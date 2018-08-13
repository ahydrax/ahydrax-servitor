﻿using System;
using System.Threading;
using Akka.Actor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ahydrax.Servitor
{
    public static class Program
    {
        private static readonly AutoResetEvent ProgramExitEvent = new AutoResetEvent(false);
        private static ILogger _logger;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;

            var loggerFactory = new LoggerFactory()
                .AddConsole();

            _logger = loggerFactory.CreateLogger(nameof(Program));
            LoggingActor.DefaultLoggerFactory = loggerFactory;

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsecrets.json")
                .Build();

            var settings = new Settings
            {
                AllowedChatId = long.Parse(configuration[nameof(Settings.AllowedChatId)]),
                LarisId = long.Parse(configuration[nameof(Settings.LarisId)]),
                TelegramBotApiKey = configuration[nameof(Settings.TelegramBotApiKey)],
                TeamspeakHost = configuration[nameof(Settings.TeamspeakHost)],
                TeamspeakPort = int.Parse(configuration[nameof(Settings.TeamspeakPort)]),
                TeamspeakUsername = configuration[nameof(Settings.TeamspeakUsername)],
                TeamspeakPassword = configuration[nameof(Settings.TeamspeakPassword)]
            };
            _logger.LogInformation("Settings initialized");

            using (var system = ActorSystem.Create("ahydrax-servitor", AkkaConfig))
            {
                system.ActorOf(
                    Props.Create(() => new TelegramMessageChannel(settings)),
                    nameof(TelegramMessageChannel));

                system.ActorOf(
                    Props.Create(() => new TelegramMessageRouter(settings)),
                    nameof(TelegramMessageRouter));

                system.ActorOf(
                    Props.Create(() => new CatStatusResponder()),
                    nameof(CatStatusResponder));

                system.ActorOf(
                    Props.Create(() => new TeamspeakActor(settings)),
                    nameof(TeamspeakActor));

                ProgramExitEvent.WaitOne();
            }
        }

        private static void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            ProgramExitEvent.Set();
            e.Cancel = true;
        }

        private const string AkkaConfig = @"
akka {
    loggers = [""ahydrax.Servitor.LoggingActor, ahydrax-servitor""]
    loglevel = DEBUG
    log-config-on-start = on        
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
    }
}
