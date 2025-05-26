using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace marketimnet.wepUI.Filters
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
            var adminEmail = context.HttpContext.Session.GetString("AdminEmail");
            var adminPassword = context.HttpContext.Session.GetString("AdminPassword");

            var configEmail = _configuration["AdminCredentials:Email"];
            var configPassword = _configuration["AdminCredentials:Password"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword) ||
                adminEmail != configEmail || adminPassword != configPassword)
            {
                context.Result = new RedirectToActionResult("Login", "Admin", null);
            }
        }
    }
} 