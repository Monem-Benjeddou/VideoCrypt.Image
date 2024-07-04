// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VideoCrypt.Image.Main.Pages.Account
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
