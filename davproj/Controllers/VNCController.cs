using davproj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Connect(int pcId, bool fullControl)
        {
            var pc = await _dBContext.PCs.FindAsync(pcId);

            if (pcId == 0 || pc is null)
            {
                return NotFound("Устройство не найдено в базе данных");
            }

            if (string.IsNullOrEmpty(pc.IP))
            {
                return BadRequest("У устройства отсутствует IP-адрес");
            }

            var vncRequest = new
            {
                AdminName = User.Identity?.Name?.Split('\\').LastOrDefault() ?? "Admin",
                IsFullControl = fullControl
            };
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(120);
            try
            {
                var response = await client.PostAsJsonAsync($"http://{pc.IP}:5005/vnc/request", vncRequest);
                if (response.IsSuccessStatusCode)
                {
                    return Ok(new { success = true, message = "Запрос одобрен пользователем, VNC запускается..." });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return BadRequest("Пользователь отклонил запрос на подключение");
                }
                return StatusCode((int)response.StatusCode, "Агент вернул ошибку");
            }
            catch (HttpRequestException)
            {
                return StatusCode(504, "Не удалось связаться с агентом (возможно, ПК выключен или порт 5005 закрыт)");
            }
        }
    }
}
