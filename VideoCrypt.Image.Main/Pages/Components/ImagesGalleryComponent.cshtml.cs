using Hydro;
using VideoCrypt.Image.Main.Repository;

namespace VideoCrypt.Image.Main.Pages.Components;

public class ImagesGalleryComponent(IFileRepository fileRepository) : HydroComponent
{
    private IFileRepository _fileRepository { get; } = fileRepository;
    private Task<List<string>>? _cachedImages;

    public int CurrentPage { get; set; }
    public int PageSize { get; set; } = 8; 
    public int TotalPages { get; private set; }

    public Cache<Task<List<string>>> Images => Cache(async () =>
    {
        if (_cachedImages != null)
            return await _cachedImages;

        var allImages = await _fileRepository.ListFilesAsync(CurrentPage,PageSize);
        TotalPages = allImages.TotalPages;
        
        return allImages != null && allImages.Items.Any() ? allImages.Items : new List<string>();
    });
}