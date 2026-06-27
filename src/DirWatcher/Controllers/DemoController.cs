using Microsoft.AspNetCore.Mvc;
using DirWatcher.Services;

namespace DirWatcher.Controllers;

public class DemoController : Controller
{
    private readonly DemoFolderService _demo;
    private readonly ISnapshotStore _store;

    public DemoController(DemoFolderService demo, ISnapshotStore store)
    {
        _demo = demo;
        _store = store;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Apply()
    {
        _demo.ApplyChanges();
        ViewBag.Path = _demo.FolderPath;
        ViewBag.DemoMessage = "Applied demo changes (modified a.txt, added d.txt and sub2/, removed b.txt). "
                            + "Click Analyze to detect them.";
        return View("~/Views/Home/Index.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reset()
    {
        _demo.Reset();
        _store.Delete(_demo.FolderPath);
        ViewBag.Path = _demo.FolderPath;
        ViewBag.DemoMessage = "Demo folder reset to baseline and history cleared. "
                            + "Click Analyze to record the new baseline.";
        return View("~/Views/Home/Index.cshtml");
    }
}
