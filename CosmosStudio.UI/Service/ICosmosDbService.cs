using CosmosStudio.UI.Models;
using Newtonsoft.Json.Linq;

namespace CosmosStudio.UI.Service
{
    public interface ICosmosDbService
    {
        Task<Dictionary<string, List<string>>> GetAllDatabasesAndContainersAsync(string endpointUri, string primaryKey);
        Task<List<JObject>> GetContainerItemsAsync(string endpointUri, string primaryKey, string databaseId, string containerId, string query = null);
        Task<string> UpdateDocumentAsync(UpdateDocumentRequest request);
        Task<string> DeleteDocumentAsync(string endpointUri, string primaryKey, string databaseId, string containerId, string id);
    }
}
