using davproj.Models;
using Microsoft.EntityFrameworkCore;
using davproj.Models;

namespace davproj.Services
{
    public class UserSettingsService
    {
        private readonly DBContext _db;
        public UserSettingsService(DBContext db) => _db = db;

        public UserSettings? GetFor(string? fullWinName)
        {
            if (string.IsNullOrEmpty(fullWinName)) return null;

            string shortName = fullWinName.Contains('\\')
                ? fullWinName.Split('\\')[1]
                : fullWinName;

            var settings = _db.ADUsers
                .AsNoTracking()
                .FirstOrDefault(u => u.Cn == shortName)?
                .Settings; 

            return settings;
        }
    }
}
