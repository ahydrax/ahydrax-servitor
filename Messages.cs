using System;

namespace ahydrax.Servitor
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

    public class ActorFailed
    {
        public string Reason { get; }
        public Exception Exception { get; }

        public ActorFailed(string reason, Exception exception = null)
        {
            Reason = reason;
            Exception = exception;
        }
    }
}
