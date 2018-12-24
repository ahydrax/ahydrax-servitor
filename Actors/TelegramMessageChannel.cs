using System.Threading.Tasks;
using ahydrax.Servitor.Extensions;
using Akka.Actor;
using Akka.Event;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ahydrax.Servitor.Actors
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

            ReceiveAsync<MessageArgs<string>>(SendMessageInChat);
        }

        protected override void PreStart() => _telegramClient.StartReceiving(AllowedUpdates);

        protected override void PostStop() => _telegramClient.StopReceiving();

        private void OnMessage(object sender, MessageEventArgs e)
        {
            _logger.Info("Message arrived '{0}' from {1}", e.Message.Text, e.Message.From.Id);
            _system.SelectActor<TelegramMessageRouter>().Tell(e.Message);
        }

        private Task SendMessageInChat(MessageArgs<string> arg)
        {
            var monospaceSymbol = arg.Content.Contains("\r\n") ? "```\r\n" : "`";

            return _telegramClient.SendTextMessageAsync(new ChatId(arg.ChatId),
                monospaceSymbol + arg.Content + monospaceSymbol,
                ParseMode.Markdown,
                disableNotification: true);
        }
    }
}
