using davproj.Filters;
using davproj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace davproj.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "IT_Full")]
    public class ViewController : Controller
    {
        private readonly DBContext _db;
        public ViewController(DBContext db)
        {
            _db = db;
        }

        // GET: api/view/data?buildingId=...&floorId=...
        [HttpGet("data")]
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

        // GET: api/view/workplace/add – возвращает справочники для формы создания рабочего места
        [HttpGet("workplace/add")]
        public IActionResult GetWorkplaceAddData()
        {
            var offices = _db.Offices
                .Include(o => o.Floor)
                    .ThenInclude(f => f!.Building)
                    .ThenInclude(b => b!.Location)
                .Select(o => new
                {
                    o.Id,
                    Title = o.FullTitle,
                    o.FloorId,
                    FloorNumber = o.Floor != null ? o.Floor.FloorNum : null,
                    BuildingName = o.Floor != null && o.Floor.Building != null ? o.Floor.Building.Name : null,
                    LocationName = o.Floor != null && o.Floor.Building != null && o.Floor.Building.Location != null ? o.Floor.Building.Location.Name : null
                })
                .ToList();

            var users = _db.Users
                .Select(u => new { u.Id, FullName = u.FullName })
                .ToList();

            var pcs = _db.PCs
                .Select(p => new { p.Id, Title = p.FullName })
                .ToList();

            var phones = _db.Phones
                .Select(p => new { p.Id, Title = p.Number + " (" + p.Model + ")" })
                .ToList();

            var printers = _db.Printers
                .Include(p => p.PrinterModel)
                    .ThenInclude(pm => pm!.Cartridge)
                    .ThenInclude(c => c.Manufactor)
                .Select(p => new
                {
                    p.Id,
                    Title = p.PrinterName + " (" + (p.PrinterModel != null ? p.PrinterModel.Name : "без модели") + ")",
                    p.IP,
                    p.HostName,
                    PrinterModel = p.PrinterModel != null ? new
                    {
                        p.PrinterModel.Name,
                        Cartridge = p.PrinterModel.Cartridge != null ? new
                        {
                            p.PrinterModel.Cartridge.Model,
                            Manufactor = p.PrinterModel.Cartridge.Manufactor != null ? p.PrinterModel.Cartridge.Manufactor.Name : null
                        } : null
                    } : null
                })
                .ToList();

            return Ok(new
            {
                offices,
                users,
                pcs,
                phones,
                printers,
                formAction = "add"
            });
        }

        // POST: api/view/workplace/add – создание рабочего места
        [HttpPost("workplace/add")]
        public IActionResult WorkplaceAdd([FromBody] Workplace workplace)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (workplace.PrinterId.HasValue)
                workplace.Printer = _db.Printers.Find(workplace.PrinterId.Value);
            if (workplace.OfficeId.HasValue)
                workplace.Office = _db.Offices.Find(workplace.OfficeId.Value);
            if (workplace.UserId.HasValue)
                workplace.User = _db.Users.Find(workplace.UserId.Value);
            if (workplace.PCId.HasValue)
                workplace.PC = _db.PCs.Find(workplace.PCId.Value);
            if (workplace.PhoneId.HasValue)
                workplace.Phone = _db.Phones.Find(workplace.PhoneId.Value);

            _db.Workplaces.Add(workplace);
            _db.SaveChanges();

            return Ok(new { success = true, workplaceId = workplace.Id });
        }

        // GET: api/view/workplace/edit/{id} – возвращает данные для редактирования рабочего места
        [HttpGet("workplace/edit/{id}")]
        public IActionResult GetWorkplaceEditData(int id)
        {
            var workplace = _db.Workplaces
                .Include(w => w.Office)
                    .ThenInclude(o => o!.Floor)
                        .ThenInclude(f => f!.Building)
                            .ThenInclude(b => b!.Location)
                .Include(w => w.User)
                .Include(w => w.PC)
                .Include(w => w.Phone)
                .Include(w => w.Printer)
                    .ThenInclude(p => p!.PrinterModel)
                        .ThenInclude(pm => pm!.Cartridge)
                            .ThenInclude(c => c!.Manufactor)
                .FirstOrDefault(w => w.Id == id);

            if (workplace == null)
                return NotFound();

            var offices = _db.Offices
                .Include(o => o.Floor)
                    .ThenInclude(f => f!.Building)
                    .ThenInclude(b => b!.Location)
                .Select(o => new
                {
                    o.Id,
                    Title = o.FullTitle,
                    o.FloorId,
                    FloorNumber = o.Floor != null ? o.Floor.FloorNum : null,
                    BuildingName = o.Floor != null && o.Floor.Building != null ? o.Floor.Building.Name : null,
                    LocationName = o.Floor != null && o.Floor.Building != null && o.Floor.Building.Location != null ? o.Floor.Building.Location.Name : null
                })
                .ToList();

            var users = _db.Users.Select(u => new { u.Id, FullName = u.FullName }).ToList();
            var pcs = _db.PCs.Select(p => new { p.Id, Title = p.FullName }).ToList();
            var phones = _db.Phones.Select(p => new { p.Id, Title = p.Number + " (" + p.Model + ")" }).ToList();
            var printers = _db.Printers
                .Include(p => p.PrinterModel)
                    .ThenInclude(pm => pm!.Cartridge)
                    .ThenInclude(c => c.Manufactor)
                .Select(p => new
                {
                    p.Id,
                    Title = p.PrinterName + " (" + (p.PrinterModel != null ? p.PrinterModel.Name : "без модели") + ")",
                    p.IP,
                    p.HostName,
                    PrinterModel = p.PrinterModel != null ? new
                    {
                        p.PrinterModel.Name,
                        Cartridge = p.PrinterModel.Cartridge != null ? new
                        {
                            p.PrinterModel.Cartridge.Model,
                            Manufactor = p.PrinterModel.Cartridge.Manufactor != null ? p.PrinterModel.Cartridge.Manufactor.Name : null
                        } : null
                    } : null
                })
                .ToList();

            return Ok(new
            {
                workplace = new
                {
                    workplace.Id,
                    workplace.Name,
                    workplace.Print,
                    workplace.OfficeId,
                    workplace.UserId,
                    workplace.PCId,
                    workplace.PhoneId,
                    workplace.PrinterId
                },
                offices,
                users,
                pcs,
                phones,
                printers,
                formAction = "edit"
            });
        }

        // POST: api/view/workplace/edit – обновление рабочего места
        [HttpPost("workplace/edit")]
        public IActionResult WorkplaceEdit([FromBody] Workplace workplace)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = _db.Workplaces
                .Include(w => w.Printer)
                .Include(w => w.Office)
                .Include(w => w.User)
                .Include(w => w.PC)
                .Include(w => w.Phone)
                .FirstOrDefault(w => w.Id == workplace.Id);

            if (existing == null)
                return NotFound();

            existing.Name = workplace.Name;
            existing.Print = workplace.Print;
            existing.OfficeId = workplace.OfficeId;
            existing.UserId = workplace.UserId;
            existing.PCId = workplace.PCId;
            existing.PhoneId = workplace.PhoneId;
            existing.PrinterId = workplace.PrinterId;

            _db.Entry(existing).State = EntityState.Modified;
            _db.SaveChanges();

            return Ok(new { success = true });
        }

        // POST: api/view/workplace/delete/{id}
        [HttpPost("workplace/delete/{id}")]
        public IActionResult WorkplaceDelete(int id)
        {
            var workplace = _db.Workplaces
                .Include(w => w.Phone)
                .Include(w => w.Printer)
                .Include(w => w.User)
                .Include(w => w.PC)
                .FirstOrDefault(w => w.Id == id);

            if (workplace == null)
                return NotFound();

            workplace.Phone = null;
            workplace.Printer = null;
            workplace.User = null;
            workplace.PC = null;
            workplace.Office = null;

            _db.Workplaces.Remove(workplace);
            _db.SaveChanges();

            return Ok(new { success = true });
        }

        // GET: api/view/pc/details/{id}
        [HttpGet("pc/details/{id}")]
        public IActionResult PcDetails(int id)
        {
            var pc = _db.PCs
                .Include(p => p.CurrentHardwareInfo)
                .FirstOrDefault(p => p.Id == id);

            if (pc == null)
                return NotFound();

            return Ok(new
            {
                pc.Id,
                pc.Hostname,
                pc.IP,
                pc.Domain,
                pc.Think,
                pc.Anydesk,
                HardwareInfo = pc.CurrentHardwareInfo != null ? new
                {
                    pc.CurrentHardwareInfo.ComputerName,
                    pc.CurrentHardwareInfo.ProcessorName,
                    pc.CurrentHardwareInfo.MonitorInfo,
                    pc.CurrentHardwareInfo.TotalMemoryGB,
                    pc.CurrentHardwareInfo.VideoCard,
                    pc.CurrentHardwareInfo.OSVersion,
                    pc.CurrentHardwareInfo.DiskInfo,
                    pc.CurrentHardwareInfo.DiskType,
                    pc.CurrentHardwareInfo.SerialNumber,
                    pc.CurrentHardwareInfo.TotalRamSlots,
                    pc.CurrentHardwareInfo.UsedRamSlots,
                    pc.CurrentHardwareInfo.RamType,
                    pc.CurrentHardwareInfo.RamManufacturer,
                    pc.CurrentHardwareInfo.IsDomainJoined,
                    pc.CurrentHardwareInfo.IpAddress,
                    pc.CurrentHardwareInfo.CollectedAtUtc,
                    pc.CurrentHardwareInfo.MotherboardModel,
                    pc.CurrentHardwareInfo.CurrentUserName,
                    pc.CurrentHardwareInfo.RamSpeed,
                    pc.CurrentHardwareInfo.DiskHealth,
                    pc.CurrentHardwareInfo.Antivirus,
                    pc.CurrentHardwareInfo.Uptime,
                    SoftwareList = pc.CurrentHardwareInfo.SoftwareList,
                    UsbDevices = pc.CurrentHardwareInfo.UsbDevices,
                    Printers = pc.CurrentHardwareInfo.Printers,
                    OpenPorts = pc.CurrentHardwareInfo.OpenPorts,
                    pc.CurrentHardwareInfo.PendingUpdatesCount,
                    pc.CurrentHardwareInfo.LastUpdateDate
                } : null
            });
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