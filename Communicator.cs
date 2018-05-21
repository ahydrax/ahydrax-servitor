using System.Threading.Tasks;

namespace ahydrax_servitor
{
    public class Communicator
    {
        private TelegramBot _telegramActor;
        private TeamspeakBot _teamspeakBot;

        public void StartBotsAndCommunication(TelegramBot telegramActor, TeamspeakBot teamspeakBot)
        {
            _telegramActor = telegramActor;
            _teamspeakBot = teamspeakBot;

            _teamspeakBot.Start().GetAwaiter().GetResult();
            _telegramActor.Start();
        }

        public async Task<string[]> AskTeamspeakWhoIsInChat()
        {
            var clients = await _teamspeakBot.AskTeamspeakWhoIsInChat();
            return clients;
        }

        public async Task SendMessageToCommonChannel(string message)
        {
            await _telegramActor.SendMessageToCommonChannel(message);
        }
    }
}
