using Hydro;
using Microsoft.AspNetCore.Authorization;
using VideoCrypt.Image.Main.Utils;

namespace VideoCrypt.Image.Main.Pages.Components;

public class ImagesGalleryComponent : HydroComponent
{
    private Task<List<byte[]>>? _cachedImages;

    public Cache<Task<List<byte[]>>> Images => Cache(async () =>
    {
        if (_cachedImages != null)
            return await _cachedImages;

        _cachedImages = S3Utils.ListFilesAsync(); 
        var images = await _cachedImages;
        return images.Any() ? images : new List<byte[]>();
    });
}

