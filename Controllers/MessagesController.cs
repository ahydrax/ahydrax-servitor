using ahydrax.Servitor.Actors;
using ahydrax.Servitor.Extensions;
using ahydrax.Servitor.Models;
using Akka.Actor;
using Microsoft.AspNetCore.Mvc;

namespace ahydrax.Servitor.Controllers
{
    public class MessagesController : AuthorizedController
    {
        private readonly Settings _settings;
        private readonly ActorSystem _actorSystem;

        public MessagesController(Settings settings, ActorSystem actorSystem)
        {
            _settings = settings;
            _actorSystem = actorSystem;
        }

        [HttpGet]
        [Route("/messages")]
        public IActionResult MessagesPage() => View();

        [HttpPost]
        [Route("/messages")]
        public IActionResult PostMessage([FromForm] TextMessage message)
        {
            if (message.Text != null)
            {
                var messageChannel = _actorSystem.SelectActor<TelegramMessageChannel>();
                messageChannel.Tell(new MessageArgs<string>(_settings.TelegramHostGroupId, message.Text));
            }

            return View("MessagesPage");
        }
    }
}
