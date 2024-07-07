using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VideoCrypt.Image.Main.Pages
{
    [Authorize]
    public class ImageGallery : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public void OnGet(int currentPage)
        {
            CurrentPage = currentPage;
        }
    }
}