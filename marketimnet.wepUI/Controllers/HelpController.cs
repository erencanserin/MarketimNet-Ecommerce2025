using Microsoft.AspNetCore.Mvc;

namespace marketimnet.wepUI.Controllers
{
    public class HelpController : Controller
    {
        public IActionResult Faq()
        {
            return View();
        }

        public IActionResult ReturnPolicy()
        {
            return View();
        }
    }
} 