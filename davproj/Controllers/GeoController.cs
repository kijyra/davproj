using davproj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace davproj.Controllers
{
    [Authorize(Roles = "IT_Full")]
    [Route("api/[controller]")]
    public class GeoController : Controller
    {
        private readonly DBContext _db;
        public GeoController(DBContext db)
        {
            _db = db;
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

        #region Location
        [HttpGet("locations")]
        public IActionResult Locations()
        {
            return Json(_db.Locations.ToList());
        }

        [HttpGet("location/{id}")]
        public IActionResult Location(int id)
        {
            return Json(_db.Locations.FirstOrDefault(x => x.Id == id));
        }
        [HttpPost]
            public IActionResult LocationAdd(Location location)
            {
                if (ModelState.IsValid)
                {
                    _db.Locations.Add(location);
                    _db.SaveChanges();
                    return Json(new { success = true, location = new { id = location.Id, title = location.Name } });
                }
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
            }
            [HttpPost("Location/Edit")]
            public IActionResult LocationEdit(Location location)
            {
                if (ModelState.IsValid)
                {
                    _db.Entry(location).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _db.SaveChanges();
                    return Json(new { success = true, location = new { id = location.Id, title = location.Name } });
                }
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, errors });
            }
            [HttpPost("Location/Delete")]
            public ActionResult LocationDelete(int id)
            {
                if (id is 0) { return NotFound(); }
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
        #endregion
        #region Building
        [HttpGet("buildings")]
        public IActionResult Buildings()
        {
            return Json(_db.Buildings.ToList());
        }

        [HttpGet("building/{id}")]
        public IActionResult Building(int id)
        {
            return Json(_db.Buildings.FirstOrDefault(x => x.Id == id));
        }
        [HttpPost("Building/Add")]
            public IActionResult BuildingAdd(Building building)
            {
                if (ModelState.IsValid)
                {
                    _db.Buildings.Add(building);
                    _db.SaveChanges();
                    return Json(new { success = true, building = new { id = building.Id, title = building.Name } });
                }
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
            }
            [HttpPost("Building/Edit")]
            public IActionResult BuildingEdit(Building building)
            {
                ViewData["locations"] = _db.Locations.ToList();
                if (ModelState.IsValid)
                {
                    _db.Entry(building).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _db.SaveChanges();
                    return Json(new { success = true, building = new { id = building.Id, title = building.Name } });
                }
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, errors });
            }
            [HttpPost("Building/Delete")]    
            public ActionResult BuildingDelete(int id)
            {
                if (id == 0) { return NotFound(); }
                var building = _db.Buildings.Find(id);
                if (building == null) { return NotFound(); }
                _db.Buildings.Remove(building);
                _db.SaveChanges();
                return Json(new { success = true });
            }
        #endregion
        #region Floor
        [HttpGet("floors")]
        public IActionResult Floors()
        {
            return Json(_db.Floors.ToList());
        }

        [HttpGet("floor/{id}")]
        public IActionResult Floor(int id)
        {
            return Json(_db.Floors.FirstOrDefault(x => x.Id == id));
        }
        [HttpPost("Floor/Add")]
            public IActionResult FloorAdd(Floor floor)
            {
                if (ModelState.IsValid)
                {
                    _db.Floors.Add(floor);
                    _db.SaveChanges();
                    return Json(new { success = true, floor = new { id = floor.Id, title = floor.FloorNum } });
                }
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
            }
           
            [HttpPost("Floor/Edit")]
            public IActionResult FloorEdit(Floor floor)
            {
                if (ModelState.IsValid)
                {
                    _db.Entry(floor).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _db.SaveChanges();
                    return Json(new { success = true, floor = new { id = floor.Id, title = floor.FloorNum } });
                }
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
            }
            [HttpPost("Floor/Delete")]
            public ActionResult FloorDelete(int id)
            {
                if (id == 0) { return NotFound(); }
                var floor = _db.Floors.Find(id);
                if (floor == null) { return NotFound(); }
                _db.Floors.Remove(floor);
                _db.SaveChanges();
                return Json(new { success = true });
            }
        #endregion
        #region Office
        [HttpGet("Offices")]
        public IActionResult Offices()
        {
            return Json(_db.Offices.ToList());
        }

        [HttpGet("office/{id}")]
        public IActionResult Office(int id)
        {
            return Json(_db.Offices.FirstOrDefault(x => x.Id == id));
        }
        [HttpPost("Office/Add")]
            public IActionResult OfficeAdd(Office office)
            {
                if (ModelState.IsValid)
                {
                    _db.Offices.Add(office);
                    _db.SaveChanges();
                    return Json(new { success = true, office = new { id = office.Id, title = office.FullTitle } });
                }
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
            }
            [HttpPost("Office/Edit")]
            public IActionResult OfficeEdit(Office office)
            {
                if (ModelState.IsValid)
                {
                    _db.Entry(office).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _db.SaveChanges();
                    return Json(new { success = true, office = new { id = office.Id, title = office.FullTitle } });
                }
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, errors });
        }
            [HttpPost("Office/Delete")]
            public ActionResult OfficeDelete(int id)
            {
                if (id == 0) { return NotFound(); }
                var office = _db.Offices.Find(id);
                if (office == null) { return NotFound(); }
                _db.Offices.Remove(office);
                _db.SaveChanges();
                return Json(new { success = true });
            }
        #endregion
    }
}
