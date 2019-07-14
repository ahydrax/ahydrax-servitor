using System;
using System.Text;
using ahydrax.Servitor.Actors;
using ahydrax.Servitor.Actors.Utility;
using Akka.Actor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ahydrax.Servitor.Controllers
{
    public class Alert
    {
        [JsonProperty("evalMatches")]
        public EvalMatch[] EvalMatches { get; set; }

        [JsonProperty("imageUrl")]
        public Uri ImageUrl { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("ruleId")]
        public long RuleId { get; set; }

        [JsonProperty("ruleName")]
        public string RuleName { get; set; }

        [JsonProperty("ruleUrl")]
        public Uri RuleUrl { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public class EvalMatch
    {
        [JsonProperty("value")]
        public long Value { get; set; }

        [JsonProperty("metric")]
        public string Metric { get; set; }

        [JsonProperty("tags")]
        public object Tags { get; set; }
    }

    [AllowAnonymous]
    [ApiController]
    public class AlertsController : ControllerBase
    {
        private readonly ActorSystem _actorSystem;

        public AlertsController(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        [HttpPost]
        [Route("api/alert/{chatId:int}")]
        public ActionResult PostAlert([FromRoute] int chatId, Alert alert)
        {
            var messageChannel = _actorSystem.Actor<TelegramMessageChannel>();

            var body = GenerateBody(alert);

            messageChannel.Tell(new MessageArgs<string>(chatId, body));

            return Ok();
        }

        private static string GenerateBody(Alert alert)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"Alert status: {alert.RuleName}");
            builder.AppendLine(alert.Message);

            foreach (var alertMatch in alert.EvalMatches)
            {
                builder.AppendLine($" * {alertMatch.Metric} - {alertMatch.Value}");
            }

            return builder.ToString();
        }
    }
}
