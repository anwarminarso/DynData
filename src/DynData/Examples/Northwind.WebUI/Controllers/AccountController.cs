using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;


namespace Northwind.WebUI.Controllers;

[Route("[controller]/[action]")]
public class AccountController : Controller
{
    private readonly ILogger _logger;

    public AccountController(ILogger<AccountController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        _logger.LogInformation("User {Name} logged out at {Time}.",
            User.Identity.Name, DateTime.UtcNow);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToPage("/Account/SignedOut");
    }
}