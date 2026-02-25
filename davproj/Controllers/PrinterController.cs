using davproj.Models;
using davproj.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Execution;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace davproj.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "IT_Full")]
    public class PrinterController : Controller
    {
        private readonly IKyoceraSnmpService _printerService;
        private readonly DBContext _db;
        public PrinterController(DBContext db, IKyoceraSnmpService printerService)
        {
            _db = db;
            _printerService = printerService;
        }
        [HttpPost("UpdateCounters")]
        public async Task<ActionResult> UpdateCounters(int id)
        {
            if (id == 0)
            {
                return NotFound(new { success = false, message = "ID не указан" });
            }
            var printer = await _db.Printers.FindAsync(id);
            if (printer == null)
            {
                return NotFound(new { success = false, message = "Принтер не найден" });
            }
            try
            {
                var counters = await _printerService.GetCountersAsync(printer.IP!);
                var changed = false;
                if (counters.PrintCounter > 0)
                {
                    printer.PrintCount = counters.PrintCounter;
                    changed = true;
                }
                if (counters.ScanCounter > 0)
                {
                    printer.ScanCount = counters.ScanCounter;
                    changed = true;
                }
                printer.LastUpdateSNMP = DateTime.Now.ToString();
                if (changed)
                {
                    await _db.SaveChangesAsync();
                }
                return Ok(new
                {
                    success = true,
                    message = changed ? "Счётчики обновлены" : "Счётчики не изменились",
                    printer = new
                    {
                        printer.Id,
                        printer.PrintCount,
                        printer.ScanCount,
                        printer.LastUpdateSNMP
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Не удалось связаться с принтером: " + ex.Message
                });
            }
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

        [HttpPost("FuserRepair")]
        public async Task<ActionResult> FuserRepair(int id)
        {
            if (id == 0)
            {
                return NotFound(new { success = false, message = "ID не указан" });
            }
            var printer = await _db.Printers.FindAsync(id);
            if (printer == null)
            {
                return NotFound(new { success = false, message = "Принтер не найден" });
            }
            printer.LastFuserRepair ??= [];
            try
            {
                await UpdateCounters(id);
                printer.LastFuserRepair.Add(DateTime.Now.ToString() + " | " + printer.PrintCount);
                _db.Entry(printer).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Запись о ремонте печи добавлена",
                    printer = new
                    {
                        printer.Id,
                        printer.PrintCount,
                        printer.LastFuserRepair
                    }
                });
            }
            catch (Exception ex)
            {
                printer.LastFuserRepair.Add(DateTime.Now.ToString() + " | " + printer.PrintCount);
                _db.Entry(printer).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    warning = "Счётчики не обновлены, но запись добавлена",
                    printer = new
                    {
                        printer.Id,
                        printer.PrintCount,
                        printer.LastFuserRepair
                    }
                });
            }
        }
        #region Printer
            [HttpGet("Printers")]
            public IActionResult Printers()
            {
                return Json(_db.Printers.ToList());
            }

            [HttpGet("Printer/{id}")]
            public IActionResult Printer(int id)
            {
                var printer = _db.Printers.FirstOrDefault(x => x.Id == id);
                return Json(printer);
            }

            [HttpPost("Printer/Add")]
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
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
             }

            [HttpPost("Printer/Edit")]
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
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, errors });
            }

            [HttpPost("Printer/Delete")]
            public ActionResult PrinterDelete(int id)
            {
            try
            {
                if (id == 0)
                    return NotFound(new { success = false, message = "ID не указан" });

                var printer  = _db.Printers.Find(id);
                if (printer == null)
                    return NotFound(new { success = false, message = "Принтер не найден" });

                _db.Printers.Remove(printer);
                _db.SaveChanges();
                return Ok(new { success = true });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { success = false, message = "Невозможно удалить: есть связанные картриджи или модели принтеров." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Внутренняя ошибка сервера" });
            }
        }
        #endregion
        #region Manufactor
            [HttpGet("Manufactors")]
            public IActionResult Manufactors()
            {
                return Json(_db.Manufactors.ToList());
            }

            [HttpGet("manufactor/{id}")]
            public IActionResult Manufactor(int id)
            { 
                return Json(_db.Printers.FirstOrDefault(x => x.Id == id));
            }

            [HttpPost("Manufactor/Add")]
            public IActionResult ManufactorAdd(Manufactor manufactor)
            {
                if (ModelState.IsValid)
                {
                    _db.Manufactors.Add(manufactor);
                    _db.SaveChanges();
                    return Json(new { success = true, manufactor = new { id = manufactor.Id, title = manufactor.Name } });
                }
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
            }

            [HttpPost("Manufactor/Edit")]
            public IActionResult ManufactorEdit(Manufactor manufactor)
            {
                if (ModelState.IsValid)
                {
                    _db.Entry(manufactor).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _db.SaveChanges();
                    return Json(new { success = true, manufactor = new { id = manufactor.Id, title = manufactor.Name } });
                }
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
            }

        [HttpPost("Manufactor/Delete")]
        public ActionResult ManufactorDelete(int id)
        {
            try
            {
                if (id == 0)
                    return NotFound(new { success = false, message = "ID не указан" });

                var manufactor = _db.Manufactors.Find(id);
                if (manufactor == null)
                    return NotFound(new { success = false, message = "Производитель не найден" });

                _db.Manufactors.Remove(manufactor);
                _db.SaveChanges();
                return Ok(new { success = true });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { success = false, message = "Невозможно удалить: есть связанные картриджи или модели принтеров." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Внутренняя ошибка сервера" });
            }
        }
        #endregion
        #region Cartridge
        [HttpGet("cartridge")]
        public IActionResult Cartridges()
        {
            return Json(_db.Cartridges.ToList());
        }

        [HttpGet("cartridge/{id}")]
        public IActionResult Cartridge(int id)
        {
            return Json(_db.Cartridges.FirstOrDefault(x => x.Id == id));
        }

        [HttpPost("Cartridge/Add")]
        public IActionResult CartridgeAdd(Cartridge cartridge)
        {
            if (ModelState.IsValid)
            {
                _db.Cartridges.Add(cartridge);
                _db.SaveChanges();
                return Json(new { success = true, cartridge = new { id = cartridge.Id, title = cartridge.Name } });
            }
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, errors });
        }

        [HttpPost("Cartridge/Edit")]
        public IActionResult CartridgeEdit(Cartridge cartridge)
        {
            if (ModelState.IsValid)
            { 
                _db.Entry(cartridge).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _db.SaveChanges();
                return Json(new { success = true, cartridge = new { id = cartridge.Id, title = cartridge.Name } });
            }
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, errors });
        }

        [HttpPost("Cartridge/Delete")]
        public ActionResult CartridgeDelete(int id)
        {
            try
            {
                if (id == 0)
                    return NotFound(new { success = false, message = "ID не указан" });

                var cartridge = _db.Cartridges.Find(id);
                if (cartridge == null)
                    return NotFound(new { success = false, message = "Картридж не найден" });

                _db.Cartridges.Remove(cartridge);
                _db.SaveChanges();
                return Ok(new { success = true });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { success = false, message = "Невозможно удалить: есть связанные принтеры или модели принтеров." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Внутренняя ошибка сервера" });
            }
        }
        #endregion
        #region PrinterModel
        [HttpGet("printermodel")]
        public IActionResult PrinterModels()
        {
            return Json(_db.PrinterModels.ToList());
        }

        [HttpGet("printermodel/{id}")]
        public IActionResult PrinterModel(int id)
        {
            return Json(_db.PrinterModels.FirstOrDefault(x => x.Id == id));
        }

        [HttpPost("PrinterModel/Add")]
        public IActionResult PrinterModelAdd(PrinterModel printerModel)
        {
            if (!ModelState.IsValid)
            { 
                _db.PrinterModels.Add(printerModel);
                _db.SaveChanges();
                return Json(new { success = true, printerModel = new { id = printerModel.Id, title = printerModel.Name } });
            }
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, errors });
        }

        [HttpPost("PrinterModel/Edit")]
        public IActionResult PrinterModelEdit(PrinterModel printerModel)
        {
            if (!ModelState.IsValid) 
            {
                _db.Entry(printerModel).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _db.SaveChanges();
                return Json(new { success = true, printerModel = new { id = printerModel.Id, title = printerModel.Name } });
            }
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, errors });
        }

        [HttpPost("PrinterModel/Delete")]
        public ActionResult PrinterModelDelete(int id)
        {
            try
            {
                if (id == 0)
                    return NotFound(new { success = false, message = "ID не указан" });

                var printerModel = _db.PrinterModels.Find(id);
                if (printerModel == null)
                    return NotFound(new { success = false, message = "Производитель не найден" });

                _db.PrinterModels.Remove(printerModel);
                _db.SaveChanges();
                return Ok(new { success = true });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { success = false, message = "Невозможно удалить: есть связанные картриджи или модели принтеров." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Внутренняя ошибка сервера" });
            }
        }
        #endregion
    }
}
