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
        [HttpGet("phones")]
        public IActionResult Phones()
        {
            return Json(_db.Phones.ToList());
        }

        [HttpGet("phone/{id}")]
        public IActionResult Phone(int id)
        {
            return Json(_db.Phones.FirstOrDefault(x => x.Id == id));
        }

        [HttpPost("phone/add")]
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

            [HttpPost("phone/edit")]
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

            [HttpPost("phone/delete")]
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
            [HttpGet("pcs")]
            public IActionResult PCs()
            {
                return Json(_db.PCs.ToList());
            }

            [HttpGet("pc/{id}")]
            public IActionResult PC(int id)
            {
                return Json(_db.PCs.FirstOrDefault(x => x.Id == id));
            }

            [HttpPost("pc/add")]
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

            [HttpPost("pc/edit")]
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

            [HttpPost("pc/delete")]
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
