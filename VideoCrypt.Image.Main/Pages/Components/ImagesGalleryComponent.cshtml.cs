using Hydro;
using VideoCrypt.Image.Main.Repository;

namespace VideoCrypt.Image.Main.Pages.Components;

public class ImagesGalleryComponent(IFileRepository fileRepository) : HydroComponent
{
    private IFileRepository _fileRepository { get; } = fileRepository;
    private Task<List<string>>? _cachedImages;

    public int CurrentPage { get; set; }
    public int PageSize { get; set; } = 12; 
    public int TotalPages { get; private set; }

    public Cache<Task<List<string>>> Images => Cache(async () =>
    {
        if (_cachedImages != null)
            return await _cachedImages;

        var allImages = await _fileRepository.ListFilesAsync();
        var paginatedImages = allImages.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
        
        TotalPages = (int)Math.Ceiling(allImages.Count / (double)PageSize);
        
        return paginatedImages.Any() ? paginatedImages : new List<string>();
    });
}