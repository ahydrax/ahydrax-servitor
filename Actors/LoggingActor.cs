using Akka.Actor;
using Akka.Event;
using Microsoft.Extensions.Logging;

namespace ahydrax.Servitor.Actors
{
    public sealed class LoggingActor : ReceiveActor
    {
        public static ILogger Logger { get; set; }
        
        public LoggingActor()
        {
            Receive<Debug>(x => Logger?.LogDebug(x.ToString()));
            Receive<Error>(x => Logger?.LogError(x.ToString()));
            Receive<Info>(x => Logger?.LogInformation(x.ToString()));
            Receive<Warning>(x => Logger?.LogWarning(x.ToString()));
            Receive<InitializeLogger>(m =>
            {
                Sender.Tell(new LoggerInitialized());
            });
        }
    }
}
