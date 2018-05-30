using System.Linq;
using Akka.Actor;
using Akka.Event;
using Telegram.Bot.Types;

namespace ahydrax_servitor
{
    public class TelegramMessageRouter : ReceiveActor
    {
        private readonly Settings _settings;
        private readonly ILoggingAdapter _logger;

        public TelegramMessageRouter(Settings settings)
        {
            _settings = settings;
            _logger = Context.GetLogger();

            Receive<Message>(RouteMessage);
        }

        private bool RouteMessage(Message arg)
        {
            var message = arg;
            if (!AuthorizedUser(message)) return true;

            _logger.Info($"Received: {message.Text} from {message.From.Id}");
            var parameters = message.Text.Split(' ');
            var command = new string(parameters.First().TakeWhile(x => x != '@').ToArray());

            switch (command)
            {
                case "/whots":
                    Context.System.ActorSelection("user/" + nameof(TeamspeakActor)).Tell(new WhoIsInTeamspeak(arg.Chat.Id));
                    return true;
                    
                case "/chokot":
                    Context.System.ActorSelection("user/" + nameof(CatStatusResponder)).Tell(new TelegramMessage<string>(message.Chat.Id, ""));
                    return true;

                default:
                    return true;
            }
        }

        private bool AuthorizedUser(Message message)
            => message.Chat.Id == _settings.AllowedChatId ||
               message.Chat.Id == _settings.LarisId ||
               message.From.Username == "ahydrax";
    }
}
