using Hydro;
using VideoCrypt.Image.Main.Models;

namespace VideoCrypt.Image.Main.Pages.Components;

public class ApiKeyGenerator : HydroComponent
{
    private Task<PaginatedList<ApiKey>>? _apiKey;
    public void DeleteApiKey(int id)
    {
        
    }
}