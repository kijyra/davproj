using davproj.Models;
using davproj.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace davproj.Controllers
{
    public class AdminController : Controller
    {
        private readonly DBContext _db;
        public AdminController(DBContext db)
        {
            _db = db;
        }
        [Authorize(Roles = "IT_Full")]
        public IActionResult ManageDatabase()
        {
            var viewModel = new ManageDBViewModel
            {
                ADUsers = _db.ADUsers.ToList(),
                Buildings = _db.Buildings.ToList(),
                Cartridges = _db.Cartridges.ToList(),
                Floors = _db.Floors.ToList(),
                Locations = _db.Locations.ToList(),
                Manufactors = _db.Manufactors.ToList(),
                Offices = _db.Offices.ToList(),
                PCs = _db.PCs.ToList(),
                Phones = _db.Phones.ToList(),
                PrinterModels = _db.PrinterModels.ToList(),
                Workplaces = _db.Workplaces.ToList(),
                Users = _db.Users.ToList(),
                Printers = _db.Printers.ToList(),
                HardwareInfo = _db.HardwareInfo.ToList()
            };
            return View(viewModel);
        }
        [Authorize(Roles = "IT_Full")]
        public IActionResult GetTableData(string tableName)
        {
            IEnumerable dataList = null;
            string viewName = "_DynamicTablePartial";
            switch (tableName)
            {
                case "ADUsers":
                    dataList = _db.ADUsers.Include(a => a.User).ToList();
                    break;
                case "Buildings":
                    dataList = _db.Buildings
                        .Include(b => b.Floors)
                        .Include(b => b.Location)
                        .ToList();
                    break;
                case "Cartridges":
                    dataList = _db.Cartridges
                        .Include(c => c.Manufactor)
                        .Include(c => c.PrinterModels)
                        .ToList();
                    break;
                case "Floors":
                    dataList = _db.Floors
                        .Include(f => f.Offices)
                        .Include(f => f.Building)
                        .ToList();
                    break;
                case "Locations":
                    dataList = _db.Locations.Include(l => l.Buildings).ToList();
                    break;
                case "Manufactors":
                    dataList = _db.Manufactors.Include(m => m.Cartridges).ToList();
                    break;
                case "Offices":
                    dataList = _db.Offices
                        .Include(o => o.Workplaces)
                        .Include(o => o.Floor)
                            .ThenInclude(f => f.Building)
                            .ThenInclude(b => b.Location)
                        .ToList();
                    break;
                case "PCs":
                    dataList = _db.PCs
                        .Include(p => p.Workplace)
                        .Include(p => p.HardwareHistory)
                        .Include(p => p.CurrentHardwareInfo)
                        .ToList();
                    break;
                case "Phones":
                    dataList = _db.Phones.Include(p => p.Workplace).ToList();
                    break;
                case "PrinterModels":
                    dataList = _db.PrinterModels
                        .Include(p => p.Cartridge)
                        .Include(p => p.Printers)
                        .ToList();
                    break;
                case "Workplaces":
                    dataList = _db.Workplaces
                        .Include(w => w.Printer)
                        .Include(w => w.PC)
                        .Include(w => w.Phone)
                        .Include(w => w.User)
                        .Include(w => w.Office)
                        .ToList();
                    break;
                case "Printers":
                    dataList = _db.Printers
                        .Include(p => p.PrinterModel)
                        .Include(p => p.Users)
                        .Include(p => p.Workplaces)
                        .ToList();
                    break;
                case "Users":
                    dataList = _db.Users
                        .Include(u => u.ADUser)
                        .Include(u => u.Printer)
                        .Include(u => u.Workplace)
                        .ToList();
                    break;
                case "HardwareInfo":
                    dataList = _db.HardwareInfo.ToList();
                    break;
            }
            if (dataList == null)
            {
                return NotFound($"Таблица '{tableName}' не найдена или не обрабатывается.");
            }
            return PartialView(viewName, dataList);
        }
    }
}
