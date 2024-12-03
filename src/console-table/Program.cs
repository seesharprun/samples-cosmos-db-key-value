using System.Configuration;

using Azure;
using Azure.Data.Tables;
using Azure.Identity;

using Microsoft.Extensions.Configuration;

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

TableServiceClient client = new(
    endpoint: new Uri(configuration.GetSection("TABLE_ENDPOINT").Value ?? throw new ConfigurationErrorsException("Missing Azure Cosmos DB for Table account endpoint")),
    tokenCredential: new DefaultAzureCredential()
);

TableClient table = client.GetTableClient(
    tableName: configuration.GetSection("TABLE_TABLENAME").Value ?? throw new ConfigurationErrorsException("Missing Azure Cosmos DB for Table table name")
);

Console.WriteLine($"Starting...");

Console.WriteLine($"> ENDPOINT: {client.Uri}");
Console.WriteLine($"> TABLE: {table.Name}");

// Insert an entity
{
    Entity entity = new()
    {
        RowKey = "azure-samples.orleans-url-shortener",
        PartitionKey = "azure-samples",
        Repository = "orleans-url-shortener",
    };

    // Use the composite key of "/rowKey" and "/partitionKey" to create or replace an entity.
    await table.UpsertEntityAsync<Entity>(
        entity: entity,
        mode: TableUpdateMode.Replace
    );

    Console.WriteLine($"Created entity:\t{entity.RowKey}");
}

// Read an entity
{
    // Retrieve an entity using a single request unit using the same composite key. The "value" is the entity itself.
    Response<Entity> response = await table.GetEntityAsync<Entity>(
        rowKey: "azure-samples.orleans-url-shortener",
        partitionKey: "azure-samples"
    );
    Entity result = response.Value;

    Console.WriteLine($"Retrieved entity:\t{result}");
}

Console.WriteLine($"Stopping...");