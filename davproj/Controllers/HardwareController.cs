using davproj.Models;
using HardwareShared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

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

            var reportCount = await db.HardwareInfo.CountAsync(h => h.PCId == pc.Id);

            if (reportCount >= 30)
            {
                var oldReports = db.HardwareInfo
                    .Where(h => h.PCId == pc.Id)
                    .OrderBy(h => h.CollectedAtUtc)
                    .Take(reportCount - 29);

                db.HardwareInfo.RemoveRange(oldReports);
                await db.SaveChangesAsync();
            }

            var softwareList = info.SoftwareList ?? new List<string>();
            var usbDevices = info.UsbDevices ?? new List<string>();
            var printers = info.Printers ?? new List<string>();
            var openPorts = info.OpenPorts ?? new List<string>();

            var collectedAt = DateTime.UtcNow;

            await db.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO ""HardwareInfo"" (
                ""ComputerName"", ""ProcessorName"", ""MonitorInfo"", ""TotalMemoryGB"", 
                ""VideoCard"", ""OSVersion"", ""DiskInfo"", ""DiskType"", ""SerialNumber"", 
                ""TotalRamSlots"", ""UsedRamSlots"", ""RamType"", ""RamManufacturer"", 
                ""IsDomainJoined"", ""IpAddress"", ""CollectedAtUtc"", ""PCId"",
                ""CurrentUserName"", ""RamSpeed"", ""DiskHealth"", ""Antivirus"", ""Uptime"",
                ""SoftwareList"", ""UsbDevices"", ""Printers"", ""OpenPorts"",
                ""PendingUpdatesCount"", ""LastUpdateDate""
            ) VALUES (
                {info.ComputerName}, {info.ProcessorName}, {info.MonitorInfo}, {info.TotalMemoryGB}, 
                {info.VideoCard}, {info.OSVersion}, {info.DiskInfo}, {info.DiskType}, {info.SerialNumber}, 
                {info.TotalRamSlots}, {info.UsedRamSlots}, {info.RamType}, {info.RamManufacturer}, 
                {info.IsDomainJoined}, {info.IpAddress}, {collectedAt}, {pc.Id},
                {info.CurrentUserName}, {info.RamSpeed}, {info.DiskHealth}, {info.Antivirus}, {info.Uptime},
                {softwareList}, 
                {usbDevices},
                {printers},
                {openPorts},
                {info.PendingUpdatesCount}, {info.LastUpdateDate}
            )");

            var newInfo = await db.HardwareInfo
                .Where(h => h.PCId == pc.Id)
                .OrderByDescending(h => h.CollectedAtUtc)
                .FirstOrDefaultAsync();

            if (newInfo != null)
            {
                pc.CurrentHardwareInfoId = newInfo.Id;
                await db.SaveChangesAsync();
            }

            return Ok(new { message = "Данные успешно добавлены через SQL" });
        }

        [HttpPost("request-update/{hostname}")]
        public async Task<IActionResult> RequestUpdate(string hostname, [FromServices] DBContext db)
        {
            var pc = await db.PCs.FirstOrDefaultAsync(p => p.Hostname == hostname);
            if (pc == null || string.IsNullOrEmpty(pc.IP))
                return NotFound($"Компьютер {hostname} не найден или у него нет IP адреса");

            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                var agentUrl = $"http://{pc.IP}:5005/hardware/collect";

                var response = await httpClient.PostAsync(agentUrl, null);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Команда на сбор данных успешно отправлена для {hostname} ({pc.IP})");
                    return Ok(new { message = "Запрос отправлен, агент начал сбор данных" });
                }

                return StatusCode((int)response.StatusCode, "Агент вернул ошибку: " + response.ReasonPhrase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Не удалось связаться с агентом на {hostname}");
                return StatusCode(503, "Агент недоступен (выключен или заблокирован брандмауэром)");
            }
        }
    }
}
