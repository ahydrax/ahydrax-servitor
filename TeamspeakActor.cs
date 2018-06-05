using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using TeamSpeak3QueryApi.Net.Specialized;
using TeamSpeak3QueryApi.Net.Specialized.Notifications;

namespace ahydrax_servitor
{
    public class TeamspeakActor : ReceiveActor
    {
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly ConcurrentDictionary<int, string> _nicknames;
        private readonly Settings _settings;
        private readonly ActorSystem _system;
        private readonly ILoggingAdapter _logger;
        private readonly TeamSpeakClient _teamSpeakClient;
        private Timer _timer;
        private bool _connected;

        public TeamspeakActor(Settings settings)
        {
            _nicknames = new ConcurrentDictionary<int, string>();
            _settings = settings;
            _system = Context.System;
            _logger = Context.GetLogger();

            _teamSpeakClient = new TeamSpeakClient(_settings.TeamspeakHost, _settings.TeamspeakPort);

            _system = Context.System;
            ReceiveAsync<MessageArgs>(RespondWhoIsInTeamspeak);
        }

        protected override void PreStart() => InternalStart().GetAwaiter().GetResult();

        private async Task InternalStart()
        {
            _logger.Info("Starting teamspeak client...");
            await _teamSpeakClient.Connect();
            await _teamSpeakClient.Login(_settings.TeamspeakUsername, _settings.TeamspeakPassword);
            _logger.Info("Teamspeak bot connected");

            await _teamSpeakClient.UseServer(1);
            _logger.Info("Server changed");

            var me = await _teamSpeakClient.WhoAmI();
            _logger.Info($"Connected using username {me.NickName}");

            var clients = await _teamSpeakClient.GetClients();
            foreach (var client in clients)
            {
                _nicknames.AddOrUpdate(client.Id, client.NickName, (i, s) => client.NickName);
            }

            await _teamSpeakClient.RegisterServerNotification();

            _teamSpeakClient.Subscribe<ClientEnterView>(UserEntered);
            _teamSpeakClient.Subscribe<ClientLeftView>(UserLeft);

            _connected = true;
            _timer = new Timer(KeepAlive, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        protected override void PostStop()
        {
            _connected = false;
            _timer?.Dispose();
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                maxNrOfRetries: 25,
                withinTimeRange: TimeSpan.FromSeconds(30),
                localOnlyDecider: ex => Directive.Restart
                );
        }

        private async Task RespondWhoIsInTeamspeak(MessageArgs arg)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                var clients = await _teamSpeakClient.GetClients();
                var clientNicknames = clients
                    .Where(x => x.Type == ClientType.FullClient)
                    .Select(x => x.NickName)
                    .OrderBy(x => x)
                    .ToArray();

                var message = clientNicknames.Length == 0 ? "в тс пусто." : string.Join("\r\n", clientNicknames);

                GetTelegramActor().Tell(new MessageArgs<string>(arg.ChatId, message));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error occured during querying teamspeak server");
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private async void KeepAlive(object state)
        {
            if (!_connected) return;

            await _semaphoreSlim.WaitAsync();
            try
            {
                var me = await _teamSpeakClient.WhoAmI();
                _logger.Info($"ping {me.VirtualServerStatus}");
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private void UserLeft(IReadOnlyCollection<ClientLeftView> views)
        {
            _logger.Info("user left");
            foreach (var clientLeftView in views)
            {
                var nickname = _nicknames?[clientLeftView.Id] ?? "хз кто";
                GetTelegramActor().Tell(new MessageArgs<string>(_settings.AllowedChatId, $"{nickname} свалил из тс."));
            }
        }

        private void UserEntered(IReadOnlyCollection<ClientEnterView> collection)
        {
            _logger.Info("user entered");
            foreach (var clientEnterView in collection)
            {
                var nickname = clientEnterView.NickName;
                GetTelegramActor().Tell(new MessageArgs<string>(_settings.AllowedChatId, FindAppropriateGreeting(nickname)));
                _nicknames.AddOrUpdate(clientEnterView.Id, nickname, (i, s) => clientEnterView.NickName);
            }
        }

        private ActorSelection GetTelegramActor() => _system.ActorSelection("user/" + nameof(TelegramMessageChannel));

        private static readonly Random Random = new Random();
        private static readonly string[] ToxicGreetings = {
            "ой бля, ёбаный руинер {0} зашел",
            "какая же вам пизда, {0} зашел",
            "это мой друг {0} (не точно) в тс "
        };

        private static readonly string[] Greetings =
        {
            "{0} зашел в тс",
            "это мой друг {0} в тс"
        };

        private static string FindAppropriateGreeting(string nickname)
        {
            if (nickname == "hwoh" || nickname == "h0l3m4k3r")
            {
                var randomIndex = Random.Next(0, ToxicGreetings.Length);
                return string.Format(ToxicGreetings[randomIndex], nickname);
            }
            else
            {
                var randomIndex = Random.Next(0, Greetings.Length);
                return string.Format(Greetings[randomIndex], nickname);
            }
        }
    }
}
