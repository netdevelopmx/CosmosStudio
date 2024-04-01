using Newtonsoft.Json.Linq;

namespace CosmosStudio.UI.Models
{
    public class UpdateDocumentRequest
    {
        public string endpointUri { get; set; }
        public string primaryKey { get; set; }
        public string databaseId { get; set; }
        public string containerId { get; set; }
        public string id { get; set; }
        public string updatedDocument { get; set; }
    }
}
