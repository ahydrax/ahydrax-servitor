using Akka.Actor;
using Akka.DI.Core;

namespace ahydrax.Servitor.Actors.Utility
{
    public static class ActorSystemExtensions
    {
        public static void CreateActor<T>(this ActorSystem system)
            where T : ActorBase
            => system.ActorOf(system.DI().Props<T>(), typeof(T).Name);
    }
}