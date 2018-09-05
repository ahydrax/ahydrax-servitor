using ahydrax.Servitor.Actors;
using LiteDB;
using Microsoft.AspNetCore.Mvc;

namespace ahydrax.Servitor.Controllers
{
    public class TeamspeakController : AuthorizedController
    {
        private readonly LiteCollection<Greeting> _greetingsCollection;
        private readonly LiteCollection<LeaveMessage> _leaveMessagesCollection;

        public TeamspeakController(LiteDatabase db)
        {
            _greetingsCollection = db.GetCollection<Greeting>();
            _leaveMessagesCollection = db.GetCollection<LeaveMessage>();
        }

        [HttpGet]
        [Route("/ts")]
        public IActionResult TeamspeakPage() => View();

        [HttpPost]
        [Route("/ts/greeting")]
        public IActionResult AddGreeting([FromForm] Greeting greeting)
        {
            if (greeting.Nickname != null && greeting.Template != null)
            {
                _greetingsCollection.Insert(greeting);
            }

            return View("TeamspeakPage");
        }

        [HttpPost]
        [Route("/ts/leave")]
        public IActionResult AddLeaveMessage([FromForm] LeaveMessage leaveMessage)
        {
            if (leaveMessage.Nickname != null && leaveMessage.Template != null)
            {
                _leaveMessagesCollection.Insert(leaveMessage);
            }

            return View("TeamspeakPage");
        }
    }
}
