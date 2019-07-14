using System;
using System.Threading.Tasks;
using ahydrax.Servitor.Actors.Utility;
using Akka.Actor;

namespace ahydrax.Servitor.Actors
{
    public class FailfastActor : ReceiveActor
    {
        public FailfastActor()
        {
            ReceiveAsync<MessageArgs>(Failfast);
        }

        private async Task Failfast(MessageArgs arg)
        {
            Context.System.Actor<TelegramMessageChannel>()
                .Tell(new MessageArgs<string>(arg.ChatId, "[FAILFAST_ACTOR] Initiated..."));
            await Task.Delay(5000);
            Environment.FailFast($"Initiated by {arg.ChatId}");
        }
    }
}
