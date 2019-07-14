using ahydrax.Servitor.Actors;
using ahydrax.Servitor.Extensions;
using LiteDB;

namespace ahydrax.Servitor.Services
{
    public class GreetingService
    {
        private readonly LiteCollection<Greeting> _greetingsCollection;
        private readonly LiteCollection<LeaveMessage> _leaveMessagesCollection;

        public GreetingService(LiteDatabase db)
        {
            _greetingsCollection = db.GetCollection<Greeting>();
            _greetingsCollection.EnsureIndex(x => x.Nickname);

            _leaveMessagesCollection = db.GetCollection<LeaveMessage>();
            _leaveMessagesCollection.EnsureIndex(x => x.Nickname);
        }

        private static readonly Greeting DefaultGreetMessage = new Greeting { Template = "{0} has entered teamspeak" };
        public string GetGreeting(string nickname)
        {
            var greeting = _greetingsCollection.FindRandomOrDefault(
                x => x.Nickname == nickname || x.Nickname == "all",
                DefaultGreetMessage);

            return greeting.Template;
        }

        private static readonly LeaveMessage DefaultLeaveMessage = new LeaveMessage { Template = "{0} has left teamspeak" };
        public string GetLeaveMessage(string nickname)
        {
            var leaveMessage = _leaveMessagesCollection.FindRandomOrDefault(
                x => x.Nickname == nickname || x.Nickname == "all",
                DefaultLeaveMessage);

            return leaveMessage.Template;
        }
    }
}
