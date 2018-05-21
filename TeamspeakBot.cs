using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeamSpeak3QueryApi.Net.Specialized;
using TeamSpeak3QueryApi.Net.Specialized.Notifications;

namespace ahydrax_servitor
{
    public class TeamspeakBot
    {
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly ConcurrentDictionary<int, string> _nicknames;
        private readonly Communicator _communicator;
        private readonly BotSettings _settings;
        private readonly ILogger<TeamspeakBot> _logger;
        private readonly TeamSpeakClient _teamSpeakClient;
        private readonly Timer _timer;
        private bool _сonnected;

        public TeamspeakBot(Communicator communicator, BotSettings settings, ILogger<TeamspeakBot> logger)
        {
            _nicknames = new ConcurrentDictionary<int, string>();
            _communicator = communicator;
            _settings = settings;
            _logger = logger;

            _teamSpeakClient = new TeamSpeakClient(_settings.TeamspeakHost, _settings.TeamspeakPort);
            _timer = new Timer(KeepAlive, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        private async void KeepAlive(object state)
        {
            if (!_сonnected) return;

            await _semaphoreSlim.WaitAsync();
            try
            {
                var me = await _teamSpeakClient.WhoAmI();
                _logger.LogInformation($"ping {me.VirtualServerStatus}");
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async Task Start()
        {
            _logger.LogInformation("Starting teamspeak client...");
            try
            {
                await _teamSpeakClient.Connect();
                await _teamSpeakClient.Login(_settings.TeamspeakUsername, _settings.TeamspeakPassword);
            }
            catch (Exception e)
            {
                _logger.LogError($"Unable to connect to {_settings.TeamspeakHost}", e);
            }

            _logger.LogInformation("Teamspeak bot connected");

            await _teamSpeakClient.UseServer(1);
            _logger.LogInformation("Server changed");

            var me = await _teamSpeakClient.WhoAmI();
            _logger.LogInformation($"Connected using username {me.NickName}");

            var clients = await _teamSpeakClient.GetClients();
            foreach (var client in clients)
            {
                _nicknames.AddOrUpdate(client.Id, client.NickName, (i, s) => client.NickName);
            }

            await _teamSpeakClient.RegisterServerNotification();

            _teamSpeakClient.Subscribe<ClientEnterView>(UserEntered);

            _teamSpeakClient.Subscribe<ClientLeftView>(UserLeft);

            _сonnected = true;
        }

        private async void UserLeft(IReadOnlyCollection<ClientLeftView> views)
        {
            _logger.LogInformation("user left");

            foreach (var clientLeftView in views)
            {
                var nickname = _nicknames?[clientLeftView.Id];
                await _communicator.SendMessageToCommonChannel($"{nickname ?? "хз кто"} свалил из тс.");
            }
        }

        private async void UserEntered(IReadOnlyCollection<ClientEnterView> collection)
        {
            _logger.LogInformation("user entered");

            foreach (var clientEnterView in collection)
            {
                var nickname = clientEnterView.NickName;
                await _communicator.SendMessageToCommonChannel(FindAppropriateGreeting(nickname));
                _nicknames.AddOrUpdate(clientEnterView.Id, nickname, (i, s) => clientEnterView.NickName);
            }
        }

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

        public async Task<string[]> AskTeamspeakWhoIsInChat()
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                var clients = await _teamSpeakClient.GetClients();
                return clients.Where(x => x.Type == ClientType.FullClient).Select(x => x.NickName).ToArray();
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}
