using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ahydrax_servitor
{
    public class TelegramMessageChannel : ReceiveActor
    {
        private static readonly UpdateType[] AllowedUpdates =
        {
            UpdateType.Message,
            UpdateType.CallbackQuery,
            UpdateType.ChosenInlineResult,
            UpdateType.InlineQuery
        };

        private readonly ActorSystem _system;
        private readonly TelegramBotClient _telegramClient;
        private readonly ILoggingAdapter _logger;

        public TelegramMessageChannel(Settings settings)
        {
            _logger = Context.GetLogger();
            _system = Context.System;
            _telegramClient = new TelegramBotClient(settings.TelegramBotApiKey);
            _telegramClient.OnMessage += OnMessage;

            ReceiveAsync<TelegramMessage<string>>(SendMessageInChat);
        }

        private Task SendMessageInChat(TelegramMessage<string> arg)
        {
            var monospaceSymbol = arg.Content.Contains("\r\n") ? "```\r\n" : "`";

            return _telegramClient.SendTextMessageAsync(new ChatId(arg.ChatId),
                monospaceSymbol + arg.Content + monospaceSymbol,
                ParseMode.Markdown,
                disableNotification: true);
        }

        protected override void PreStart() => _telegramClient.StartReceiving(AllowedUpdates);

        protected override void PostStop() => _telegramClient.StopReceiving();

        private void OnMessage(object sender, MessageEventArgs e)
        {
            _system.ActorSelection("user/" + nameof(TelegramMessageRouter)).Tell(e.Message);
            _logger.Info("Message arrived: {0}", e);
        }
    }
}
