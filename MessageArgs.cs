namespace ahydrax.Servitor
{
    public class MessageArgs
    {
        public long ChatId { get; }

        public MessageArgs(long chatId)
        {
            ChatId = chatId;
        }
    }

    public class MessageArgs<T> : MessageArgs
    {
        public T Content { get; }

        public MessageArgs(long chatId, T content) : base(chatId)
        {
            Content = content;
        }
    }
}
