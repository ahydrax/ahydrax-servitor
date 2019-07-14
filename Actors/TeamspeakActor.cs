using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ahydrax.Servitor.Actors.Utility;
using ahydrax.Servitor.Services;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using TeamSpeak3QueryApi.Net.Specialized;
using TeamSpeak3QueryApi.Net.Specialized.Notifications;

namespace ahydrax.Servitor.Actors
{
    public class TeamspeakActor : ReceiveActor
    {
        private readonly ConcurrentDictionary<int, string> _nicknamesCache;
        private readonly GreetingService _greetingService;
        private readonly Settings _settings;
        private readonly ActorSystem _system;
        private readonly ILogger<TeamspeakActor> _logger;
        private TeamSpeakClient _teamSpeakClient;

        public TeamspeakActor(GreetingService greetingService, Settings settings, ILogger<TeamspeakActor> logger)
        {
            _nicknamesCache = new ConcurrentDictionary<int, string>();
            _greetingService = greetingService;
            _settings = settings;
            _system = Context.System;
            _logger = logger;

            ReceiveAsync<MessageArgs>(RespondWhoIsInTeamspeak);
            ReceiveAsync<Pulse>(PulseServerAsync);
        }

        protected override SupervisorStrategy SupervisorStrategy()
            => new OneForOneStrategy(10, 100, exception => Directive.Restart);

        private async Task PulseServerAsync(Pulse arg)
        {
            try
            {
                var me = await _teamSpeakClient.WhoAmI();
                _logger.LogInformation($"Ping {me.VirtualServerStatus}");
            }
            catch
            {
                _logger.LogError("Teamspeak actor failed");
                throw;
            }
        }

        protected override void PreStart() => InternalStart().GetAwaiter().GetResult();

        private async Task InternalStart()
        {
            _teamSpeakClient = new TeamSpeakClient(_settings.Teamspeak.Host, _settings.Teamspeak.Port);
            _logger.LogInformation("Starting teamspeak client...");
            await _teamSpeakClient.Connect();
            await _teamSpeakClient.Login(_settings.Teamspeak.Username, _settings.Teamspeak.Key);
            _logger.LogInformation("Teamspeak bot connected");

            await _teamSpeakClient.UseServer(1);
            _logger.LogInformation("Server changed");

            var me = await _teamSpeakClient.WhoAmI();
            _logger.LogInformation($"Connected using username {me.NickName}");

            _nicknamesCache.Clear();
            var clients = await _teamSpeakClient.GetClients();
            foreach (var client in clients)
            {
                _nicknamesCache.AddOrUpdate(client.Id, client.NickName, (i, s) => client.NickName);
            }

            await _teamSpeakClient.RegisterServerNotification();

            _teamSpeakClient.Subscribe<ClientEnterView>(UserEntered);
            _teamSpeakClient.Subscribe<ClientLeftView>(UserLeft);
            _system.Actor<TelegramMessageChannel>().Tell(new MessageArgs<string>(_settings.Telegram.HostGroupId, "[TEAMSPEAK_ACTOR] I'm alive"));
        }

        protected override void PostStop()
        {
            _system.Actor<TelegramMessageChannel>().Tell(new MessageArgs<string>(_settings.Telegram.HostGroupId, "[TEAMSPEAK_ACTOR] I'm dead :("));
            _logger.LogInformation("Client disposal initiated");
            _teamSpeakClient.Unsubscribe<ClientEnterView>();
            _teamSpeakClient.Unsubscribe<ClientLeftView>();
            _teamSpeakClient.Dispose();
            _logger.LogInformation("Client disposed");
        }

        private async Task RespondWhoIsInTeamspeak(MessageArgs arg)
        {
            try
            {
                var clients = await _teamSpeakClient.GetClients();
                var clientNicknames = clients
                    .Where(x => x.Type == ClientType.FullClient)
                    .Select(x => x.NickName)
                    .OrderBy(x => x)
                    .ToArray();

                var message = clientNicknames.Length == 0 ? "there are no clients connected" : string.Join("\r\n", clientNicknames);

                _system.Actor<TelegramMessageChannel>().Tell(new MessageArgs<string>(arg.ChatId, message));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occured during querying teamspeak server");
                throw;
            }
        }

        private void UserEntered(IReadOnlyCollection<ClientEnterView> collection)
        {
            foreach (var clientEnterView in collection)
            {
                var nickname = clientEnterView.NickName;
                var template = _greetingService.GetGreeting(nickname);
                _system.Actor<TelegramMessageChannel>().Tell(new MessageArgs<string>(_settings.Telegram.HostGroupId, string.Format(template, nickname)));
                _nicknamesCache.AddOrUpdate(clientEnterView.Id, nickname, (i, s) => clientEnterView.NickName);
                _logger.LogInformation($"{nickname} has entered");
            }
        }

        private void UserLeft(IReadOnlyCollection<ClientLeftView> views)
        {
            foreach (var clientLeftView in views)
            {
                var nickname = _nicknamesCache[clientLeftView.Id] ?? "???";
                var leaveMessage = _greetingService.GetLeaveMessage(nickname);
                _system.Actor<TelegramMessageChannel>().Tell(new MessageArgs<string>(_settings.Telegram.HostGroupId, string.Format(leaveMessage, nickname)));
                _nicknamesCache.TryRemove(clientLeftView.Id, out var _);
                _logger.LogInformation($"{nickname} has left");
            }
        }
    }
}
