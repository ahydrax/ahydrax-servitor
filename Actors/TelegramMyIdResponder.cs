using ahydrax.Servitor.Extensions;
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
            Context.System.SelectActor<TelegramMessageChannel>().Tell(new MessageArgs<string>(obj.ChatId, obj.ChatId.ToString()));
            return true;
        }
    }
}
