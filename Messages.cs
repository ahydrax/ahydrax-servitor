namespace ahydrax_servitor
{
    public class NotifyChat
    {
        public long ChatId { get; }

        public string Message { get; }

        public NotifyChat(long chatId, string message)
        {
            Message = message;
            ChatId = chatId;
        }
    }

    public class WhoIsInTeamspeak
    {
        public long ChatId { get; }

        public WhoIsInTeamspeak(long chatId)
        {
            ChatId = chatId;
        }
    }
}
