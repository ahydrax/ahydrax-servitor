﻿using System;
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
        private readonly BotSettings _settings;
        private readonly ILogger<TelegramActor> _logger;
        private readonly TelegramBotClient _botClient;
        private User _user;

        private static readonly UpdateType[] AllowedUpdates =
        {
            UpdateType.Message,
            UpdateType.ChannelPost
        };

        public TelegramActor(BotSettings settings, ILogger<TelegramActor> logger)
        {
            _settings = settings;
            _logger = logger;
            _botClient = new TelegramBotClient(settings.TelegramBotApiKey);

            Receive<>(async r => await Start());
        }

        protected override void PreStart()
        {
            _botClient.OnMessage += BotClientOnMessage;
            _user = _botClient.GetMeAsync().GetAwaiter().GetResult();
            _botClient.StartReceiving(AllowedUpdates);
        }

        protected override void PostStop()
        {
            _botClient.StopReceiving();
        }


        private async void BotClientOnMessage(object sender, MessageEventArgs e)
        {


            var message = e.Message;
            if (!AuthorizedUser(message)) return;

            if (message.Text == null) return;

            _logger.LogInformation($"Received: {message.Text} from {message.From.Id}");
            var parameters = message.Text.Split(' ');
            var command = parameters.First();

            switch (command)
            {
                case "/whots@ahydrax_servitor_bot":
                case "/whots":
                    await RespondWhoInTeamspeak(message.Chat.Id);
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

        private async Task RespondWhoInTeamspeak(long chatId)
        {
            var clients = await _communicator.AskTeamspeakWhoIsInChat();
            var responseMessage = clients.Length == 0 ? "`В тс пусто.`" : "```\r\n" + string.Join("\r\n", clients) + "```";
            await _botClient.SendTextMessageAsync(new ChatId(chatId), responseMessage, ParseMode.Markdown, disableNotification: true);
        }

        private async Task SaySorry(string s, long chatId)
        {
            await _botClient.SendTextMessageAsync(new ChatId(chatId), $"{s}, за мат извени");
        }

        public void Stop()
        {
            _botClient.StopReceiving();
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

        public async Task SendMessageToCommonChannel(string message)
        {
            await _botClient.SendTextMessageAsync(new ChatId(_settings.AllowedChatId),
                "`" + message + "`",
                ParseMode.Markdown,
                disableNotification: true);
        }
    }
}