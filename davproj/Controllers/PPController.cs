using davproj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace davproj.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "IT_Full")]
    public class PPController : Controller
    {
        private readonly DBContext _db;
        public PPController(DBContext db)
        {
            _db = db;
        }
        public string GetDNS(string ipAddress)
        {
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(ipAddress);
                return entry.HostName;
            }
            catch (Exception)
            {
                return "Имя не найдено";
            }
        }

        #region Phone

            [HttpGet]
            public IActionResult Phone()
            {
                return Json(_db.Phones.ToList());
            }

            [HttpPost]
            public IActionResult PhoneAdd(Phone phone)
            {
                if (ModelState.IsValid)
                {
                    _db.Phones.Add(phone);
                    _db.SaveChanges();
                    return Json(new { success = true, phone = new { id = phone.Id, title = phone.Number } });
                }
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
            }

            [HttpPost]
            public IActionResult PhoneEdit(Phone phone)
            {
                if (ModelState.IsValid)
                {
                    _db.Entry(phone).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _db.SaveChanges();
                    return Json(new { success = true, phone = new { id = phone.Id, title = phone.Number } });
                }
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
           }

            [HttpPost]
            public ActionResult PhoneDelete(int id)
            {
                if (id is 0) { return NotFound(); }
                var phone = _db.Phones.Find(id);
                if (phone == null) { return NotFound(); }
                _db.Phones.Remove(phone);
                _db.SaveChanges();
                return Json(new { success = true });
            }
        #endregion
        #region PC
            [HttpGet]
            public IActionResult PC()
            {
                return Json(_db.PCs.ToList());
            }

            [HttpPost]
            public IActionResult PCAdd(PC pc)
            {
                if (ModelState.IsValid)
                {
                    if (pc.Hostname == null && pc.IP != null)
                    {
                        pc.Hostname = GetDNS(pc.IP);
                    }
                    _db.PCs.Add(pc);
                    _db.SaveChanges();
                    return Json(new { success = true, phone = new { id = pc.Id, title = pc.Hostname } });
                }
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
            }

            [HttpPost]
            public IActionResult PCEdit(PC pc)
            {
                if (ModelState.IsValid)
                {
                    if (pc.Hostname == null && pc.IP != null)
                    {
                        pc.Hostname = GetDNS(pc.IP);
                    }
                    _db.Entry(pc).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _db.SaveChanges();
                    return Json(new { success = true, pc = new { id = pc.Id, title = pc.Hostname } });
                }
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
            }

            [HttpPost]
            public ActionResult PCDelete(int id)
            {
                if (id is 0) { return NotFound(); }
                var pc = _db.PCs.Find(id);
                if (pc == null) { return NotFound(); }
                _db.PCs.Remove(pc);
                _db.SaveChanges();
                return Json(new { success = true });
            }
        #endregion
    }
}
