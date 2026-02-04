using ClosedXML.Excel;
using davproj.Models;
using davproj.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

[Authorize(Roles = "IT_Full")]
public class ReportsController : Controller
{
    private readonly DBContext _db;
    private readonly IExcelService _excelService;

    public ReportsController(DBContext context, IExcelService excelService)
    {
        _db = context;
        _excelService = excelService;
    }

    private async Task<List<Building>> GetBuildingsDataAsync()
    {
        return await _db.Buildings
            .Include(b => b.Floors!).ThenInclude(f => f.Offices!)
                .ThenInclude(o => o.Workplaces!).ThenInclude(w => w.User!)
            .Include(b => b.Floors!).ThenInclude(f => f.Offices!)
                .ThenInclude(o => o.Workplaces!).ThenInclude(w => w.PC!)
                .ThenInclude(p => p.CurrentHardwareInfo)
            .Include(b => b.Location)
            .ToListAsync();
    }

    [HttpGet]
    public async Task<IActionResult> ExportMonitors()
    {
        var buildings = await GetBuildingsDataAsync();
        var content = _excelService.GetMonitorsReport(buildings);
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Monitors.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportFullInventory()
    {
        var buildings = await GetBuildingsDataAsync();
        var content = _excelService.GetFullReport(buildings);
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Monitors.xlsx");
    }

    public async Task<IActionResult> Index()
    {
        return View();
    }
}
    