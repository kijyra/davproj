using davproj.Models;
using HardwareShared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace davproj.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HardwareController : ControllerBase
    {
        private readonly ILogger<HardwareController> _logger;
        public HardwareController(ILogger<HardwareController> logger)
        {
            _logger = logger;
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] HardwareInfo info, [FromServices] DBContext db)
        {
            if (info == null) return BadRequest("Данные не получены");
            var pc = await db.PCs.FirstOrDefaultAsync(p => p.Hostname == info.ComputerName);
            if (pc == null)
            {
                pc = new PC { Hostname = info.ComputerName, IP = info.IpAddress, Domain = info.IsDomainJoined };
                db.PCs.Add(pc);
            }
            else
            {
                pc.IP = info.IpAddress;
                pc.Domain = info.IsDomainJoined;
            }
            await db.SaveChangesAsync();
            var collectedAt = DateTime.UtcNow;
            await db.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO ""HardwareInfo"" (
                ""ComputerName"", ""ProcessorName"", ""MonitorInfo"", ""TotalMemoryGB"", 
                ""VideoCard"", ""OSVersion"", ""DiskInfo"", ""DiskType"", ""SerialNumber"", 
                ""TotalRamSlots"", ""UsedRamSlots"", ""RamType"", ""RamManufacturer"", 
                ""IsDomainJoined"", ""IpAddress"", ""CollectedAtUtc"", ""PCId""
                ) VALUES (
                {info.ComputerName}, {info.ProcessorName}, {info.MonitorInfo}, {info.TotalMemoryGB}, 
                {info.VideoCard}, {info.OSVersion}, {info.DiskInfo}, {info.DiskType}, {info.SerialNumber}, 
                {info.TotalRamSlots}, {info.UsedRamSlots}, {info.RamType}, {info.RamManufacturer}, 
                {info.IsDomainJoined}, {info.IpAddress}, {collectedAt}, {pc.Id}
            )");
            var newInfo = await db.HardwareInfo
                .Where(h => h.ComputerName == info.ComputerName)
                .OrderByDescending(h => h.CollectedAtUtc)
                .FirstOrDefaultAsync();
            if (newInfo != null)
            {
                pc.CurrentHardwareInfoId = newInfo.Id;
                await db.SaveChangesAsync();
            }
            return Ok(new { message = "Данные успешно добавлены через прямой SQL" });
        }
    }
}
