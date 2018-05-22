using System;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ahydrax_servitor
{
    public class TelegramActor : ReceiveActor
    {
        private readonly Settings _settings;
        private readonly ActorSystem _system;
        private readonly ILogger<TelegramActor> _logger;
        private readonly TelegramBotClient _botClient;
        private User _user;

        private static readonly UpdateType[] AllowedUpdates =
        {
            UpdateType.Message,
            UpdateType.ChannelPost
        };

        public TelegramActor(Settings settings, ActorSystem system, ILogger<TelegramActor> logger)
        {
            _settings = settings;
            _system = system;
            _logger = logger;
            _botClient = new TelegramBotClient(settings.TelegramBotApiKey);

            ReceiveAsync<NotifyChat>(NotifyChat);
        }

        private async Task NotifyChat(NotifyChat arg)
        {
            await _botClient.SendTextMessageAsync(new ChatId(arg.ChatId),
            "`" + arg.Message + "`",
            ParseMode.Markdown,
            disableNotification: true);
        }

        protected override async void PreStart()
        {
            _botClient.OnMessage += OnMessage;
            _user = await _botClient.GetMeAsync();
            _botClient.StartReceiving(AllowedUpdates);
        }

        protected override void PostStop()
        {
            _botClient.StopReceiving();
        }

        private async void OnMessage(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            if (!AuthorizedUser(message)) return;

            _logger.LogInformation($"Received: {message.Text} from {message.From.Id}");
            var parameters = message.Text.Split(' ');
            var command = parameters.First().Replace("@" + _user.Username, "");

            switch (command)
            {
                case "/whots":
                    RespondWhoInTeamspeak(message.Chat.Id);
                    break;

                case "/извени":
                    if (parameters.Length == 2)
                    {
                        await SaySorry(parameters[1], message.Chat.Id);
                    }
                    break;

                case "/allo":
                    await _botClient.SendTextMessageAsync(new ChatId(message.Chat.Id), $"Хватит баловаться, {message.From.FirstName}");
                    break;

                case "/chokot":
                    await ReplyRandomMessage(message.Chat.Id);
                    break;

                default:
                    await Reply(message.Chat.Id);
                    break;
            }
        }

        private async Task Reply(long chatId)
        {
            var recipient = string.Empty;

            if (chatId == _settings.LarisId)
            {
                recipient = "Ларис, ";
            }

            var message = "хватит дурью маяться";
            var khabTime = (DateTimeOffset.UtcNow + TimeSpan.FromHours(10)).TimeOfDay;

            if (khabTime <= TimeSpan.FromHours(8) || khabTime >= TimeSpan.FromHours(23))
            {
                message += ", иди спать";
            }

            await _botClient.SendTextMessageAsync(new ChatId(chatId), recipient + message);
        }

        private bool AuthorizedUser(Message message)
        {
            return message.Chat.Id == _settings.AllowedChatId ||
                   message.Chat.Id == _settings.LarisId ||
                   message.From.Username == "ahydrax";
        }

        private void RespondWhoInTeamspeak(long chatId)
        {
            var teamspeakActor = _system.ActorSelection("user/" + nameof(TeamspeakActor));
            teamspeakActor.Tell(new WhoIsInTeamspeak(chatId));
        }

        private async Task SaySorry(string s, long chatId)
        {
            await _botClient.SendTextMessageAsync(new ChatId(chatId), $"{s}, за мат извени");
        }

        private static readonly Random Random = new Random();
        private static readonly string[] Replies = {
            "Ларис, я занят",
            "У меня митинг",
            "Кот спит",
            "занет пока",
            "Ларис, чо отвлекаешь",
            "Мущина, вы не видите, у нас обед",
            "Кот отдыхает",
            "Кот ест",
            "Кот мышь поймал",
            "Кот раздербанил крысу",
            "Кот лежит",
            "Кот играется",
            "Кот в беседке",
            "Котик бегает",
            "Кот хищник"
        };

        private async Task ReplyRandomMessage(long chatId)
        {
            var randomIndex = Random.Next(0, Replies.Length);
            var reply = Replies[randomIndex];
            await _botClient.SendTextMessageAsync(new ChatId(chatId), reply);
        }
    }
}
