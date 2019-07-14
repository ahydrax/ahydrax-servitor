using System;
using System.Threading.Tasks;
using ahydrax.Servitor.Actors.Utility;
using Akka.Actor;

namespace ahydrax.Servitor.Actors
{
    public class RestartingActor : ReceiveActor
    {
        public RestartingActor()
        {
            ReceiveAsync<MessageArgs>(Restart);
        }

        private async Task Restart(MessageArgs arg)
        {
            try
            {
                var actor = await Context.System.Actor<TeamspeakActor>().ResolveOne(TimeSpan.Zero);
                Context.System.Actor<TelegramMessageChannel>().Tell(new MessageArgs<string>(arg.ChatId, "[RESTART_ACTOR] Teamspeak actor restart requested"));
                var stopped = await actor.GracefulStop(TimeSpan.FromSeconds(5));

                if (!stopped)
                {
                    await actor.Ask(Kill.Instance);
                }

                Context.System.Actor<TelegramMessageChannel>().Tell(new MessageArgs<string>(arg.ChatId, "[RESTART_ACTOR] Actor stopped"));

                Context.System.CreateActor<TeamspeakActor>();

                Context.System.Actor<TelegramMessageChannel>().Tell(new MessageArgs<string>(arg.ChatId, "[RESTART_ACTOR] Actor restared"));
            }
            catch (ActorNotFoundException)
            {
                Context.System.CreateActor<TeamspeakActor>();
            }
        }
    }
}
