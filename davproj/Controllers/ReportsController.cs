using Microsoft.AspNetCore.Mvc;

namespace davproj.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
