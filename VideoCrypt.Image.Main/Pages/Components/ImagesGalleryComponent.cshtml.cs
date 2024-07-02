using Hydro;
using Microsoft.AspNetCore.Components;
using VideoCrypt.Image.Main.Repository;

namespace VideoCrypt.Image.Main.Pages.Components;

public class ImagesGalleryComponent(IFileRepository fileRepository) : HydroComponent
{
    private IFileRepository _fileRepository { get; } = fileRepository;
    private Task<List<byte[]>>? _cachedImages;

    public Cache<Task<List<byte[]>>> Images => Cache(async () =>
    {
        if (_cachedImages != null)
            return await _cachedImages;

        _cachedImages = _fileRepository.ListFilesAsync();
        var images = await _cachedImages;
        return images.Any() ? images : new List<byte[]>();
    });
}