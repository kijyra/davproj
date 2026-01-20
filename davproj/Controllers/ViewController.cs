using davproj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace davproj.Controllers
{
    public class ViewController : Controller
    {
        private readonly DBContext _db;
        public ViewController(DBContext db)
        {
            _db = db;
        }
        [Authorize(Roles = "IT_Full")]

        public IActionResult Index(int? buildingId, int? floorId)
        {
            var allBuildings = _db.Buildings
                .Include(l => l.Location)
                .Include(b => b.Floors)
                    .ThenInclude(f => f.Offices)
                        .ThenInclude(o => o.Workplaces)
                            .ThenInclude(w => w.User)
                                .ThenInclude(u => u.ADUser)
                .Include(b => b.Floors)
                    .ThenInclude(f => f.Offices)
                        .ThenInclude(o => o.Workplaces)
                            .ThenInclude(w => w.User)
                .Include(b => b.Floors)
                    .ThenInclude(f => f.Offices)
                        .ThenInclude(o => o.Workplaces)
                            .ThenInclude(w => w.PC)
                .Include(b => b.Floors)
                    .ThenInclude(f => f.Offices)
                        .ThenInclude(o => o.Workplaces)
                            .ThenInclude(w => w.Phone)
                .Include(b => b.Floors)
                    .ThenInclude(f => f.Offices)
                        .ThenInclude(o => o.Workplaces)
                            .ThenInclude(w => w.Printer)
                                .ThenInclude(p => p.PrinterModel)
                                    .ThenInclude(pm => pm.Cartridge)
                                        .ThenInclude(c => c.Manufactor)
                .ToList();

            if (!allBuildings.Any())
            {
                return View();
            }
            Building selectedBuilding;
            if (buildingId.HasValue)
            {
                selectedBuilding = _db.Buildings
                .Include(l => l.Location)
                .Include(b => b.Floors)
                    .ThenInclude(f => f.Offices)
                        .ThenInclude(o => o.Workplaces)
                            .ThenInclude(w => w.User)
                                .ThenInclude(u => u.ADUser)
                .Include(b => b.Floors)
                    .ThenInclude(f => f.Offices)
                        .ThenInclude(o => o.Workplaces)
                            .ThenInclude(w => w.User)
                .Include(b => b.Floors)
                    .ThenInclude(f => f.Offices)
                        .ThenInclude(o => o.Workplaces)
                            .ThenInclude(w => w.PC)
                .Include(b => b.Floors)
                    .ThenInclude(f => f.Offices)
                        .ThenInclude(o => o.Workplaces)
                            .ThenInclude(w => w.Phone)
                .Include(b => b.Floors)
                    .ThenInclude(f => f.Offices)
                        .ThenInclude(o => o.Workplaces)
                            .ThenInclude(w => w.Printer)
                                .ThenInclude(p => p.PrinterModel)
                                    .ThenInclude(pm => pm.Cartridge)
                                        .ThenInclude(c => c.Manufactor)
                                       .FirstOrDefault(b => b.Id == buildingId.Value);
                if (selectedBuilding == null)
                {
                    selectedBuilding = allBuildings.FirstOrDefault();
                }
            }
            else
            {
                selectedBuilding = allBuildings.FirstOrDefault();
            }
            if (selectedBuilding != null && selectedBuilding.Floors == null)
            {
                _db.Entry(selectedBuilding).Collection(b => b.Floors).Load();
            }
            Floor selectedFloor;
            IEnumerable<Floor> availableFloors = selectedBuilding?.Floors ?? Enumerable.Empty<Floor>();

            if (floorId.HasValue)
            {
                selectedFloor = _db.Floors
                                   .Include(f => f.Offices)
                                   .ThenInclude(f => f.Workplaces)
                                        .ThenInclude(w => w.User)
                                            .ThenInclude(u => u.ADUser)
                                   .Include(f => f.Offices)
                                        .ThenInclude(o => o.Workplaces)
                                            .ThenInclude(w => w.User)
                                    .Include(f => f.Offices)
                                        .ThenInclude(o => o.Workplaces)
                                            .ThenInclude(w => w.PC)
                                    .Include(f => f.Offices)
                                        .ThenInclude(o => o.Workplaces)
                                            .ThenInclude(w => w.Phone)
                                    .Include(f => f.Offices)
                                        .ThenInclude(o => o.Workplaces)
                            .ThenInclude(w => w.Printer)
                                .ThenInclude(p => p.PrinterModel)
                                    .ThenInclude(pm => pm.Cartridge)
                                        .ThenInclude(c => c.Manufactor)
                                   .FirstOrDefault(f => f.Id == floorId.Value);
                if (selectedFloor == null || selectedFloor.BuildingId != selectedBuilding.Id)
                {
                    selectedFloor = availableFloors.FirstOrDefault();
                }
            }
            else
            {
                selectedFloor = availableFloors.FirstOrDefault();
            }
            ViewBag.Buildings = allBuildings;
            ViewBag.SelectedBuilding = selectedBuilding;
            ViewBag.SelectedFloor = selectedFloor;

            return View();
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public IActionResult WorkplaceAdd()
        {
            ViewData["offices"] = _db.Offices
                .Include(o => o.Floor)
                    .ThenInclude(f => f.Building)
                    .ThenInclude(b => b.Location)
                .ToList();
            ViewData["users"] = _db.Users.ToList();
            ViewData["pcs"] = _db.PCs.ToList();
            ViewData["phones"] = _db.Phones.ToList();
            ViewData["printers"] = _db.Printers
                .Include(p => p.PrinterModel)
                    .ThenInclude(pm => pm.Cartridge)
                    .ThenInclude(c => c.Manufactor)
                .ToList();
            return PartialView("Workplace");
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public IActionResult WorkplaceAdd(Workplace workplace)
        {
            if (ModelState.IsValid)
            {
                var printer = _db.Printers.Find(workplace.PrinterId);
                var office = _db.Offices.Find(workplace.OfficeId);
                var user = _db.Users.Find(workplace.User.Id);
                var pc = _db.PCs.Find(workplace.PC.Id);
                var phone = _db.Phones.Find(workplace.Phone.Id);
                if (printer != null)
                {
                    workplace.Printer = printer;
                }
                if (office != null)
                {
                    workplace.Office = office;
                }
                if (user != null)
                {
                    workplace.User = user;
                }
                if (pc != null)
                {
                    workplace.PC = pc;
                }
                if (phone != null)
                {
                    workplace.Phone = phone;
                }
                _db.Workplaces.Add(workplace);
                _db.SaveChanges();
                return Json(new { success = true });
            }
            ViewData["offices"] = _db.Offices
                .Include(o => o.Floor)
                    .ThenInclude(f => f.Building)
                        .ThenInclude(b => b.Location)
                .ToList();
            ViewData["users"] = _db.Users.ToList();
            ViewData["pcs"] = _db.PCs.ToList();
            ViewData["phones"] = _db.Phones.ToList();
            ViewData["printers"] = _db.Printers
                .Include(p => p.PrinterModel)
                    .ThenInclude(pm => pm.Cartridge)
                    .ThenInclude(c => c.Manufactor)
                .ToList();
            return PartialView("Workplace", workplace);
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public ActionResult WorkplaceEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Workplace workplace = _db.Workplaces.Find(id);

            if (workplace != null)
            {
                ViewData["offices"] = _db.Offices
                    .Include(o => o.Floor)
                        .ThenInclude(f => f.Building)
                        .ThenInclude(b => b.Location)
                    .ToList();
                ViewData["users"] = _db.Users.ToList();
                ViewData["pcs"] = _db.PCs.ToList();
                ViewData["phones"] = _db.Phones.ToList();
                ViewData["printers"] = _db.Printers
                    .Include(p => p.PrinterModel)
                        .ThenInclude(pm => pm.Cartridge)
                        .ThenInclude(c => c.Manufactor)
                    .ToList();
                return PartialView("Workplace", workplace);
            }
            return NotFound();
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public IActionResult WorkplaceEdit(Workplace workplace)
        {
            if (ModelState.IsValid)
            {
                var printer = _db.Printers.Find(workplace.PrinterId);
                var office = _db.Offices.Find(workplace.OfficeId);
                var user = _db.Users.Find(workplace.User.Id);
                var pc = _db.PCs.Find(workplace.PC.Id);
                var phone = _db.Phones.Find(workplace.Phone.Id);
                if (printer != null)
                {
                    workplace.Printer = printer;
                }
                if (office != null)
                {
                    workplace.Office = office;
                }
                if (user != null)
                {
                    workplace.User = user;
                }
                if (pc != null)
                {
                    workplace.PC = pc;
                }
                if (phone != null)
                {
                    workplace.Phone = phone;
                }
                _db.Entry(workplace).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _db.SaveChanges();
                return Json(new { success = true });
            }
            ViewData["offices"] = _db.Offices
                .Include(o => o.Floor)
                    .ThenInclude(f => f.Building)
                        .ThenInclude(b => b.Location)
                .ToList();
            ViewData["users"] = _db.Users.ToList();
            ViewData["pcs"] = _db.PCs.ToList();
            ViewData["phones"] = _db.Phones.ToList();
            ViewData["printers"] = _db.Printers
                .Include(p => p.PrinterModel)
                    .ThenInclude(pm => pm.Cartridge)
                    .ThenInclude(c => c.Manufactor)
                .ToList();
            return PartialView("Workplace", workplace);
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public ActionResult WorkplaceDelete(int id)
        {
            if (id == null) { return NotFound(); }
            var workplace = _db.Workplaces
                    .Include(w => w.Phone)
                    .Include(w => w.Printer)
                    .Include(w => w.User)
                    .Include(w => w.PC)
                    .FirstOrDefault(w => w.Id == id);
            if (workplace != null)
            {
                if (workplace.Phone != null)
                {
                    workplace.Phone = null;
                }
                if (workplace.Printer != null)
                {
                    workplace.Printer = null;
                }
                if (workplace.User != null)
                {
                    workplace.User = null; 
                }
                if (workplace.PC != null)
                {
                    workplace.PC = null;
                }
            }
            if (workplace == null) { return NotFound(); }
            _db.Workplaces.Remove(workplace);
            _db.SaveChanges();
            return Ok();
        }
    }
}
