namespace ahydrax_servitor
{
    public class TelegramMessage<T>
    {
        public long ChatId { get; }

        public T Content { get; }

        public TelegramMessage(long chatId, T content)
        {
            ChatId = chatId;
            Content = content;
        }
    }
}
