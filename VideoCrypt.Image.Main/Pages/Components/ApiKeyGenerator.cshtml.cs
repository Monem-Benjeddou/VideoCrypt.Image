using Hydro;
using VideoCrypt.Image.Main.Models;
using VideoCrypt.Image.Main.Repository;
namespace VideoCrypt.Image.Main.Pages.Components
{
    public class ApiKeyGenerator : HydroComponent
    {
        private readonly IApiKeyRepository _apiKeyRepository;
        private Task<PaginatedList<ApiKey>> _apiKeysCache;

        public ApiKeyGenerator(IApiKeyRepository apiKeyRepository)
        {
            _apiKeyRepository = apiKeyRepository ?? throw new ArgumentNullException(nameof(apiKeyRepository));
        }

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; private set; }

        public async Task DeleteApiKey(int id)
        {
            await _apiKeyRepository.DeleteApiKeyAsync(id);
            _apiKeysCache = null;
            await LoadApiKeys();
        }

        public Cache<Task<PaginatedList<ApiKey>>> ApiKeys => Cache(async () =>
        {
            if (_apiKeysCache != null)
                return await _apiKeysCache;

            await LoadApiKeys();

            return _apiKeysCache != null ? await _apiKeysCache : new PaginatedList<ApiKey>();
        });

        private async Task LoadApiKeys()
        {
            _apiKeysCache ??= _apiKeyRepository.GetApiKeysAsync(CurrentPage, PageSize);
            var apiKeys = await _apiKeysCache;
            TotalPages = apiKeys.TotalPages;
            Render();
        }

        public async Task GoToPage(int page)
        {
            if (page < 1 || page > TotalPages || page == CurrentPage)
                return;

            CurrentPage = page;
            await LoadApiKeys();
        }
    }
}