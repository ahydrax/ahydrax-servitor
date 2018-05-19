﻿using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeamSpeak3QueryApi.Net.Specialized;
using TeamSpeak3QueryApi.Net.Specialized.Notifications;

namespace ahydrax_servitor
{
    public class TeamspeakBot
    {
        private readonly ConcurrentDictionary<int, string> _nicknames;
        private readonly Communicator _communicator;
        private readonly BotSettings _settings;
        private readonly ILogger<TeamspeakBot> _logger;
        private readonly TeamSpeakClient _teamSpeakClient;

        public TeamspeakBot(Communicator communicator, BotSettings settings, ILogger<TeamspeakBot> logger)
        {
            _nicknames = new ConcurrentDictionary<int, string>();
            _communicator = communicator;
            _settings = settings;
            _logger = logger;

            _teamSpeakClient = new TeamSpeakClient(_settings.TeamspeakHost, 10011);
        }

        public async Task Start()
        {
            _logger.LogInformation("Starting teamspeak client...");
            await _teamSpeakClient.Connect();
            await _teamSpeakClient.Login(_settings.TeamspeakUsername, _settings.TeamspeakPassword);

            _logger.LogInformation("Teamspeak bot connected");

            await _teamSpeakClient.UseServer(1);
            _logger.LogInformation("Server changed");

            var clients = await _teamSpeakClient.GetClients();
            foreach (var client in clients)
            {
                _nicknames.AddOrUpdate(client.Id, client.NickName, (i, s) => client.NickName);
            }

            await _teamSpeakClient.RegisterServerNotification();

            _teamSpeakClient.Subscribe<ClientEnterView>(collection =>
            {
                foreach (var clientEnterView in collection)
                {
                    _communicator.NotifyTelegramChannelUserJoined(clientEnterView.NickName);
                    _nicknames.AddOrUpdate(clientEnterView.Id, clientEnterView.NickName, (i, s) => clientEnterView.NickName);
                }
            });

            _teamSpeakClient.Subscribe<ClientLeftView>(views =>
            {
                foreach (var clientLeftView in views)
                {
                    _communicator.NotifyTelegramChannelUserLeft(_nicknames[clientLeftView.Id]);
                }
            });
        }

        public async Task<string[]> AskTeamspeakWhoIsInChat()
        {
            var clients = await _teamSpeakClient.GetClients();
            return clients.Select(x => x.NickName).Where(x => !x.Contains(" from ")).ToArray();
        }


    }
}
