using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace marketimnet.wepUI.Areas.Admin.Filters
{
    public class AdminAuthFilter : IAuthorizationFilter
    {
        private readonly IConfiguration _configuration;

        public AdminAuthFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var isAdminController = context.RouteData.Values["controller"].ToString() == "Admin";
            var isIndexAction = context.RouteData.Values["action"].ToString() == "Index";
            
            // Eğer kullanıcı giriş yapmamışsa ve Admin/Index (login sayfası) değilse, login sayfasına yönlendir
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                if (!(isAdminController && isIndexAction))
                {
                    context.Result = new RedirectToActionResult("Index", "Admin", new { area = "Admin" });
                    return;
                }
            }
            // Kullanıcı giriş yapmış ama Admin/Index (login) sayfasına erişmeye çalışıyorsa ana panele yönlendir
            else if (isAdminController && isIndexAction)
            {
                context.Result = new RedirectToActionResult("Index", "Main", new { area = "Admin" });
                return;
            }

            if (!context.HttpContext.User.IsInRole("Admin"))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Main", new { area = "Admin" });
                return;
            }
        }
    }
} 