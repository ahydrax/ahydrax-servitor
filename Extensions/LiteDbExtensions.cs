using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using LiteDB;

namespace ahydrax.Servitor.Extensions
{
    internal static class LiteDbExtensions
    {
        private static readonly ThreadLocal<Random> ThreadLocalRandom = new ThreadLocal<Random>(() => new Random());
        
        public static T FindRandomOrDefault<T>(this LiteCollection<T> collection, Expression<Func<T, bool>> predicate, T ifNotFound)
        {
            var entities = collection.Find(predicate).ToArray();
            if (entities.Length == 0)
            {
                return ifNotFound;
            }

            var rnd = ThreadLocalRandom.Value;
            return entities[rnd.Next(0, entities.Length)];
        }
    }
}
