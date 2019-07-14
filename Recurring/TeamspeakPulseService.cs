using System;
using System.Threading;
using System.Threading.Tasks;
using ahydrax.Servitor.Actors;
using ahydrax.Servitor.Actors.Utility;
using Akka.Actor;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ahydrax.Servitor.Recurring
{
    internal sealed class TeamspeakPulseService : BackgroundService
    {
        private readonly ActorSystem _actorSystem;
        private readonly ILogger<TeamspeakPulseService> _logger;

        public TeamspeakPulseService(ActorSystem actorSystem, ILogger<TeamspeakPulseService> logger)
        {
            _actorSystem = actorSystem;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _actorSystem.Actor<TeamspeakActor>().Tell(new Pulse());
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e, "Error occured during pulsing TS server");
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
