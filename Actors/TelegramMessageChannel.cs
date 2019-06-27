using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ahydrax.Servitor.Extensions;
using Akka.Actor;
using Akka.Event;
using MihaZupan;
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

            _telegramClient = settings.Socks5 != null
                ? CreateClientWithProxy(settings.Telegram, settings.Socks5)
                : CreateClientWithoutProxy(settings.Telegram);

            _telegramClient.OnMessage += OnMessage;
            _telegramClient.OnReceiveGeneralError += LogError;
            _telegramClient.OnReceiveError += LogError;

            ReceiveAsync<MessageArgs<string>>(SendMessageInChat);
            ReceiveAsync<MessageArgs<byte[]>>(SendContentInChat);
        }

        private Task SendContentInChat(MessageArgs<byte[]> arg)
        {
            return _telegramClient.SendPhotoAsync(new ChatId(arg.ChatId),
                new InputMedia(new MemoryStream(arg.Content), "selfie.jpg"));
        }

        private static TelegramBotClient CreateClientWithoutProxy(TelegramSettings settings)
            => new TelegramBotClient(settings.BotApiKey);

        private static TelegramBotClient CreateClientWithProxy(TelegramSettings tgSettings, Socks5Settings socks5Settings)
        {
            var proxy = new HttpToSocks5Proxy(socks5Settings.Host, socks5Settings.Port, socks5Settings.Username, socks5Settings.Password);
            proxy.ResolveHostnamesLocally = true;
            var handler = new HttpClientHandler();
            handler.Proxy = proxy;
            handler.UseProxy = true;
            var client = new HttpClient(handler);
            return new TelegramBotClient(tgSettings.BotApiKey, client);
        }

        private void LogError(object sender, ReceiveGeneralErrorEventArgs e)
            => _logger.Error("tg error occured", e.Exception.Message, e.Exception.StackTrace);

        private void LogError(object sender, ReceiveErrorEventArgs e)
            => _logger.Error("tg error occured", e.ApiRequestException.Message, e.ApiRequestException.StackTrace);

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
