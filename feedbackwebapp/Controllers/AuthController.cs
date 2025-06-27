using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FeedbackWebApp.Controllers;

[Route("[controller]")]
public class AuthController : Controller
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] string password, [FromForm] string? returnUrl = null)
    {
        // Check if we're in mock mode (bypass auth)
        var useMocks = _configuration.GetValue<bool>("FeedbackApi:UseMocks");
        if (useMocks)
        {
            await SignInUser("MockUser");
            return RedirectToLocal(returnUrl);
        }

        // Get the expected password from configuration
        var expectedPassword = _configuration["FeedbackApp:AccessPassword"];
        
        if (string.IsNullOrEmpty(expectedPassword))
        {
            // If no password is configured, allow access (development scenario)
            await SignInUser("DevelopmentUser");
            return RedirectToLocal(returnUrl);
        }

        // Validate the provided password against the configured password
        if (password == expectedPassword)
        {
            await SignInUser("AuthenticatedUser");
            return RedirectToLocal(returnUrl);
        }

        // Invalid password
        TempData["ErrorMessage"] = "Invalid password";
        return RedirectToAction("LoginPage", new { returnUrl });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }

    [HttpGet("login")]
    public IActionResult LoginPage(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["ErrorMessage"] = TempData["ErrorMessage"];
        return View();
    }

    [HttpGet("check")]
    public IActionResult CheckAuth()
    {
        return Json(new { isAuthenticated = User.Identity?.IsAuthenticated ?? false });
    }

    private async Task SignInUser(string userName)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.Role, "User")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return Redirect("/");
    }
}
