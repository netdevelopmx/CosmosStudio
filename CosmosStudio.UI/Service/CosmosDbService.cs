using CosmosStudio.UI.Models;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;

namespace CosmosStudio.UI.Service
{
    public class CosmosDbService : ICosmosDbService
    {
        public async Task<Dictionary<string, List<string>>> GetAllDatabasesAndContainersAsync(string endpointUri, string primaryKey)
        {
            var cosmosClient = new CosmosClient(endpointUri, primaryKey);
            var databasesAndContainers = new Dictionary<string, List<string>>();

            var databaseIterator = cosmosClient.GetDatabaseQueryIterator<DatabaseProperties>();
            var databasesList = new List<DatabaseProperties>();

            // Retrieve all databases
            while (databaseIterator.HasMoreResults)
            {
                var databases = await databaseIterator.ReadNextAsync();
                databasesList.AddRange(databases);
            }

            // Sort databases by name (Id property)
            var sortedDatabases = databasesList.OrderBy(db => db.Id).ToList();

            // Iterate over sorted databases and retrieve containers
            foreach (var database in sortedDatabases)
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
            try
            {
                using var cosmosClient = new CosmosClient(request.endpointUri, request.primaryKey);
                var container = cosmosClient.GetContainer(request.databaseId, request.containerId);

                // Fetch the container's partition key paths to determine the strategy
                var containerProperties = await container.ReadContainerAsync();
                var partitionKeyPaths = containerProperties.Resource.PartitionKeyPaths;

                // Parse the document to be updated
                var updatedDocument = JObject.Parse(request.updatedDocument);


                // If there's only one partition key path, use it to construct the PartitionKey
                if (partitionKeyPaths.Count == 1)
                {
                    var partitionKeyPath = partitionKeyPaths.First().TrimStart('/');
                    var partitionKeyValue = updatedDocument.SelectToken(partitionKeyPath)?.ToString();
                    if (string.IsNullOrEmpty(partitionKeyValue))
                    {
                        throw new InvalidOperationException($"Value for partition key path '{partitionKeyPath}' is missing in the document.");
                    }

                    var partitionKey = new PartitionKey(partitionKeyValue);
                    await container.ReplaceItemAsync(updatedDocument, request.id, partitionKey);
                }
                else
                {
                    // When multiple partition key paths are present or when the partition key is not used,
                    // rely on the unique document ID for the update.
                    // This approach assumes the ID is sufficient for Cosmos DB to locate and update the document.
                    // Note: This may not adhere to best performance practices, especially for large datasets.
                    await container.ReplaceItemAsync(updatedDocument, request.id);
                }

                return "Document updated successfully.";
            }
            catch (CosmosException ex)
            {
                return $"Cosmos DB operation failed: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
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
