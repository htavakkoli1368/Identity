using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using security.Data;

namespace security.Controllers
{
    public class UserController1 : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SecurityDbContext _db;


        public UserController1(SecurityDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            var userList = _db.ApplicationUser.ToList();
            var userRole = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();

            foreach (var user in userList)
            {
                var role = userRole.FirstOrDefault(u=>u.UserId == user.Id);
                if(role == null)
                {
                    user.Role = "None";
                }
                else
                {
                    user.Role = roles.FirstOrDefault(u=>u.Id == user.RoleId).Name;
                }
            }

            return View(userList);
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
