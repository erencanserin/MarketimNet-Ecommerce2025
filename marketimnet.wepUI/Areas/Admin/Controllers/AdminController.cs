using Microsoft.AspNetCore.Mvc;
using marketimnet.wepUI.Areas.Admin.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace marketimnet.wepUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private const string AdminUsername = "admin";
        private const string AdminPassword = "Admin123!";

        public AdminController(ILogger<AdminController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Main", new { area = "Admin" });
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Username == AdminUsername && model.Password == AdminPassword)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.Username),
                        new Claim(ClaimTypes.Role, "Admin")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(3)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    _logger.LogInformation("Admin girişi başarılı: {Username}", model.Username);
                    return RedirectToAction("Index", "Main", new { area = "Admin" });
                }

                ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı");
                _logger.LogWarning("Başarısız admin girişi denemesi: {Username}", model.Username);
            }

            return View("Index", model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
} 