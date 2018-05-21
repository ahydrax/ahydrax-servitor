using Telegram.Bot.Types;

namespace ahydrax_servitor
{
    public class Context
    {
        public Context(User me, Message message)
        {
            Message = message;
            Me = me;
        }

        public User Me { get; }

        public Message Message { get; }
    }
}
