using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VideoCrypt.Image.Main.Pages
{
    [Authorize]
    public class ImageGallery : PageModel
    {
        public void OnGet()
        {
        }
    }
}