using System.Threading.Tasks;

namespace ahydrax_servitor
{
    public abstract class Middleware
    {
        private readonly Middleware _next;

        protected Middleware(Middleware next)
        {
            _next = next;
        }

        public abstract Task InvokeAsync(Context context);

        protected async Task InvokeNext(Context context)
        {
            if (_next != null)
            {
                await _next.InvokeAsync(context);
            }
        }
    }
}
