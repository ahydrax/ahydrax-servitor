using System.Threading.Tasks;

namespace ahydrax_servitor
{
    public class Communicator
    {
        private TelegramBot _telegramBot;
        private TeamspeakBot _teamspeakBot;

        public void StartBotsAndCommunication(TelegramBot telegramBot, TeamspeakBot teamspeakBot)
        {
            _telegramBot = telegramBot;
            _teamspeakBot = teamspeakBot;

            _telegramBot.Start();
            _teamspeakBot.Start().GetAwaiter().GetResult();
        }

        public async Task<string[]> AskTeamspeakWhoIsInChat()
        {
            var clients = await _teamspeakBot.AskTeamspeakWhoIsInChat();

            return clients;
        }

        public async Task NotifyTelegramChannelUserJoined(string nickName)
        {
            await _telegramBot.SendUserJoined(nickName);
        }

        public async Task NotifyTelegramChannelUserLeft(string nickName)
        {
            await _telegramBot.SendUserLeft(nickName);
        }
    }
}
