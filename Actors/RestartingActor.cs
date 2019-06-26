using System;
using System.Threading.Tasks;
using ahydrax.Servitor.Extensions;
using Akka.Actor;
using LiteDB;

namespace ahydrax.Servitor.Actors
{
    public class RestartingActor : ReceiveActor
    {
        private readonly Settings _settings;
        private readonly LiteDatabase _database;

        public RestartingActor(Settings settings, LiteDatabase database)
        {
            _settings = settings;
            _database = database;
            ReceiveAsync<MessageArgs>(Restart);
        }

        private async Task Restart(MessageArgs arg)
        {
            var actor = await Context.System.SelectActor<TeamspeakActor>().ResolveOne(TimeSpan.Zero);
            Context.System.SelectActor<TelegramMessageChannel>().Tell(new MessageArgs<string>(arg.ChatId, "Restart requested"));
            var stopped = await actor.GracefulStop(TimeSpan.FromSeconds(5));

            if (!stopped)
            {
                await actor.Ask(Kill.Instance);
            }

            Context.System.SelectActor<TelegramMessageChannel>().Tell(new MessageArgs<string>(arg.ChatId, "Stopped"));

            Context.System.ActorOf(
                Props.Create(() => new TeamspeakActor(_settings, _database)),
                nameof(TeamspeakActor));

            Context.System.SelectActor<TelegramMessageChannel>().Tell(new MessageArgs<string>(arg.ChatId, "Restared"));
        }
    }
}
