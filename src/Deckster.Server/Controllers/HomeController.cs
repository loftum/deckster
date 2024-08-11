using System.Security.Claims;
using System.Text.Json.Serialization;
using Deckster.Server.Authentication;
using Deckster.Server.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Deckster.Server.Controllers;

[Route("")]
public class HomeController : Controller
{
    private readonly IUserRepo _repo;

    public HomeController(IUserRepo repo)
    {
        _repo = repo;
    }

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

    [HttpPost("login")]
    public async Task<object> Login([FromBody] LoginModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Username))
        {
            return StatusCode(400, new ResponseMessage("Username must be specified"));
        }

        if (string.IsNullOrWhiteSpace(input.Password))
        {
            return StatusCode(400, new ResponseMessage("Password must be specified"));
        }
        
        var user = await _repo.GetByUsernameAsync(input.Username, HttpContext.RequestAborted);
        if (user == null)
        {
            user = new User
            {
                Name = input.Username,
                Password = input.Password,
                AccessToken = string.Join("", Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N"))  
            };
            await _repo.SaveAsync(user);
        }

        if (input.Password != user.Password)
        {
            return StatusCode(400, new ResponseMessage("Invalid credentials"));
        }

        var identity = new ClaimsIdentity(new []
        {
            new Claim("sub", user.Id.ToString())
        }, AuthenticationSchemes.Cookie);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(AuthenticationSchemes.Cookie, principal, new AuthenticationProperties
        {
            
        });

        return StatusCode(200, new ResponseMessage("OK"));
    }
}

public class LoginModel
{
    [JsonPropertyName("username")]
    public string Username { get; init; }
    [JsonPropertyName("password")]
    public string Password { get; init; }
}