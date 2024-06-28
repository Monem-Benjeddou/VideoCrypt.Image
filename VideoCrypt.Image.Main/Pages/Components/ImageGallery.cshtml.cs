using Hydro;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VideoCrypt.Image.Main.Utils;

namespace VideoCrypt.Image.Main.Pages.Components
{
    public class ImageGallery : HydroComponent
    {
        public Task<List<string>>? _cachedImages;
        public Cache<Task<List<string>>>? Images => Cache(async () =>
        {
            if (_cachedImages is null)
            {
                    
                var images = await S3Utils.ListFilesAsync();
                return images.Any() ? images : new List<string>();
            }

            return new();
        });

    }
}