using davproj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.DirectoryServices.AccountManagement;

namespace davproj.Controllers
{
    public class UserController : Controller
    {
        private readonly DBContext _db;
        public UserController(DBContext db)
        {
            _db = db;
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public IActionResult UserAdd()
        {
            ViewData["workplaces"] = _db.Workplaces.ToList();
            ViewData["adusers"] = _db.ADUsers.ToList();
            ViewData["printers"] = _db.Printers.ToList();
            ViewData["FormAction"] = "UserAdd";
            return PartialView("User");
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public IActionResult UserAdd(User user)
        {
            if (ModelState.IsValid)
            {
                _db.Users.Add(user);
                _db.SaveChanges();
                return Json(new { success = true, user = new { id = user.Id, title = user.FullName } });
            }
            ViewData["workplaces"] = _db.Workplaces.ToList();
            ViewData["adusers"] = _db.ADUsers.ToList();
            ViewData["printers"] = _db.Printers.ToList();
            ViewData["FormAction"] = "UserAdd";
            return PartialView("User", user);
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public ActionResult UserEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            User user = _db.Users.Find(id);

            if (user != null)
            {
                ViewData["workplaces"] = _db.Workplaces.ToList();
                ViewData["adusers"] = _db.ADUsers.ToList();
                ViewData["printers"] = _db.Printers.ToList();
                ViewData["FormAction"] = "UserEdit";
                return PartialView("User", user);
            }
            return NotFound();
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public IActionResult UserEdit(User user)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _db.SaveChanges();
                return Json(new { success = true, user = new { id = user.Id, title = user.FullName } });
            }
            ViewData["workplaces"] = _db.Workplaces.ToList();
            ViewData["adusers"] = _db.ADUsers.ToList();
            ViewData["printers"] = _db.Printers.ToList();
            ViewData["FormAction"] = "UserEdit";
            return PartialView("User", user);
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public ActionResult UserDelete(int id)
        {
            if (id == null) { return NotFound(); }
            var user = _db.Users.Find(id);
            if (user == null) { return NotFound(); }
            _db.Users.Remove(user);
            _db.SaveChanges();
            return Ok();
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public ActionResult ADUserUpdate()
        {
            return PartialView("ADUserUpdate");
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public ActionResult ADUserUpdate(string IdentityName)
        {
            ADUser adUser = new ADUser();
            adUser.Group = new List<string>();
            var users = _db.ADUsers.ToList();
            bool isAlready = false;

            foreach (ADUser u in users)
            {
                if (u.Cn == IdentityName.Split('\\')[1])
                {
                    isAlready = true;
                    adUser = u;
                    break;
                }
                else { isAlready = false; }
            }
            if (!isAlready)
            {
                adUser = UpdateADUser(IdentityName);
                _db.ADUsers.AddRange(adUser);
                _db.SaveChanges();
            }
            return Json(new { success = true });
        }
        [Authorize(Roles = "IT_Full")]
        static ADUser UpdateADUser(string IdentityName)
        {
            string domainName = IdentityName.Split('\\')[0];
            string userName = IdentityName.Split('\\')[1];
            PrincipalContext context = new PrincipalContext(ContextType.Domain, domainName);
            UserPrincipal user = UserPrincipal.FindByIdentity(context, userName);
            if (user != null)
            {
                List<string> ad = new List<string>();
                var groups = user.GetAuthorizationGroups();
                List<string> roles = new List<string>();
                foreach (GroupPrincipal group in groups)
                {
                    ad.Add(group.Name);
                }
                ADUser aduser = new ADUser
                {
                    Name = user.GivenName,
                    SurName = user.Surname,
                    Cn = user.SamAccountName,
                    GivenName = user.DisplayName,
                    Admin = ad.Contains("Администраторы"),
                    Group = ad,
                    Enabled = user.Enabled,
                };
                return aduser;
            }
            else { return new ADUser(); }
        }
    }
}
