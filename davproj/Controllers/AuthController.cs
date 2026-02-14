using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace davproj.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [Authorize(Roles = "Пользователи домена")]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var user = new
            {
                UserName = User.Identity?.Name,
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                AuthType = User.Identity?.AuthenticationType
            };

            return Ok(user);
        }
    }
}
