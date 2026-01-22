using davproj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace davproj.Controllers
{
    public class GeoController : Controller
    {
        private readonly DBContext _db;
        public GeoController(DBContext db)
        {
            _db = db;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public IActionResult LocationAdd()
        {
            ViewData["FormAction"] = "LocationAdd";
            return PartialView("Location");
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public IActionResult LocationAdd(Location location)
        {
            if (ModelState.IsValid)
            {
                _db.Locations.Add(location);
                _db.SaveChanges();
                return Json(new { success = true, location = new { id = location.Id, title = location.Name } });
            }
            ViewData["FormAction"] = "LocationAdd";
            return PartialView("Location", location);
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public ActionResult LocationEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Location location = _db.Locations.Find(id);
            ViewData["FormAction"] = "LocationEdit";
            if (location != null)
            {
                return PartialView("Location", location);
            }
            return NotFound();
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public IActionResult LocationEdit(Location location)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(location).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _db.SaveChanges();
                return Json(new { success = true, location = new { id = location.Id, title = location.Name } });
            }
            ViewData["FormAction"] = "LocationEdit";
            return PartialView("Location", location);
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public ActionResult LocationDelete(int id)
        {
            if (id == null) { return NotFound(); }
            var location = _db.Locations.Find(id);
            if (location == null) { return NotFound(); }
            if (location.Buildings != null)
            {
                foreach (var building in location.Buildings)
                {
                    building.Location = null;
                }
                location.Buildings.Clear();
            }
            _db.Locations.Remove(location);
            _db.SaveChanges();
            return Json(new { success = true });
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public IActionResult BuildingAdd()
        {
            ViewData["locations"] = _db.Locations.ToList();
            ViewData["FormAction"] = "BuildingAdd";
            return PartialView("Building");
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public IActionResult BuildingAdd(Building building)
        {
            if (ModelState.IsValid)
            {
                _db.Buildings.Add(building);
                _db.SaveChanges();
                return Json(new { success = true, building = new { id = building.Id, title = building.Name } });
            }
            ViewData["FormAction"] = "BuildingAdd";
            ViewData["locations"] = _db.Locations.ToList();
            return PartialView("Building", building);
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public ActionResult BuildingEdit(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }
            ViewData["locations"] = _db.Locations.ToList();
            ViewData["FormAction"] = "BuildingEdit";
            Building building = _db.Buildings.Find(id);
            if (building != null)
            {
                return PartialView("Building", building);
            }
            return NotFound();
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public IActionResult BuildingEdit(Building building)
        {
            ViewData["locations"] = _db.Locations.ToList();
            if (ModelState.IsValid)
            {
                _db.Entry(building).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _db.SaveChanges();
                return Json(new { success = true, building = new { id = building.Id, title = building.Name } });
            }
            ViewData["FormAction"] = "BuildingEdit";
            return PartialView("Building", building);
        }
        [Authorize(Roles = "IT_Full")]
        public ActionResult BuildingDelete(int id)
        {
            if (id == null) { return NotFound(); }
            var building = _db.Buildings.Find(id);
            if (building == null) { return NotFound(); }
            _db.Buildings.Remove(building);
            _db.SaveChanges();
            return Json(new { success = true });
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public IActionResult FloorAdd()
        {
            ViewData["buildings"] = _db.Buildings.ToList();
            ViewData["FormAction"] = "FloorAdd";
            return PartialView("Floor");
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public IActionResult FloorAdd(Floor floor)
        {
            if (ModelState.IsValid)
            {
                _db.Floors.Add(floor);
                _db.SaveChanges();
                return Json(new { success = true, floor = new { id = floor.Id, title = floor.FloorNum } });
            }
            ViewData["FormAction"] = "FloorAdd";
            ViewData["buildings"] = _db.Buildings.ToList();
            return PartialView("Floor",floor);
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public ActionResult FloorEdit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewData["buildings"] = _db.Buildings.ToList();
            ViewData["FormAction"] = "FloorEdit";
            Floor floor = _db.Floors.Find(id);
            if (floor != null)
            {
                return PartialView("Floor", floor);
            }
            return NotFound();
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public IActionResult FloorEdit(Floor floor)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(floor).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _db.SaveChanges();
                return Json(new { success = true, floor = new { id = floor.Id, title = floor.FloorNum } });
            }
            ViewData["FormAction"] = "FloorEdit";
            ViewData["buildings"] = _db.Buildings.ToList();
            return PartialView("Floor", floor);
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public ActionResult FloorDelete(int id)
        {
            if (id == null) { return NotFound(); }
            var floor = _db.Floors.Find(id);
            if (floor == null) { return NotFound(); }
            _db.Floors.Remove(floor);
            _db.SaveChanges();
            return Json(new { success = true });
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public IActionResult OfficeAdd()
        {
            ViewData["floors"] = _db.Floors
                .Include(f => f.Building)
                .OrderBy(m => m.Id)
                .ToList();
            ViewData["FormAction"] = "OfficeAdd";
            return PartialView("Office");
        }
        [HttpPost]
        [Authorize(Roles = "IT_Full")]
        public IActionResult OfficeAdd(Office office)
        {
            if (ModelState.IsValid)
            {
                _db.Offices.Add(office);
                _db.SaveChanges();
                return Json(new { success = true, office = new { id = office.Id, title = office.FullTitle } });
            }
            ViewData["FormAction"] = "OfficeAdd";
            ViewData["floors"] = _db.Floors
                .Include(f => f.Building)
                .OrderBy(m => m.Id)
                .ToList();
            return PartialView("Office", office);
        }
        [Authorize(Roles = "IT_Full")]
        [HttpGet]
        public IActionResult OfficeEdit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewData["FormAction"] = "OfficeEdit";
            ViewData["floors"] = _db.Floors.ToList();
            Office office = _db.Offices.Find(id);
            if (office != null)
            {
                return PartialView("Office", office);
            }
            return NotFound();
        }
        [Authorize(Roles = "IT_Full")]
        [HttpPost]
        public IActionResult OfficeEdit(Office office)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(office).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _db.SaveChanges();
                return Json(new { success = true, office = new { id = office.Id, title = office.FullTitle } });
            }
            ViewData["FormAction"] = "OfficeEdit";
            ViewData["floors"] = _db.Floors
                .Include(f => f.Building)
                .OrderBy(m => m.Id)
                .ToList();
            return PartialView("Office", office); 
        }
        [HttpPost]
        [Authorize(Roles = "IT_Full")]
        public ActionResult OfficeDelete(int id)
        {
            if (id == null) { return NotFound(); }
            var office = _db.Offices.Find(id);
            if (office == null) { return NotFound(); }
            _db.Offices.Remove(office);
            _db.SaveChanges();
            return Json(new { success = true });
        }
    }
}
