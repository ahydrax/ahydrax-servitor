using Akka.Actor;
using Akka.Event;
using Microsoft.Extensions.Logging;

namespace ahydrax.Servitor.Actors
{
    public sealed class LoggingActor : ReceiveActor
    {

        public LoggingActor(ILogger logger)
        {
            Receive<Debug>(x => logger.LogDebug(x.ToString()));
            Receive<Error>(x => logger.LogError(x.ToString()));
            Receive<Info>(x => logger.LogInformation(x.ToString()));
            Receive<Warning>(x => logger.LogWarning(x.ToString()));
            Receive<InitializeLogger>(m =>
            {
                Sender.Tell(new LoggerInitialized());
            });
        }
    }
}
