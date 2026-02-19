using davproj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Sockets;

namespace davproj.Controllers
{
    [Authorize(Roles = "IT_Full")]
    public class ViewController : Controller
    {
        private readonly DBContext _db;
        public ViewController(DBContext db)
        {
            _db = db;
        }
        [HttpGet("api/view/data")]
        public IActionResult Data(int? buildingId, int? floorId)
        {
            var allBuildings = _db.Buildings
                .Select(b => new { b.Id, b.Name })
                .ToList();

            object? selectedBuilding = null;
            object? selectedFloor = null;

            if (buildingId.HasValue)
            {
                var building = _db.Buildings
                    .Include(b => b.Location)
                    .Include(b => b.Floors!)
                        .ThenInclude(f => f.Offices!)
                            .ThenInclude(o => o.Workplaces!)
                                .ThenInclude(w => w.User!)
                                    .ThenInclude(u => u.ADUser)
                    .Include(b => b.Floors!)
                        .ThenInclude(f => f.Offices!)
                            .ThenInclude(o => o.Workplaces!)
                                .ThenInclude(w => w.PC!)
                                    .ThenInclude(p => p.CurrentHardwareInfo)
                    .Include(b => b.Floors!)
                        .ThenInclude(f => f.Offices!)
                            .ThenInclude(o => o.Workplaces!)
                                .ThenInclude(w => w.Phone)
                    .Include(b => b.Floors!)
                        .ThenInclude(f => f.Offices!)
                            .ThenInclude(o => o.Workplaces!)
                                .ThenInclude(w => w.Printer!)
                                    .ThenInclude(p => p.PrinterModel!)
                                        .ThenInclude(pm => pm.Cartridge!)
                                            .ThenInclude(c => c.Manufactor)
                    .FirstOrDefault(b => b.Id == buildingId.Value);

                if (building != null)
                {
                    selectedBuilding = MapBuilding(building);

                    if (floorId.HasValue)
                    {
                        selectedFloor = building.Floors?.FirstOrDefault(f => f.Id == floorId.Value);
                    }
                    selectedFloor ??= building.Floors?.FirstOrDefault();
                }
            }

            return Ok(new
            {
                buildings = allBuildings,
                selectedBuilding,
                selectedFloor = selectedFloor != null ? MapFloor((Floor)selectedFloor) : null
            });
        }

        [HttpGet]
        public IActionResult WorkplaceAdd()
        {
            ViewData["offices"] = _db.Offices
                .Include(o => o.Floor)
                    .ThenInclude(f => f!.Building)
                    .ThenInclude(b => b!.Location)
                .ToList();
            ViewData["users"] = _db.Users.ToList();
            ViewData["pcs"] = _db.PCs.ToList();
            ViewData["phones"] = _db.Phones.ToList();
            ViewData["printers"] = _db.Printers
                .Include(p => p.PrinterModel)
                    .ThenInclude(pm => pm!.Cartridge)
                    .ThenInclude(c => c.Manufactor)
                .ToList();
            ViewData["FormAction"] = "WorkplaceAdd";
            return PartialView("Workplace");
        }

        [HttpPost]
        public IActionResult WorkplaceAdd(Workplace workplace)
        {
            if (ModelState.IsValid)
            {
                var printer = _db.Printers.Find(workplace.PrinterId);
                var office = _db.Offices.Find(workplace.OfficeId);
                var user = _db.Users.Find(workplace.UserId);
                var pc = _db.PCs.Find(workplace.PCId);
                var phone = _db.Phones.Find(workplace.PhoneId);
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
                    .ThenInclude(f => f!.Building)
                        .ThenInclude(b => b!.Location)
                .ToList();
            ViewData["users"] = _db.Users.ToList();
            ViewData["pcs"] = _db.PCs.ToList();
            ViewData["phones"] = _db.Phones.ToList();
            ViewData["printers"] = _db.Printers
                .Include(p => p.PrinterModel)
                    .ThenInclude(pm => pm!.Cartridge)
                    .ThenInclude(c => c.Manufactor)
                .ToList();
            ViewData["FormAction"] = "WorkplaceAdd";
            return PartialView("Workplace", workplace);
        }

        [HttpGet]
        public ActionResult WorkplaceEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Workplace? workplace = _db.Workplaces.Find(id);
            if (workplace != null)
            {
                ViewData["offices"] = _db.Offices
                    .Include(o => o.Floor)
                        .ThenInclude(f => f!.Building)
                        .ThenInclude(b => b!.Location)
                    .ToList();
                ViewData["users"] = _db.Users.ToList();
                ViewData["pcs"] = _db.PCs.ToList();
                ViewData["phones"] = _db.Phones.ToList();
                ViewData["printers"] = _db.Printers
                    .Include(p => p.PrinterModel)
                        .ThenInclude(pm => pm!.Cartridge)
                        .ThenInclude(c => c.Manufactor)
                    .ToList();
                ViewData["FormAction"] = "WorkplaceEdit";
                return PartialView("Workplace", workplace);
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult WorkplaceEdit(Workplace workplace)
        {
            if (ModelState.IsValid)
            {
                var printer = _db.Printers.Find(workplace.PrinterId);
                var office = _db.Offices.Find(workplace.OfficeId);
                var user = _db.Users.Find(workplace.UserId);
                var pc = _db.PCs.Find(workplace.PCId);
                var phone = _db.Phones.Find(workplace.PhoneId);
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
                    .ThenInclude(f => f!.Building)
                        .ThenInclude(b => b!.Location)
                .ToList();
            ViewData["users"] = _db.Users.ToList();
            ViewData["pcs"] = _db.PCs.ToList();
            ViewData["phones"] = _db.Phones.ToList();
            ViewData["printers"] = _db.Printers
                .Include(p => p.PrinterModel)
                    .ThenInclude(pm => pm!.Cartridge)
                    .ThenInclude(c => c.Manufactor)
                .ToList();
            ViewData["FormAction"] = "WorkplaceEdit";
            return PartialView("Workplace", workplace);
        }

        [HttpPost]
        public ActionResult WorkplaceDelete(int id)
        {
            if (id == 0) { return NotFound(); }
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

        [HttpGet]
        public async Task<IActionResult> PcDetails(int id)
        {
            var PC = _db.PCs
                .Include(p => p.CurrentHardwareInfo)
                .FirstOrDefault(p => p.Id == id);
            if (PC == null)
            {
                return NotFound();
            }
            return PartialView("_HardwareDetails", PC);
        }

        private object MapBuilding(Building b)
        {
            return new
            {
                b.Id,
                b.Name,
                Location = b.Location == null ? null : new { b.Location.Id, b.Location.Name },
                Floors = b.Floors?.Select(f => MapFloor(f)).ToList()
            };
        }

        private object MapFloor(Floor f)
        {
            return new
            {
                f.Id,
                f.FloorNum,
                Offices = f.Offices?.Select(o => MapOffice(o)).ToList()
            };
        }

        private object MapOffice(Office o)
        {
            return new
            {
                o.Id,
                o.Name,
                Workplaces = o.Workplaces?.Select(w => MapWorkplace(w)).ToList()
            };
        }

        private object MapWorkplace(Workplace w)
        {
            return new
            {
                w.Id,
                w.Name,
                w.Print,
                User = w.User == null ? null : new
                {
                    w.User.Id,
                    FullName = w.User.FullName,
                    w.User.Position,
                    ADUser = w.User.ADUser == null ? null : new { w.User.ADUser.Cn }
                },
                PC = w.PC == null ? null : new
                {
                    w.PC.Id,
                    w.PC.Hostname,
                    w.PC.IP,
                    w.PC.Domain,
                    w.PC.Think,
                    CurrentHardwareInfo = w.PC.CurrentHardwareInfo == null ? null : new
                    {
                        w.PC.CurrentHardwareInfo.Id,
                        w.PC.CurrentHardwareInfo.ComputerName,
                        w.PC.CurrentHardwareInfo.ProcessorName,
                        w.PC.CurrentHardwareInfo.MonitorInfo,
                        w.PC.CurrentHardwareInfo.TotalMemoryGB,
                        w.PC.CurrentHardwareInfo.VideoCard,
                        w.PC.CurrentHardwareInfo.OSVersion,
                        w.PC.CurrentHardwareInfo.DiskInfo,
                        w.PC.CurrentHardwareInfo.DiskType,
                        w.PC.CurrentHardwareInfo.SerialNumber,
                        w.PC.CurrentHardwareInfo.TotalRamSlots,
                        w.PC.CurrentHardwareInfo.UsedRamSlots,
                        w.PC.CurrentHardwareInfo.RamType,
                        w.PC.CurrentHardwareInfo.RamManufacturer,
                        w.PC.CurrentHardwareInfo.IsDomainJoined,
                        w.PC.CurrentHardwareInfo.IpAddress,
                        w.PC.CurrentHardwareInfo.CollectedAtUtc,
                        w.PC.CurrentHardwareInfo.MotherboardModel,
                        w.PC.CurrentHardwareInfo.CurrentUserName,
                        w.PC.CurrentHardwareInfo.RamSpeed,
                        w.PC.CurrentHardwareInfo.DiskHealth,
                        w.PC.CurrentHardwareInfo.Antivirus,
                        w.PC.CurrentHardwareInfo.Uptime,
                        SoftwareList = w.PC.CurrentHardwareInfo.SoftwareList,
                        UsbDevices = w.PC.CurrentHardwareInfo.UsbDevices,
                        Printers = w.PC.CurrentHardwareInfo.Printers,
                        OpenPorts = w.PC.CurrentHardwareInfo.OpenPorts,
                        w.PC.CurrentHardwareInfo.PendingUpdatesCount,
                        w.PC.CurrentHardwareInfo.LastUpdateDate
                    }
                },
                Phone = w.Phone == null ? null : new
                {
                    w.Phone.Id,
                    w.Phone.Number,
                    w.Phone.Model,
                    w.Phone.Ip,
                    w.Phone.Handset,
                    w.Phone.NameInBase
                },
                Printer = w.Printer == null ? null : new
                {
                    w.Printer.Id,
                    w.Printer.PrinterName,
                    w.Printer.IP,
                    w.Printer.HostName,
                    w.Printer.PrintCount,
                    w.Printer.ScanCount,
                    w.Printer.LastUpdateSNMP,
                    w.Printer.LastFuserRepair,
                    PrinterModel = w.Printer.PrinterModel == null ? null : new
                    {
                        w.Printer.PrinterModel.Name,
                        Cartridge = w.Printer.PrinterModel.Cartridge == null ? null : new
                        {
                            w.Printer.PrinterModel.Cartridge.Model,
                            Manufactor = w.Printer.PrinterModel.Cartridge.Manufactor?.Name
                        }
                    }
                }
            };
        }
    }
}
