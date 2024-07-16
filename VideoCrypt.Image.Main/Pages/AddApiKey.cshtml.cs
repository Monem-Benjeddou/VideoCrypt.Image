using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VideoCrypt.Image.Main.Models;
using VideoCrypt.Image.Main.Repository;

namespace VideoCrypt.Image.Main.Pages
{
    public class AddApiKey : PageModel
    {
        private readonly IApiKeyRepository _apiKeyRepository;

        public AddApiKey(IApiKeyRepository apiKeyRepository)
        {
            _apiKeyRepository = apiKeyRepository ?? throw new ArgumentNullException(nameof(apiKeyRepository));
        }

        [BindProperty,Required]
        public string Name { get; set; }

        [BindProperty,Required]
        public string? Description { get; set; }

        [BindProperty,Required]
        public DateTime? ExpireAt { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var apiKey = new ApiKeyForCreation
            {
                Name = Name,
                Description = Description,
                ExpireAt = ExpireAt
            };

            await _apiKeyRepository.CreateApiKeyAsync(apiKey);
            return RedirectToPage("apikey");
        }
    }
}