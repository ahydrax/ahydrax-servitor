﻿using Akka.Actor;

namespace ahydrax.Servitor
{
    internal static class ActorSelectionExtensions
    {
        public static ActorSelection SelectActor<T>(this ActorSystem system) where T : ReceiveActor
            => system.ActorSelection("user/" + typeof(T).Name);
    }
}
