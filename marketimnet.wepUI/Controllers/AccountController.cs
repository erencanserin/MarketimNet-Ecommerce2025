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
                    var user = new AppUser
                    {
                        Name = model.Name,
                        Surname = model.Surname,
                        Email = model.Email,
                        Password = model.Password,
                        Phone = model.Phone,
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    };

                    await _userService.AddAsync(user);
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kullanıcı kaydı sırasında hata oluştu");
                    ModelState.AddModelError("", "Kayıt işlemi sırasında bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.");
                }
            }
            return View(model);
        }
    }
} 