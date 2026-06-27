using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DirWatcher.Models;
using DirWatcher.Services;

namespace DirWatcher.Controllers;

public class HomeController : Controller
{
    private readonly ChangeDetectionService _service;

    public HomeController(ChangeDetectionService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(string path)
    {
        ViewBag.Path = path;

        if (string.IsNullOrWhiteSpace(path))
        {
            ModelState.AddModelError(nameof(path), "Please enter a directory path.");
            return View();
        }

        try
        {
            var result = _service.Analyze(path);
            return View(result);
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or ArgumentException or UnauthorizedAccessException)
        {
            ModelState.AddModelError(nameof(path), ex.Message);
            return View();
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
