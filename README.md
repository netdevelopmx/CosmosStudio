# Cosmos DB Studio

Cosmos DB Studio is a web application designed to interact with Azure Cosmos DB. It allows users to connect to their Cosmos DB account, manage documents, and perform CRUD operations within a user-friendly interface.

## Features

- Connect to Azure Cosmos DB using primary connection strings
- List databases and containers
- Run custom queries
- Display query results in a tabular format with options to edit or delete documents
- Edit documents using a JSON editor
- Delete documents with confirmation prompt

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Azure Cosmos DB account](https://azure.microsoft.com/services/cosmos-db/)

### Installing

1. Clone the repository
   ```sh
   git clone https://github.com/your-username/CosmosDBStudio.git
 
### Usage
After starting the application, navigate to https://localhost:7044/ (or the port you've configured) in your web browser to access the Cosmos DB Studio interface.

Enter your Cosmos DB endpoint URI and primary key.
Click the 'Connect' button to list the databases and containers.
Select a container to run queries and manage documents.
Use the JSON editor to edit documents and click 'Save' to update changes.
Click the 'Delete' button beside any document to remove it from the database.
