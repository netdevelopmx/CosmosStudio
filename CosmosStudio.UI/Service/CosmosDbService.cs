using CosmosStudio.UI.Models;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace CosmosStudio.UI.Service
{
    public class CosmosDbService : ICosmosDbService
    {
        public async Task<Dictionary<string, List<string>>> GetAllDatabasesAndContainersAsync(string endpointUri, string primaryKey)
        {
            var cosmosClient = new CosmosClient(endpointUri, primaryKey);
            var databasesAndContainers = new Dictionary<string, List<string>>();

            var databaseIterator = cosmosClient.GetDatabaseQueryIterator<DatabaseProperties>();
            while (databaseIterator.HasMoreResults)
            {
                var databases = await databaseIterator.ReadNextAsync();
                foreach (var database in databases)
                {
                    var containerIterator = cosmosClient.GetDatabase(database.Id).GetContainerQueryIterator<ContainerProperties>();
                    var containers = new List<string>();
                    while (containerIterator.HasMoreResults)
                    {
                        var response = await containerIterator.ReadNextAsync();
                        containers.AddRange(response.Select(container => container.Id));
                    }
                    databasesAndContainers.Add(database.Id, containers);
                }
            }

            return databasesAndContainers;
        }

        public async Task<List<JObject>> GetContainerItemsAsync(string endpointUri, string primaryKey, string databaseId, string containerId, string query = null)
        {
            var cosmosClient = new CosmosClient(endpointUri, primaryKey);
            var container = cosmosClient.GetContainer(databaseId, containerId);
            var items = new List<JObject>();
            query ??= "SELECT TOP 1000 * FROM c";

            var iterator = container.GetItemQueryIterator<JObject>(new QueryDefinition(query));
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                items.AddRange(response);
            }

            return items;
        }

        public async Task<string> UpdateDocumentAsync(UpdateDocumentRequest request)
        {
            using var cosmosClient = new CosmosClient(request.endpointUri, request.primaryKey);
            var container = cosmosClient.GetContainer(request.databaseId, request.containerId);
            var readResponse = await container.ReadItemAsync<JObject>(request.id, new PartitionKey(request.id));
            var etag = readResponse.ETag;

            var requestOptions = new ItemRequestOptions { IfMatchEtag = etag };
            var updatedDocument = JObject.Parse(request.updatedDocument);
            await container.ReplaceItemAsync(updatedDocument, request.id, new PartitionKey(request.id), requestOptions);

            return "Document updated successfully.";
        }

        public async Task<string> DeleteDocumentAsync(string endpointUri, string primaryKey, string databaseId, string containerId, string id)
        {
            using var cosmosClient = new CosmosClient(endpointUri, primaryKey);
            var container = cosmosClient.GetContainer(databaseId, containerId);
            await container.DeleteItemAsync<JObject>(id, new PartitionKey(id));

            return "Document deleted successfully.";
        }
    }
}
