using System;
using System.Linq;
using Akka.Actor;
using Akka.DI.Core;
using Microsoft.Extensions.DependencyInjection;

namespace ahydrax.Servitor.Actors.Utility
{
    public class MicrosoftDependencyResolver : IDependencyResolver, IDisposable
    {
        private readonly IServiceScope _scope;

        public MicrosoftDependencyResolver(IServiceProvider serviceProvider)
        {
            _scope = serviceProvider.CreateScope();
        }

        public Type GetType(string actorName)
            => typeof(MicrosoftDependencyResolver).Assembly.GetTypes()
                .FirstOrDefault(x => x.Name == actorName);

        public Func<ActorBase> CreateActorFactory(Type actorType)
            => () => (ActorBase)_scope.ServiceProvider.GetService(actorType);

        public Props Create<TActor>() where TActor : ActorBase
            => new Props(typeof(TActor));

        public Props Create(Type actorType)
            => new Props(actorType);

        public void Release(ActorBase actor)
        {
        }

        public void Dispose() => _scope?.Dispose();
    }
}
