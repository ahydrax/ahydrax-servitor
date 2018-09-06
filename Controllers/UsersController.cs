using System.Linq;
using ahydrax.Servitor.Models;
using LiteDB;
using Microsoft.AspNetCore.Mvc;

namespace ahydrax.Servitor.Controllers
{
    public class UsersController : AuthorizedController
    {
        private readonly LiteCollection<AuthorizedUser> _usersCollection;

        public UsersController(LiteDatabase db)
        {
            _usersCollection = db.GetCollection<AuthorizedUser>();
        }

        [HttpGet]
        [Route("/users")]
        public IActionResult UsersList()
        {
            var users = _usersCollection.FindAll().ToList();
            return View(users);
        }

        [HttpPost]
        [Route("/users")]
        public IActionResult UsersAdd([FromForm] AuthorizedUser user)
        {
            if (user.Id != 0 && user.Name != null)
            {
                _usersCollection.Insert(user.Id, user);
            }

            return RedirectToAction("UsersList");
        }

        [HttpPost]
        [Route("/users/{id:long}/delete")]
        public IActionResult UsersDelete(long id)
        {
            _usersCollection.Delete(id);
            return RedirectToAction("UsersList");
        }
    }
}
