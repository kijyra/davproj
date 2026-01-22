using davproj.Models;
using HardwareShared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.DirectoryServices.ActiveDirectory;
using System.Net;

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
            var pc = await db.PCs
                .FirstOrDefaultAsync(p => p.Hostname == info.ComputerName);
            if (pc == null)
            {
                _logger.LogInformation("Регистрация нового ПК в базе: {Name}", info.ComputerName);
                pc = new PC
                {
                    Hostname = info.ComputerName,
                    IP = info.IpAddress,
                    Domain = info.IsDomainJoined,
                    Think = false
                };
                db.PCs.Add(pc);
                await db.SaveChangesAsync();
            }
            else
            {
                _logger.LogInformation("Обновление данных для существующего ПК: {Name}", info.ComputerName);
                pc.IP = info.IpAddress;
                pc.Domain = info.IsDomainJoined;
                pc.Think = false;
                db.Entry(pc).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await db.SaveChangesAsync();
            }

            info.Id = 0;
            info.CollectedAtUtc = DateTime.UtcNow;
            db.HardwareInfo.Add(info);
            db.Entry(info).Property("PCId").CurrentValue = pc.Id;
            await db.SaveChangesAsync();
            pc.CurrentHardwareInfoId = info.Id;
            await db.SaveChangesAsync();

            return Ok(new { message = "Данные успешно добавлены в историю и привязаны к ПК" });
        }

    }
}
