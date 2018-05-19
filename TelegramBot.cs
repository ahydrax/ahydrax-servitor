using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ahydrax_servitor
{
    public class TelegramBot
    {
        private readonly Communicator _communicator;
        private readonly BotSettings _settings;
        private readonly ILogger<TelegramBot> _logger;
        private readonly TelegramBotClient _botClient;

        private static readonly UpdateType[] AllowedUpdates =
        {
            UpdateType.Message,
            UpdateType.ChannelPost
        };

        public TelegramBot(Communicator communicator, BotSettings settings, ILogger<TelegramBot> logger)
        {
            _communicator = communicator;
            _settings = settings;
            _logger = logger;
            _botClient = new TelegramBotClient(settings.TelegramBotApiKey);
        }

        public void Start()
        {
            InitializeBotClient();
        }

        private void InitializeBotClient()
        {
            _botClient.OnMessage += BotClientOnMessage;

            _botClient.StartReceiving(AllowedUpdates);
        }

        private async void BotClientOnMessage(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            if (message.Chat.Id != _settings.AllowedChatId) return;

            if (message.Text == null) return;

            _logger.LogInformation($"Received: {message.Text}");
            var parameters = message.Text.Split(' ');
            var command = parameters.First();

            switch (command)
            {
                case "/whots":
                    await RespondWhoInTeamspeak();
                    break;

                case "/извени":
                    if (parameters.Length == 2)
                    {
                        await SaySorry(parameters[1]);
                    }
                    break;
            }
        }

        private async Task RespondWhoInTeamspeak()
        {
            var clients = await _communicator.AskTeamspeakWhoIsInChat();
            var responseMessage = clients.Length == 0 ? "`В тс пусто.`" : "```\r\n" + string.Join("\r\n", clients) + "```";
            await _botClient.SendTextMessageAsync(new ChatId(_settings.AllowedChatId), responseMessage, ParseMode.Markdown);
        }

        private async Task SaySorry(string s)
        {
            await _botClient.SendTextMessageAsync(new ChatId(_settings.AllowedChatId), $"{s}, за мат извени");
        }

        public async Task SendUserJoined(string nickname)
        {
            await _botClient.SendTextMessageAsync(new ChatId(_settings.AllowedChatId), $"`{nickname} зашел в тс.`", ParseMode.Markdown);
        }

        public async Task SendUserLeft(string nickname)
        {
            await _botClient.SendTextMessageAsync(new ChatId(_settings.AllowedChatId), $"`{nickname} вышел из тс.`", ParseMode.Markdown);
        }
    }
}
