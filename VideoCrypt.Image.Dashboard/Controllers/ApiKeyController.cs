using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VideoCrypt.Image.Dashboard.Models;
using VideoCrypt.Image.Dashboard.Repositories;

namespace VideoCrypt.Image.Dashboard.Controllers;
public class ApiKeyController(IApiKeyRepository apiKeyRepository) : Controller
{
    private readonly IApiKeyRepository _apiKeyRepository = apiKeyRepository;
    public  async  Task<IActionResult> Index(int pageIndex = 1,int pageSize = 8)
    {
        var items = await _apiKeyRepository.GetApiKeysAsync(pageIndex, pageSize);
        
        return View(items);
    }

    [HttpPost]
    public async Task<IActionResult> CreateApiKey(ApiKeyForCreation apiKeyForCreation)
    {
        if (ModelState.IsValid)
        {
            var isCreated = await _apiKeyRepository.CreateApiKeyAsync(apiKeyForCreation);
            if (isCreated != null)
            {
                return RedirectToAction("Index");
            }
        }
        return View(apiKeyForCreation);
    }
}