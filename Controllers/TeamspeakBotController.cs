using System;
using System.Threading.Tasks;
using ahydrax.Servitor.Actors;
using ahydrax.Servitor.Actors.Utility;
using Akka.Actor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ahydrax.Servitor.Controllers
{
    public class TeamspeakBotController : AuthorizedController
    {
        private readonly ActorSystem _actorSystem;
        private readonly ILogger<TeamspeakBotController> _logger;

        public TeamspeakBotController(ActorSystem actorSystem, ILogger<TeamspeakBotController> logger)
        {
            _actorSystem = actorSystem;
            _logger = logger;
        }

        [HttpGet]
        [Route("/tsbot")]
        public async Task<IActionResult> BotPage()
        {
            try
            {
                var bot = await _actorSystem.Actor<TeamspeakActor>().ResolveOne(TimeSpan.Zero);
                return View(!bot.IsNobody());
            }
            catch (ActorNotFoundException)
            {
                return View(false);
            }
        }

        [HttpPost]
        [Route("/tsbot/reload")]
        public async Task<IActionResult> ReloadTsBot()
        {
            await StopBot();
            StartBot();

            return RedirectToAction("BotPage");
        }

        [HttpPost]
        [Route("/tsbot/start")]
        public IActionResult StartTsBot()
        {
            StartBot();
            return RedirectToAction("BotPage");
        }

        [HttpPost]
        [Route("/tsbot/stop")]
        public async Task<IActionResult> StopTsBot()
        {
            await StopBot();

            return RedirectToAction("BotPage");
        }

        private void StartBot()
        {
            _logger.LogWarning("Bot starting");
            _actorSystem.CreateActor<TeamspeakActor>();
            _logger.LogWarning("Bot started");
        }

        private async Task StopBot()
        {
            _logger.LogWarning("Bot stopping");
            var bot = await _actorSystem.Actor<TeamspeakActor>().ResolveOne(TimeSpan.Zero);
            var stopped = await bot.GracefulStop(TimeSpan.FromSeconds(5));
            if (!stopped)
            {
                await bot.Ask(Kill.Instance);
            }

            _logger.LogWarning("Bot stopped");
        }
    }
}
