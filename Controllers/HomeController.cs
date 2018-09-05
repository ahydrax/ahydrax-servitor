using Microsoft.AspNetCore.Mvc;

namespace ahydrax.Servitor.Controllers
{
    public class HomeController : AuthorizedController
    {
        [HttpGet]
        [Route("/")]
        public IActionResult Home() => View();


    }
}
