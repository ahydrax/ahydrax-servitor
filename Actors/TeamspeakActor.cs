using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using LiteDB;
using TeamSpeak3QueryApi.Net.Specialized;
using TeamSpeak3QueryApi.Net.Specialized.Notifications;

namespace ahydrax.Servitor.Actors
{
    public class TeamspeakActor : ReceiveActor
    {
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly ConcurrentDictionary<int, string> _nicknames;
        private readonly Settings _settings;
        private readonly ActorSystem _system;
        private readonly ILoggingAdapter _logger;
        private readonly LiteCollection<Greeting> _greetingsCollection;
        private readonly LiteCollection<LeaveMessage> _leaveMessagesCollection;
        private TeamSpeakClient _teamSpeakClient;
        private Timer _timer;
        private bool _connected;

        public TeamspeakActor(Settings settings, LiteDatabase db)
        {
            _nicknames = new ConcurrentDictionary<int, string>();
            _settings = settings;
            _system = Context.System;
            _logger = Context.GetLogger();
            _system = Context.System;
            _greetingsCollection = db.GetCollection<Greeting>();
            _greetingsCollection.EnsureIndex(x => x.Nickname);

            _leaveMessagesCollection = db.GetCollection<LeaveMessage>();
            _leaveMessagesCollection.EnsureIndex(x => x.Nickname);

            ReceiveAsync<MessageArgs>(RespondWhoIsInTeamspeak);
            ReceiveAsync<ActorFailed>(ActorFailed);
        }

        private async Task ActorFailed(ActorFailed arg)
        {
            DisposeClient();
            await InternalStart();
        }

        protected override void PreStart() => InternalStart().GetAwaiter().GetResult();

        private async Task InternalStart()
        {
            _teamSpeakClient = new TeamSpeakClient(_settings.TeamspeakHost, _settings.TeamspeakPort);
            _logger.Info("Starting teamspeak client...");
            await _teamSpeakClient.Connect();
            await _teamSpeakClient.Login(_settings.TeamspeakUsername, _settings.TeamspeakPassword);
            _logger.Info("Teamspeak bot connected");

            await _teamSpeakClient.UseServer(1);
            _logger.Info("Server changed");

            var me = await _teamSpeakClient.WhoAmI();
            _logger.Info($"Connected using username {me.NickName}");

            _nicknames.Clear();
            var clients = await _teamSpeakClient.GetClients();
            foreach (var client in clients)
            {
                _nicknames.AddOrUpdate(client.Id, client.NickName, (i, s) => client.NickName);
            }

            await _teamSpeakClient.RegisterServerNotification();

            _teamSpeakClient.Subscribe<ClientEnterView>(UserEntered);
            _teamSpeakClient.Subscribe<ClientLeftView>(UserLeft);

            _connected = true;
            _timer = new Timer(Pulse, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        private void DisposeClient()
        {
            _logger.Info("Client disposal initiated");
            _timer?.Dispose();
            _connected = false;
            _teamSpeakClient.Unsubscribe<ClientEnterView>();
            _teamSpeakClient.Unsubscribe<ClientLeftView>();
            _teamSpeakClient.Client?.Dispose();
            _teamSpeakClient = null;
            _logger.Info("Client unsubscribed");
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

        private async void Pulse(object state)
        {
            if (!_connected)
            {
                Self.Tell(new ActorFailed("Connection is broken"));
                return;
            }

            await _semaphoreSlim.WaitAsync();
            try
            {
                var me = await _teamSpeakClient.WhoAmI();
                _logger.Info($"Ping {me.VirtualServerStatus}");
            }
            catch
            {
                _connected = false;
                _logger.Error("Teamspeak actor failed");
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
                var nickname = _nicknames?[clientLeftView.Id] ?? "???";
                var leaveMessage = FindAppropriateLeaveMessage(nickname);
                GetTelegramActor().Tell(new MessageArgs<string>(_settings.TelegramHostGroupId, string.Format(leaveMessage, nickname)));
            }
        }

        private void UserEntered(IReadOnlyCollection<ClientEnterView> collection)
        {
            _logger.Info("user entered");
            foreach (var clientEnterView in collection)
            {
                var nickname = clientEnterView.NickName;
                var template = FindAppropriateGreeting(nickname);
                GetTelegramActor().Tell(new MessageArgs<string>(_settings.TelegramHostGroupId, string.Format(template, nickname)));
                _nicknames.AddOrUpdate(clientEnterView.Id, nickname, (i, s) => clientEnterView.NickName);
            }
        }

        private ActorSelection GetTelegramActor() => _system.ActorSelection("user/" + nameof(TelegramMessageChannel));

        private static readonly Random Random = new Random();

        private string FindAppropriateGreeting(string nickname)
        {
            var greetings = _greetingsCollection.Find(x => x.Nickname == nickname || x.Nickname == "all").ToList();
            if (greetings.Count == 0)
            {
                return "{0} entered teamspeak";
            }

            var greeting = greetings[Random.Next(greetings.Count)];

            return greeting.Template;
        }

        private string FindAppropriateLeaveMessage(string nickname)
        {
            var leaveMessages = _leaveMessagesCollection.Find(x => x.Nickname == nickname || x.Nickname == "all").ToList();
            if (leaveMessages.Count == 0)
            {
                return "{0} left teamspeak";
            }

            var leaveMessage = leaveMessages[Random.Next(leaveMessages.Count)];

            return leaveMessage.Template;
        }
    }
}
