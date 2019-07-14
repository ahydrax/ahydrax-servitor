using System.Linq;
using ahydrax.Servitor.Actors.Utility;
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

            _logger.Info($"Received: {message.Text} from {message.From.Id}");
            var parameters = message.Text.Split(' ');
            var command = new string(parameters.First().TakeWhile(x => x != '@').ToArray());

            switch (command)
            {
                case "/whots":
                    if (!AuthorizedUser(message)) return true;
                    Context.System.Actor<TeamspeakActor>().Tell(new MessageArgs(arg.Chat.Id));
                    return true;

                case "/teamspeak":
                    if (!AuthorizedUser(message)) return true;
                    Context.System.Actor<TeamspeakCredentialsHolder>().Tell(new MessageArgs(arg.Chat.Id));
                    return true;

                case "/restart":
                    if (!AuthorizedUser(message)) return true;
                    Context.System.Actor<RestartingActor>().Tell(new MessageArgs(arg.Chat.Id));
                    return true;

                case "/health":
                    if (!AuthorizedUser(message)) return true;
                    Context.System.Actor<HealthActor>().Tell(new MessageArgs(arg.Chat.Id));
                    return true;

                case "/temp":
                    if (!AuthorizedUser(message)) return true;
                    Context.System.Actor<TempActor>().Tell(new MessageArgs(arg.Chat.Id));
                    return true;

                case "/selfie":
                    if (!AuthorizedUser(message)) return true;
                    Context.System.Actor<SelfieActor>().Tell(new MessageArgs(arg.Chat.Id));
                    return true;

                case "/failfast":
                    if (!AuthorizedUser(message)) return true;
                    Context.System.Actor<FailfastActor>().Tell(new MessageArgs(arg.Chat.Id));
                    return true;

                case "/chatid":
                    Context.System.Actor<TelegramMyIdResponder>().Tell(new MessageArgs(arg.Chat.Id));
                    return true;

                default:
                    return true;
            }
        }

        private bool AuthorizedUser(Message message)
        {
            if (message.Chat.Id == _settings.Telegram.HostGroupId) return true;
            return _authorizedUsersCollection.Exists(x => x.Id == message.Chat.Id);
        }
    }
}
