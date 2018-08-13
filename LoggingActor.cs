using Akka.Actor;
using Akka.Event;
using Microsoft.Extensions.Logging;

namespace ahydrax.Servitor
{
    public sealed class LoggingActor : ReceiveActor
    {
        public static ILoggerFactory DefaultLoggerFactory { get; set; }

        public LoggingActor()
        {
            var logger = DefaultLoggerFactory.CreateLogger("akka");

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
