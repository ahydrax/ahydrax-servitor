using ahydrax.Servitor.Actors.Utility;
using ahydrax.Servitor.Extensions;
using Akka.Actor;

namespace ahydrax.Servitor.Actors
{
    public class TeamspeakCredentialsHolder : ReceiveActor
    {
        private readonly Settings _settings;

        public TeamspeakCredentialsHolder(Settings settings)
        {
            _settings = settings;
            Receive<MessageArgs>(RespondTeamspeakCredentials);
        }

        private bool RespondTeamspeakCredentials(MessageArgs arg)
        {
            var message = $"Host: {_settings.Teamspeak.Host}\r\nPassword: {_settings.Teamspeak.Password}";
            Context.System.Actor<TelegramMessageChannel>().Tell(new MessageArgs<string>(arg.ChatId, message));
            return true;
        }
    }
}
