using davproj.Models;
using davproj.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Reflection;

namespace davproj.Controllers
{
    public class PrinterController : Controller
    {
        private readonly IKyoceraSnmpService _printerService;
        private readonly DBContext _db;
        public PrinterController(DBContext db, IKyoceraSnmpService printerService)
        {
            _db = db;
            _printerService = printerService;
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public async Task<ActionResult> UpdateCounters(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Printer printer = await _db.Printers.FindAsync(id);
            if (printer == null)
            {
                return NotFound();
            }
            try
            {
                var counters = await _printerService.GetCountersAsync(printer.IP);
                var changed = false;
                if (counters.PrintCounter > 0)
                { printer.PrintCount = counters.PrintCounter; changed = true; }
                if (counters.ScanCounter > 0)
                { printer.ScanCount = counters.ScanCounter; changed = true; }
                printer.LastUpdateSNMP = DateTime.Now.ToString();
                if (changed)
                { await _db.SaveChangesAsync(); }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Не удалось связаться с принтером: " + ex.Message);
            }
            if (!string.IsNullOrEmpty(Request.Headers["Referer"].ToString()))
            {
                return Redirect(Request.Headers["Referer"].ToString());
            }
            return View(printer);
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
                return "Не найдено";
            }
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public async Task<ActionResult> FuserRepair(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Printer printer = await _db.Printers.FindAsync(id);
            if (printer == null)
            {
                return NotFound();
            }
            if (printer.LastFuserRepair == null)
            {
                printer.LastFuserRepair = new List<string>();
            }
            try
            {
                UpdateCounters(id);
                printer.LastFuserRepair.Add(DateTime.Now.ToString() + " | " + printer.PrintCount);
                _db.Entry(printer).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                printer.LastFuserRepair.Add(DateTime.Now.ToString() + " | " + printer.PrintCount);
                _db.Entry(printer).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _db.SaveChanges();
            }
            if (!string.IsNullOrEmpty(Request.Headers["Referer"].ToString()))
            {
                return Redirect(Request.Headers["Referer"].ToString());
            }
            return View(printer);
        }

        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public IActionResult PrinterAdd()
        {
            ViewData["workplaces"] = _db.Workplaces.ToList();
            ViewData["models"] = _db.PrinterModels.ToList();
            ViewData["FormAction"] = "PrinterAdd";
            return PartialView("Printer");
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public IActionResult PrinterAdd(Printer printer)
        {
            if (ModelState.IsValid)
            {
                if (printer.HostName == null && printer.IP != null)
                {
                    printer.HostName = GetDNS(printer.IP);
                }
                _db.Printers.Add(printer);
                _db.SaveChanges();
                return Json(new { success = true, printer = new { id = printer.Id, title = printer.PrinterName } });
            }
            ViewData["workplaces"] = _db.Workplaces.ToList();
            ViewData["models"] = _db.PrinterModels.ToList();
            ViewData["FormAction"] = "PrinterAdd";
            return PartialView("Printer", printer);
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public ActionResult PrinterEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewData["workplaces"] = _db.Workplaces.ToList();
            ViewData["models"] = _db.PrinterModels.ToList();
            ViewData["FormAction"] = "PrinterEdit";
            Printer printer = _db.Printers.Find(id);
            if (printer != null)
            {
                return PartialView("Printer", printer);
            }
            return NotFound();
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public IActionResult PrinterEdit(Printer printer)
        {
            if (ModelState.IsValid)
            {
                if (printer.HostName == null && printer.IP != null)
                {
                    printer.HostName = GetDNS(printer.IP);
                }
                _db.Entry(printer).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _db.SaveChanges();
                return Json(new { success = true, printer = new { id = printer.Id, title = printer.PrinterName } });
            }
            ViewData["workplaces"] = _db.Workplaces.ToList();
            ViewData["models"] = _db.PrinterModels.ToList();
            ViewData["FormAction"] = "PrinterEdit";
            return PartialView("Printer", printer);
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public ActionResult PrinterDelete(int id)
        {
            if (id == null) { return NotFound(); }
            var printer = _db.Printers.Find(id);
            if (printer == null) { return NotFound(); }
            _db.Printers.Remove(printer);
            _db.SaveChanges();
            return Ok();
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public IActionResult ManufactorAdd()
        {
            ViewData["FormAction"] = "ManufactorAdd";
            return PartialView("Manufactor");
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public IActionResult ManufactorAdd(Manufactor manufactor)
        {
            if (ModelState.IsValid)
            {
                _db.Manufactors.Add(manufactor);
                _db.SaveChanges();
                return Json(new { success = true, manufactor = new { id = manufactor.Id, title = manufactor.Name } });
            }
            ViewData["FormAction"] = "ManufactorAdd";
            return PartialView("Manufactor", manufactor);
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public ActionResult ManufactorEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewData["FormAction"] = "ManufactorEdit";
            Manufactor manufactor = _db.Manufactors.Find(id);
            if (manufactor != null)
            {
                return PartialView("Manufactor", manufactor);
            }
            return NotFound();
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public IActionResult ManufactorEdit(Manufactor manufactor)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(manufactor).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _db.SaveChanges();
                return Json(new { success = true, manufactor = new { id = manufactor.Id, title = manufactor.Name } });
            }
            ViewData["FormAction"] = "ManufactorEdit";
            return PartialView("Manufactor", manufactor);
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public ActionResult ManufactorDelete(int id)
        {
            if (id == null) { return NotFound(); }
            var manufactor = _db.Manufactors.Find(id);
            if (manufactor == null) { return NotFound(); }
            _db.Manufactors.Remove(manufactor);
            _db.SaveChanges();
            return Ok();
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public IActionResult CartridgeAdd()
        {
            ViewData["manufactors"] = _db.Manufactors.ToList();
            ViewData["FormAction"] = "CartridgeAdd";
            return PartialView("Cartridge");
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public IActionResult CartridgeAdd(Cartridge cartridge)
        {
            if (ModelState.IsValid)
            {
                _db.Cartridges.Add(cartridge);
                _db.SaveChanges();
                return Json(new { success = true, cartridge = new { id = cartridge.Id, title = cartridge.Name } });
            }
            ViewData["manufactors"] = _db.Manufactors.ToList();
            ViewData["FormAction"] = "CartridgeAdd";
            return PartialView("Cartridge", cartridge);
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public ActionResult CartridgeEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Cartridge cartridge = _db.Cartridges.Find(id);
            ViewData["manufactors"] = _db.Manufactors.ToList();
            ViewData["FormAction"] = "CartridgeEdit";
            if (cartridge != null)
            {
                return PartialView("Cartridge", cartridge);
            }
            return NotFound();
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public IActionResult CartridgeEdit(Cartridge cartridge)
        {
            if (ModelState.IsValid)
            { 
                _db.Entry(cartridge).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _db.SaveChanges();
                return Json(new { success = true, cartridge = new { id = cartridge.Id, title = cartridge.Name } });
            }
            ViewData["manufactors"] = _db.Manufactors.ToList();
            ViewData["FormAction"] = "CartridgeEdit";
            return PartialView("Cartridge", cartridge);
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public ActionResult CartridgeDelete(int id)
        {
            if (id == null) { return NotFound(); }
            var cartridge = _db.Cartridges.Find(id);
            if (cartridge == null) { return NotFound(); }
            _db.Cartridges.Remove(cartridge);
            _db.SaveChanges();
            return Ok();
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public IActionResult PrinterModelAdd()
        {
            ViewData["cartridges"] = _db.Cartridges.Include(c => c.Manufactor).ToList();
            ViewData["FormAction"] = "PrinterModelAdd";
            return PartialView("PrinterModel");
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public IActionResult PrinterModelAdd(PrinterModel printerModel)
        {
            if (!ModelState.IsValid)
            { 
                _db.PrinterModels.Add(printerModel);
                _db.SaveChanges();
                return Json(new { success = true, printerModel = new { id = printerModel.Id, title = printerModel.Name } });
            }
            ViewData["cartridges"] = _db.Cartridges.Include(c => c.Manufactor).ToList();
            ViewData["FormAction"] = "PrinterModelAdd";
            return PartialView("PrinterModel", printerModel);
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public ActionResult PrinterModelEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewData["cartridges"] = _db.Cartridges.Include(c => c.Manufactor).ToList();
            ViewData["FormAction"] = "PrinterModelEdit";
            PrinterModel printerModel = _db.PrinterModels.Find(id);
            if (printerModel != null)
            {
                return PartialView("PrinterModel", printerModel);
            }
            return NotFound();
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public IActionResult PrinterModelEdit(PrinterModel printerModel)
        {
            if (!ModelState.IsValid) 
            {
                _db.Entry(printerModel).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _db.SaveChanges();
                return Json(new { success = true, printerModel = new { id = printerModel.Id, title = printerModel.Name } });
            }
            ViewData["cartridges"] = _db.Cartridges.ToList();
            ViewData["FormAction"] = "PrinterModelEdit";
            return PartialView("PrinterModel", printerModel);
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public ActionResult PrinterModelDelete(int id)
        {
            if (id == null) { return NotFound(); }
            var printerModel = _db.PrinterModels.Find(id);
            if (printerModel == null) { return NotFound(); }
            _db.PrinterModels.Remove(printerModel);
            _db.SaveChanges();
            return Ok();
        }
    }
}
