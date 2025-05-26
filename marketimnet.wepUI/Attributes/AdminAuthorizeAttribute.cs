using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace marketimnet.wepUI.Attributes
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Main", new { area = "Admin" });
                return;
            }

            if (!user.IsInRole("Admin"))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Main", new { area = "Admin" });
                return;
            }
        }
    }
} 