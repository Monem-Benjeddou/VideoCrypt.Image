using Hydro;
using VideoCrypt.Image.Main.Models;
using VideoCrypt.Image.Main.Repository;

namespace VideoCrypt.Image.Main.Pages.Components;

public class ImagesGalleryComponent(IFileRepository fileRepository) : HydroComponent
{
    private IFileRepository _fileRepository { get; } = fileRepository;
    private Task<PaginatedList<string>>? _cachedImages;

    public int CurrentPage { get; set; }
    public int PageSize { get; set; } = 8; 
    public int TotalPages { get; private set; }

    public void PageChangedAsync(int i)
    {
        CurrentPage=i; 
        Render();
    }

    public Cache<Task<PaginatedList<string>>> Images => Cache(async () =>
    {
        if (_cachedImages != null)
            return await _cachedImages;
        _cachedImages =  _fileRepository.ListFilesAsync(CurrentPage,PageSize);
        var images = await _cachedImages;
        TotalPages = images.TotalPages;
        Render();
        return images != null && images.Items.Any() ? images : new PaginatedList<string>();
    });

    public void ClearCache()
    {
        _cachedImages = null;
    }
}