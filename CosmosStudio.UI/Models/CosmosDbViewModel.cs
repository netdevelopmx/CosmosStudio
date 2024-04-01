namespace CosmosStudio.UI.Models
{
    public class CosmosDbViewModel
    {
        public string EndpointUri { get; set; }
        public string PrimaryKey { get; set; }
        public string DatabaseId { get; set; }
        public List<string> Containers { get; set; }

        public List<CosmosDbItem> Items { get; set; }

    }

    public class CosmosDbItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        // ... Other properties
    }
}
