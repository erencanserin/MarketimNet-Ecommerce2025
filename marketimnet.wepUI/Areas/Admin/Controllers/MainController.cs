using Microsoft.AspNetCore.Mvc;
using marketimnet.Service.Abstract;
using marketimnet.wepUI.Areas.Admin.Filters;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace marketimnet.wepUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [ServiceFilter(typeof(AdminAuthFilter))]
    [Authorize]
    public class MainController : Controller
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MainController> _logger;

        public MainController(
            IUserService userService,
            IOrderService orderService,
            IProductService productService,
            ICategoryService categoryService,
            IConfiguration configuration,
            ILogger<MainController> logger)
        {
            _userService = userService;
            _orderService = orderService;
            _productService = productService;
            _categoryService = categoryService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Dashboard verileri
                ViewBag.TotalOrders = await _orderService.GetTotalOrderCountAsync();
                ViewBag.TotalProducts = await _productService.GetTotalProductCountAsync();
                ViewBag.TotalCategories = await _categoryService.GetTotalCategoryCountAsync();
                ViewBag.TotalUsers = await _userService.GetTotalUserCountAsync();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard verileri yüklenirken hata oluştu");
                TempData["Error"] = "Dashboard verileri yüklenirken bir hata oluştu.";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            try
            {
                if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Main", new { area = "Admin" });
                }

                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login sayfası yüklenirken hata oluştu");
                TempData["Error"] = "Bir hata oluştu. Lütfen tekrar deneyin.";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string returnUrl = null)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError("", "E-posta ve şifre alanları zorunludur");
                    return View();
                }

                var configEmail = _configuration["AdminCredentials:Email"];
                var configPassword = _configuration["AdminCredentials:Password"];

                if (string.IsNullOrEmpty(configEmail) || string.IsNullOrEmpty(configPassword))
                {
                    _logger.LogError("Admin kimlik bilgileri yapılandırmada bulunamadı");
                    ModelState.AddModelError("", "Sistem yapılandırma hatası");
                    return View();
                }

                if (email == configEmail && password == configPassword)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, email),
                        new Claim(ClaimTypes.Role, "Admin")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    _logger.LogInformation("Admin başarıyla giriş yaptı: {Email}", email);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Main", new { area = "Admin" });
                }

                _logger.LogWarning("Başarısız admin giriş denemesi: {Email}", email);
                ModelState.AddModelError("", "Geçersiz e-posta veya şifre");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login işlemi sırasında hata oluştu");
                ModelState.AddModelError("", "Giriş yapılırken bir hata oluştu. Lütfen tekrar deneyin.");
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                _logger.LogInformation("Admin başarıyla çıkış yaptı");
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout işlemi sırasında hata oluştu");
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalOrders = await _orderService.GetTotalOrderCountAsync();
            var totalProducts = await _productService.GetTotalProductCountAsync();
            var totalCategories = await _categoryService.GetTotalCategoryCountAsync();

            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalCategories = totalCategories;

            return View();
        }
    }
}
