using Deckster.Server.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Deckster.Server.Controllers;

[Route("")]
public class HomeController : Controller
{
    [HttpGet("")]
    public object Index()
    {
        if (!HttpContext.TryGetUser(out _))
        {
            return RedirectToAction("login");
        }
        return View();
    }

    [HttpGet("login")]
    public ViewResult Login()
    {
        return View();
    }
}