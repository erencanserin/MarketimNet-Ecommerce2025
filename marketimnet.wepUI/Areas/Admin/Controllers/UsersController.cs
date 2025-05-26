using Microsoft.AspNetCore.Mvc;

namespace marketimnet.wepUI.Areas.Admin.Controllers
{
    public class UsersController : AdminBaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
} 