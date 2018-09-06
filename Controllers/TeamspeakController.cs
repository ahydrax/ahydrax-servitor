using System;
using System.Linq;
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
        public IActionResult TeamspeakPage()
        {
            var greetings = _greetingsCollection.FindAll().ToList();
            var leaveMessages = _leaveMessagesCollection.FindAll().ToList();

            return View(Tuple.Create(greetings, leaveMessages));
        }

        [HttpPost]
        [Route("/ts/greeting")]
        public IActionResult AddGreeting([FromForm] Greeting greeting)
        {
            if (greeting.Nickname != null && greeting.Template != null)
            {
                _greetingsCollection.Insert(greeting);
            }
            return RedirectToAction("TeamspeakPage");
        }

        [HttpPost]
        [Route("/ts/greeting/{id:long}/delete")]
        public IActionResult GreetingDelete(long id)
        {
            _greetingsCollection.Delete(id);
            return RedirectToAction("TeamspeakPage");
        }

        [HttpPost]
        [Route("/ts/leave")]
        public IActionResult AddLeaveMessage([FromForm] LeaveMessage leaveMessage)
        {
            if (leaveMessage.Nickname != null && leaveMessage.Template != null)
            {
                _leaveMessagesCollection.Insert(leaveMessage);
            }
            return RedirectToAction("TeamspeakPage");
        }

        [HttpPost]
        [Route("/ts/leave/{id:long}/delete")]
        public IActionResult LeaveMessageDelete(long id)
        {
            _leaveMessagesCollection.Delete(id);
            return RedirectToAction("TeamspeakPage");
        }
    }
}
