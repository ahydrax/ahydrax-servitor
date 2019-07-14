using ahydrax.Servitor.Actors.Utility;
using Akka.Actor;

namespace ahydrax.Servitor.Actors
{
    public class TelegramMyIdResponder : ReceiveActor
    {
        public TelegramMyIdResponder()
        {
            Receive<MessageArgs>(Respond);
        }

        private bool Respond(MessageArgs obj)
        {
            Context.System.Actor<TelegramMessageChannel>().Tell(new MessageArgs<string>(obj.ChatId, obj.ChatId.ToString()));
            return true;
        }
    }
}
