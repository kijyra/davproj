using davproj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace davproj.Controllers
{
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
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public IActionResult PhoneAdd()
        {
            ViewData["workplaces"] = _db.Workplaces.ToList();
            ViewData["FormAction"] = "PhoneAdd";
            return PartialView("Phone");
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public IActionResult PhoneAdd(Phone phone)
        {
            if (ModelState.IsValid)
            {
                _db.Phones.Add(phone);
                _db.SaveChanges();
                return Json(new { success = true, phone = new { id = phone.Id, title = phone.Number } });
            }
            ViewData["workplaces"] = _db.Workplaces.ToList();
            ViewData["FormAction"] = "PhoneAdd";
            return PartialView("Phone", phone);
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public ActionResult PhoneEdit(int? id)
        {
            if (id is null or 0)
            {
                return NotFound();
            }
            Phone phone = _db.Phones.Find(id)!;
            if (phone != null)
            {
                ViewData["workplaces"] = _db.Workplaces.ToList();
                ViewData["FormAction"] = "PhoneEdit";
                return PartialView("Phone", phone);
            }
            return NotFound();
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public IActionResult PhoneEdit(Phone phone)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(phone).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _db.SaveChanges();
                return Json(new { success = true, phone = new { id = phone.Id, title = phone.Number } });
            }
            ViewData["workplaces"] = _db.Workplaces.ToList();
            ViewData["FormAction"] = "PhoneEdit";
            return PartialView("Phone", phone);
        }
        [Authorize(Roles = "IT_Full")]
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
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public IActionResult PCAdd()
        {
            ViewData["workplaces"] = _db.Workplaces.ToList();
            ViewData["FormAction"] = "PCAdd";
            return PartialView("PC");
        }
        [Authorize(Roles = "IT_Full")]
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
                return Json(new { success = true, pc = new { id = pc.Id, title = pc.Hostname } });
            }
            ViewData["workplaces"] = _db.Workplaces.ToList();
            ViewData["FormAction"] = "PCAdd";
            return PartialView("PC", pc);
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public ActionResult PCEdit(int? id)
        {
            if (id is null or 0)
            {
                return NotFound();
            }
            PC pc = _db.PCs.Find(id)!;
            if (pc != null)
            {
                ViewData["workplaces"] = _db.Workplaces.ToList();
                ViewData["FormAction"] = "PCEdit";
                return PartialView("PC", pc);
            }
            return NotFound();
        }
        [Authorize(Roles = "IT_Full")]
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
            ViewData["workplaces"] = _db.Workplaces.ToList();
            ViewData["FormAction"] = "PCEdit";
            return PartialView("PC", pc);
        }
        [Authorize(Roles = "IT_Full")]
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
    }
}
