using Microsoft.AspNetCore.Mvc;

namespace SpoMusic.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index([FromQuery] string? code, [FromQuery] string? error)
    {
        // Forward code or error query params if user lands on root /
        if (!string.IsNullOrEmpty(code) || !string.IsNullOrEmpty(error))
        {
            return RedirectToAction("Callback", "Auth", new { code, error });
        }

        return View();
    }
}
