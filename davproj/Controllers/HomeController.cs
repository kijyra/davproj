using davproj.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;

namespace davproj.Controllers
{
    public class HomeController : Controller
    {
        private readonly DBContext _db;
        private readonly IWebHostEnvironment _env;
        public HomeController(DBContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }
        [AllowAnonymous]
        public IActionResult Index()
        {
            var filesFolderPath = Path.Combine(_env.WebRootPath, "files");
            var rootItems = new List<FileSystemItemViewModel>();
            var directoryMap = new Dictionary<string, FileSystemItemViewModel>();
            if (Directory.Exists(filesFolderPath))
            {
                var filesInDirectory = Directory.GetFiles(filesFolderPath, "*.*", SearchOption.AllDirectories);
                foreach (var filePath in filesInDirectory)
                {
                    var relativePathFromFilesFolder = Path.GetRelativePath(filesFolderPath, filePath);
                    var pathParts = relativePathFromFilesFolder.Split(Path.DirectorySeparatorChar);
                    if (pathParts.Any(part => part.Equals("private", StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }
                    var fileInfo = new FileInfo(filePath);
                    var relativePathFromRoot = Path.GetRelativePath(_env.WebRootPath, filePath).Replace("\\", "/");
                    var parentDirectoryPath = Path.GetDirectoryName(relativePathFromFilesFolder);
                    if (!string.IsNullOrEmpty(parentDirectoryPath) && parentDirectoryPath != ".")
                    {
                        if (!directoryMap.ContainsKey(parentDirectoryPath))
                        {
                            CreateDirectoryHierarchy(filesFolderPath, parentDirectoryPath, directoryMap, rootItems);
                        }
                    }
                    List<FileSystemItemViewModel> targetList;
                    if (string.IsNullOrEmpty(parentDirectoryPath) || parentDirectoryPath == ".")
                    {
                        targetList = rootItems;
                    }
                    else
                    {
                        targetList = directoryMap[parentDirectoryPath].Children;
                    }
                    var fileSize = "";
                    if ((fileInfo.Length / 1024f) > 10048f)
                    { fileSize = (fileInfo.Length / 1024f / 1024f).ToString("F2") + " MB"; }
                    else if (fileInfo.Length > 10048f)
                    { fileSize = (fileInfo.Length / 1024f).ToString("F2") + " KB"; }
                    else
                    { fileSize = (fileInfo.Length).ToString("F2") + " B"; }    
                    targetList.Add(new FileSystemItemViewModel
                    {
                        Name = fileInfo.Name,
                        Type = "File",
                        RelativePath = $"/{relativePathFromRoot}",
                        FormattedSize = fileSize,
                        FileExtension = fileInfo.Extension.ToLower()
                    });
                }
            }
            return View(rootItems);
        }
        private void CreateDirectoryHierarchy(string rootPath, string relativePath, Dictionary<string, FileSystemItemViewModel> map, List<FileSystemItemViewModel> rootList)
        {
            var currentPath = string.Empty;
            var parts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                var parentPath = currentPath;
                currentPath = Path.Combine(currentPath, part);
                if (!map.ContainsKey(currentPath))
                {
                    var newFolder = new FileSystemItemViewModel
                    {
                        Name = part,
                        Type = "Directory"
                    };
                    map.Add(currentPath, newFolder);
                    if (string.IsNullOrEmpty(parentPath))
                    {
                        rootList.Add(newFolder);
                    }
                    else
                    {
                        map[parentPath].Children.Add(newFolder);
                    }
                }
            }
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [Authorize]
        public IActionResult Settings()
        {
            ADUser adUser = new ADUser();
            adUser.Group = new List<string>();
            var users = _db.ADUsers.ToList();
            bool isAlready = false;
            foreach (ADUser u in users)
            {
                if (u.Cn == User?.Identity?.Name?.Split('\\').Last()) 
                { 
                    isAlready = true;
                    adUser = u; 
                    break; 
                }
                else { isAlready =  false;}
            }
            if (!isAlready)
            {
                adUser = UpdateADUser(User?.Identity?.Name ?? string.Empty);
                _db.ADUsers.AddRange(adUser);
                _db.SaveChanges();
            }
            return View(adUser);
        }
        [Authorize]
        [HttpGet]
        public IActionResult UserSettings()
        {
            string shortName = (User?.Identity?.Name?.Split('\\').Last() ?? string.Empty);
            var adUser = _db.ADUsers.FirstOrDefault(u => u.Cn == shortName);
            return PartialView(adUser?.Settings ?? new UserSettings());
        }
        [Authorize]
        [HttpPost]
        public IActionResult UserSettings(UserSettings userSettings)
        {
            if (ModelState.IsValid)
            {
                string? fullWinName = (User?.Identity?.Name ?? string.Empty);
                if (string.IsNullOrEmpty(fullWinName))
                {
                    return Json(new { success = false, message = "Пользователь Windows не определен." });
                }
                string shortName = fullWinName.Contains('\\')
                    ? fullWinName.Split('\\').Last()
                    : fullWinName;
                var adUser = _db.ADUsers
                    .FirstOrDefault(u => u.Cn == shortName);
                if (adUser == null)
                {
                    return Json(new { success = false, message = "Пользователь не найден в БД." });
                }
                adUser.Settings = userSettings;
                _db.SaveChanges();
                return Json(new { success = true, message = "Настройки сохранены." });
            }
            return PartialView(userSettings);
        }
        [Authorize]
        static ADUser UpdateADUser(string IdentityName)
        {
            string domainName = IdentityName.Split('\\').First();
            string userName = IdentityName.Split('\\').Last();
            PrincipalContext context = new PrincipalContext(ContextType.Domain, domainName);
            UserPrincipal user = UserPrincipal.FindByIdentity(context, userName);
            if (user != null)
            {
                List<string> ad = new List<string>();
                var groups = user.GetAuthorizationGroups();
                List<string> roles = new List<string>();
                foreach (GroupPrincipal group in groups)
                {
                    ad.Add(group.Name);
                }
                ADUser aduser = new ADUser
                {
                    Name = user.GivenName,
                    SurName = user.Surname,
                    Cn = user.SamAccountName,
                    GivenName = user.DisplayName,
                    Admin = ad.Contains("Администраторы"),
                    Group = ad,
                    Enabled = user.Enabled,
                };
                aduser.InitializeDefaultSettings();
                return aduser;
            }
            else { return new ADUser(); }
        }
    }
}