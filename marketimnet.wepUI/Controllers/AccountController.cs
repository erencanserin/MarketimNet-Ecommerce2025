using Microsoft.AspNetCore.Mvc;
using marketimnet.Service.Abstract;
using marketimnet.Core.Entities;
using marketimnet.wepUI.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace marketimnet.wepUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserService userService, ILogger<AccountController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // E-posta adresi kullanılıyor mu kontrol et
                    if (await _userService.AnyAsync(u => u.Email == model.Email))
                    {
                        ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor.");
                        return View(model);
                    }

                    // Yeni kullanıcı oluştur
                    var user = new AppUser
                    {
                        Name = model.Name,
                        Surname = model.Surname,
                        Email = model.Email,
                        Phone = model.Phone,
                        Password = model.Password, // UserService'de hash'lenecek
                        IsActive = true,
                        IsAdmin = false,
                        UserGuid = Guid.NewGuid(),
                        CreatedDate = DateTime.Now
                    };

                    // Kullanıcıyı kaydet
                    await _userService.AddAsync(user);

                    // Başarılı kayıt mesajını TempData'ya ekle
                    TempData["SuccessMessage"] = "Kaydınız başarıyla gerçekleşti! Şimdi giriş yapabilirsiniz.";

                    // Login sayfasına yönlendir
                    return RedirectToAction(nameof(Login));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kullanıcı kaydı sırasında hata oluştu");
                    ModelState.AddModelError("", "Kayıt işlemi sırasında bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }

            return View(model);
        }
    }
} 