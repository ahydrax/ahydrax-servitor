using ahydrax.Servitor.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ahydrax.Servitor.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("/error/500")]
        public IActionResult InternalServerError()
        {
            var requestId = HttpContext.TraceIdentifier;

            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionFeature != null)
            {
                var route = exceptionFeature.Path;
                var exception = exceptionFeature.Error;

                _logger.LogError(exception, "Site error", requestId, route);
            }

            Response.StatusCode = 500;
            return View(new ErrorViewModel { RequestId = requestId });
        }

        [Route("/error/404")]
        public IActionResult PageNotFound()
        {
            Response.StatusCode = 404;
            return View();
        }
    }
}
