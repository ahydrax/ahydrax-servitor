using System.Linq;
using ahydrax.Servitor.Models;
using Akka.Actor;
using Akka.Event;
using LiteDB;
using Telegram.Bot.Types;

namespace ahydrax.Servitor.Actors
{
    public class TelegramMessageRouter : ReceiveActor
    {
        private readonly Settings _settings;
        private readonly ILoggingAdapter _logger;
        private readonly LiteCollection<AuthorizedUser> _authorizedUsersCollection;

        public TelegramMessageRouter(Settings settings, LiteDatabase db)
        {
            _settings = settings;
            _logger = Context.GetLogger();
            _authorizedUsersCollection = db.GetCollection<AuthorizedUser>();

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
                    Context.System.SelectActor<TeamspeakActor>().Tell(new MessageArgs(arg.Chat.Id));
                    return true;

                case "/chokot":
                    Context.System.SelectActor<CatStatusResponder>().Tell(new MessageArgs(arg.Chat.Id));
                    return true;

                default:
                    return true;
            }
        }

        private bool AuthorizedUser(Message message)
        {
            if (message.Chat.Id == _settings.TelegramHostGroupId) return true;

            return _authorizedUsersCollection.Exists(x => x.Id == message.Chat.Id);
        }
    }
}
