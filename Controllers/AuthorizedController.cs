using System;
using System.Linq;
using System.Security.Claims;
using ahydrax.Servitor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ahydrax.Servitor.Controllers
{
    [Authorize]
    public class AuthorizedController : Controller
    {
        private readonly Lazy<User> _lazyUser;

        public AuthorizedController()
        {
            _lazyUser = new Lazy<User>(ValueFactory);
        }

        private User ValueFactory()
        {
            var name = HttpContext.User.Claims.First(x => x.Type == ClaimTypes.Name).Value;
            return new User { Username = name };
        }

        protected User AuthenticatedUser => _lazyUser.Value;
    }
}
