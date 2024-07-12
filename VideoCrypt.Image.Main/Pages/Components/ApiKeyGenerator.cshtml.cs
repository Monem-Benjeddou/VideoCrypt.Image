using Hydro;
using VideoCrypt.Image.Main.Models;
using VideoCrypt.Image.Main.Repository;

namespace VideoCrypt.Image.Main.Pages.Components;

public class ApiKeyGenerator(IApiKeyRepository _apiKeyRepository) : HydroComponent
{
    private Task<PaginatedList<ApiKey>>? _apiKeysCache;
    
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 8; 
    public int TotalPages { get; private set; }
    public void DeleteApiKey(int id)
    {
        var result = _apiKeyRepository.DeleteApiKeyAsync(id).Result;
    }
    public Cache<Task<PaginatedList<ApiKey>>> ApiKeys => Cache(async () =>
    {
        if (_apiKeysCache != null)
            return await _apiKeysCache;
        _apiKeysCache =  _apiKeyRepository.GetApiKeysAsync(CurrentPage,TotalPages);
        var apiKeys = await _apiKeysCache;
        TotalPages = apiKeys.TotalPages;
        CurrentPage = 1;
        Render();
        return apiKeys != null && apiKeys.Items.Any() ? apiKeys : new PaginatedList<ApiKey>();
    });

}