using CosmosStudio.UI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using CosmosStudio.UI.Service; // Ensure this is the correct namespace for your service

namespace CosmosStudio.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICosmosDbService _cosmosDbService;

        public HomeController(ILogger<HomeController> logger, ICosmosDbService cosmosDbService)
        {
            _logger = logger;
            _cosmosDbService = cosmosDbService;
        }

        public IActionResult Index()
        {
            var viewModel = new CosmosDbViewModel();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GetAllDatabasesAndContainers(string endpointUri, string primaryKey)
        {
            try
            {
                var databasesAndContainers = await _cosmosDbService.GetAllDatabasesAndContainersAsync(endpointUri, primaryKey);
                return Json(databasesAndContainers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving databases and containers.");
                return StatusCode(500, "Internal server error");
            }
        }

        public async Task<IActionResult> GetContainerItems(string endpointUri, string primaryKey, string databaseId, string containerId, string query = null)
        {
            try
            {
                var items = await _cosmosDbService.GetContainerItemsAsync(endpointUri, primaryKey, databaseId, containerId, query);
                return PartialView("_ItemsResult", items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving container items.");
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDocument(UpdateDocumentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.endpointUri) || string.IsNullOrWhiteSpace(request.primaryKey) ||
                string.IsNullOrWhiteSpace(request.databaseId) || string.IsNullOrWhiteSpace(request.containerId) || string.IsNullOrWhiteSpace(request.id))
            {
                return BadRequest("All parameters are required.");
            }

            try
            {
                var message = await _cosmosDbService.UpdateDocumentAsync(request);
                return Ok(new { message = message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document.");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDocument(string endpointUri, string primaryKey, string databaseId, string containerId, string id)
        {
            try
            {
                var message = await _cosmosDbService.DeleteDocumentAsync(endpointUri, primaryKey, databaseId, containerId, id);
                return Ok(new { message = message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document.");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public IActionResult ClearConnection()
        {
            var viewModel = new CosmosDbViewModel(); // Resetting the ViewModel
            return View("_ConnectionSettings", viewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
