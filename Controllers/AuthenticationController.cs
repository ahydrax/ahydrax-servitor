using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ahydrax.Servitor.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace ahydrax.Servitor.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly Settings _settings;

        public AuthenticationController(Settings settings)
        {
            _settings = settings;
        }

        [HttpGet]
        [Route("/login")]
        public IActionResult LoginPage() => View();

        [HttpPost]
        [Route("/login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] UserLogin userLogin)
        {
            var authenticated = ValidateCredentials(userLogin);
            if (authenticated)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userLogin.Login)
                };
                var userIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(userIdentity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                return RedirectToAction("Home", "Home");
            }

            return View("LoginPage", new UserLogin { Reason = "Enter correct credentials" });
        }

        private bool ValidateCredentials(UserLogin userLogin)
        {
            var loginMatched = _settings.Username.Equals(userLogin.Login, StringComparison.InvariantCulture);
            var pwMatched = _settings.Password.Equals(userLogin.Password, StringComparison.InvariantCulture);
            return loginMatched && pwMatched;
        }

        [HttpGet]
        [Route("/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("LoginPage");
        }
    }
}
