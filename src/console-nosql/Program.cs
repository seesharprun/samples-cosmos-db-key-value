using System.Configuration;

using Azure.Identity;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

CosmosClientOptions options = new()
{
    SerializerOptions = new CosmosSerializationOptions
    {
        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
    }
};

CosmosClient client = new(
    accountEndpoint: configuration.GetSection("NOSQL_ENDPOINT").Value ?? throw new ConfigurationErrorsException("Missing Azure Cosmos DB for NoSQL account endpoint"),
    tokenCredential: new DefaultAzureCredential(),
    clientOptions: options
);

Container container = client.GetContainer(
    databaseId: configuration.GetSection("NOSQL_DATABASENAME").Value ?? throw new ConfigurationErrorsException("Missing Azure Cosmos DB for NoSQL database name"),
    containerId: configuration.GetSection("NOSQL_CONTAINERNAME").Value ?? throw new ConfigurationErrorsException("Missing Azure Cosmos DB for NoSQL container name")
);

Console.WriteLine($"Starting...");

Console.WriteLine($"> ENDPOINT: {client.Endpoint}");
Console.WriteLine($"> DATABASE: {container.Database.Id}");
Console.WriteLine($"> CONTAINER: {container.Id}");

// Insert an item
{
    // Create an item with a composite key of "/id" and "/organization".
    Item item = new(
        Id: "azure-samples.orleans-url-shortener",
        Organization: "azure-samples",
        Repository: "orleans-url-shortener"
    );

    // Use the composite key of "/id" and "/organization" to create or replace an item.
    ItemResponse<Item> response = await container.UpsertItemAsync(
        item: item,
        partitionKey: new PartitionKey(item.Organization)
    );
    Item result = response.Resource;

    Console.WriteLine($"Created item:\t{result.Id}");
    Console.WriteLine($"Request units:\t{response.RequestCharge}");
}

// Read an item
{
    // Retrieve an item using a single request unit using the same composite key. The "value" is the item itself.
    Response<Item> response = await container.ReadItemAsync<Item>(
        id: "azure-samples.orleans-url-shortener",
        partitionKey: new PartitionKey("azure-samples")
    );
    Item result = response.Resource;

    Console.WriteLine($"Retrieved item:\t{result}");
    Console.WriteLine($"Request units:\t{response.RequestCharge}");
}

Console.WriteLine($"Stopping...");