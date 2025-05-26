using marketimnet.wepUI.Areas.Admin.Filters;
using Microsoft.AspNetCore.Mvc;

namespace marketimnet.wepUI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [ServiceFilter(typeof(AdminAuthFilter))]
    public abstract class AdminBaseController : Controller
    {
    }
} 