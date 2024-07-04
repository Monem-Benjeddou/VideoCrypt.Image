using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using AuthenticationService = VideoCrypt.Image.Main.Authentication.AuthenticationService;

namespace VideoCrypt.Image.Main.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly AuthenticationService _authenticationService;
        private readonly ILogger<LoginModel> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginModel(AuthenticationService authenticationService, ILogger<LoginModel> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _authenticationService = authenticationService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        [BindProperty] public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        [TempData] public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "The Email field is required.")]
            [EmailAddress(ErrorMessage = "The Email field is not a valid email address.")]
            public string Email { get; set; }

            [Required(ErrorMessage = "The Password field is required.")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")] public bool RememberMe { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            try
            {
                if (ModelState.IsValid)
                {
                    var token = await _authenticationService.AuthenticateAsync(Input.Email, Input.Password);

                    if (token != null)
                    {
                        var claims = new[]
                        {
                            new Claim(ClaimTypes.Name, Input.Email),
                            new Claim("JWT", token)
                        };

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var principal = new ClaimsPrincipal(identity); 

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            principal,
                            new AuthenticationProperties
                            {
                                IsPersistent = Input.RememberMe,
                                RedirectUri = returnUrl
                            });
                        Response.Cookies.Append("access_token", token, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true, // or CookieSecurePolicy.None for development
                            SameSite = SameSiteMode.Strict
                        });
                        _logger.LogInformation("User logged in.");
                        return LocalRedirect(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while logging in.");
                ModelState.AddModelError(string.Empty, "An error occurred while processing your request.");
            }

            // If login fails or there's an error, return to the login page with errors displayed
            return Page();
        }
    }
}