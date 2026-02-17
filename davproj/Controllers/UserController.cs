using davproj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.DirectoryServices.AccountManagement;

namespace davproj.Controllers
{
    [Authorize(Roles = "IT_Full")]
    public class UserController : Controller
    {
        private readonly DBContext _db;
        public UserController(DBContext db)
        {
            _db = db;
        }
        
        [HttpGet]
        public IActionResult UserAdd()
        {
            return Json(_db.Users.ToList());
        }

        [HttpPost]
        public IActionResult UserAdd(User user)
        {
            if (ModelState.IsValid)
            {
                _db.Users.Add(user);
                _db.SaveChanges();
                return Json(new { success = true, user = new { id = user.Id, title = user.FullName } });
            }
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, errors });
        }

        [HttpPost]
        public IActionResult UserEdit(User user)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _db.SaveChanges();
                return Json(new { success = true, user = new { id = user.Id, title = user.FullName } });
            }
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, errors });
        }

        [HttpPost]
        public ActionResult UserDelete(int id)
        {
            try
            {
                if (id == 0)
                    return NotFound(new { success = false, message = "ID не указан" });

                var user = _db.Users.Find(id);
                if (user == null)
                    return NotFound(new { success = false, message = "Пользователь не найден" });

                _db.Users.Remove(user);
                _db.SaveChanges();
                return Ok(new { success = true });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { success = false, message = "Невозможно удалить: есть связанные записи в БД." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet]
        public ActionResult ADUserUpdate()
        {
            return Json(new { success = true });
        }

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

        [HttpPost]
        public ActionResult ADUserDelete(int id)
        {
            try
            {
                if (id == 0)
                    return NotFound(new { success = false, message = "ID не указан" });

                var aduser = _db.ADUsers.Find(id);
                if (aduser == null)
                    return NotFound(new { success = false, message = "Принтер не найден" });

                _db.ADUsers.Remove(aduser);
                _db.SaveChanges();
                return Ok(new { success = true });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { success = false, message = "Невозможно удалить: есть связанные записи в БД." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Внутренняя ошибка сервера" });
            }
        }

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
