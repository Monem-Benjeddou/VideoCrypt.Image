
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VideoCrypt.Image.Dashboard.Views.Account
{
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogoutModel(ILogger<LogoutModel> logger,IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!string.IsNullOrEmpty(_httpContextAccessor.HttpContext.Request.Cookies["access_token"]))
            {
                _httpContextAccessor.HttpContext.Response.Cookies.Delete("access_token");
            }

            await HttpContext.SignOutAsync();

            _logger.LogInformation("User logged out.");

            return RedirectToPage("/Index");
        }
    }
    
}