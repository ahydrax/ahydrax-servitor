using Akka.Actor;

namespace ahydrax.Servitor.Actors.Utility
{
    internal static class ActorSelectionExtensions
    {
        public static ActorSelection Actor<T>(this ActorSystem system) where T : ReceiveActor
            => system.ActorSelection("user/" + typeof(T).Name);
    }
}
