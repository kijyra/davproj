using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Authorization;

public class FilesController : Controller
{
    private readonly IWebHostEnvironment _hostingEnvironment;
    public FilesController(IWebHostEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
    }

    [Authorize(Roles = "IT_Full")]
    public IActionResult DownloadPrivateFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return NotFound();
        }
        string webRootPath = _hostingEnvironment.WebRootPath;
        string privateFolderPath = Path.Combine(webRootPath, "files", "private");
        string filePath = Path.Combine(privateFolderPath, fileName);
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }
        string contentType = "application/octet-stream";
        return File(System.IO.File.OpenRead(filePath), contentType, fileName);
    }
}
