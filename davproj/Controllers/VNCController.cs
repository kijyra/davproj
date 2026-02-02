using davproj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace davproj.Controllers
{
    [Authorize(Roles = "IT_Full")]
    public class VNCController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DBContext _dBContext;

        public VNCController(IHttpClientFactory httpClientFactory, DBContext dBContext)
        {
            _httpClientFactory = httpClientFactory;
            _dBContext = dBContext;
        }

        [HttpPost]
        public async Task<IActionResult> Connect(int pcId, bool fullControl, bool requestUser = true)
        {
            var pc = await _dBContext.PCs.FindAsync(pcId);
            if (pc == null) return NotFound(new { message = "Устройство не найдено" });
            if (string.IsNullOrEmpty(pc.IP)) return BadRequest(new { message = "У ПК нет IP-адреса" });

            var mode = fullControl ? "control" : "view";
            var vncUri = $"vnc://{pc.IP}/?mode={mode}";

            if (requestUser)
            {
                try
                {
                    var vncRequest = new
                    {
                        AdminName = User.Identity?.Name?.Split('\\').LastOrDefault() ?? "Admin",
                        IsFullControl = fullControl
                    };

                    var client = _httpClientFactory.CreateClient();
                    client.Timeout = TimeSpan.FromSeconds(60);

                    var response = await client.PostAsJsonAsync($"http://{pc.IP}:5005/vnc/request", vncRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(new { success = true, message = "Доступ разрешен пользователем", uri = vncUri });
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        return BadRequest(new { message = "Пользователь ОТКЛОНИЛ запрос на подключение" });
                    }

                    return StatusCode((int)response.StatusCode, new { message = "Агент вернул ошибку или истекло время ожидания" });
                }
                catch (HttpRequestException)
                {
                    return StatusCode(504, new { message = "ПК недоступен: проверьте сеть или работу агента" });
                }
            }
            return Ok(new { success = true, message = "Подключение в режиме bypass", uri = vncUri });

        }
    }
}
