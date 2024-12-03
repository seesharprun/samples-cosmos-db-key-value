# Use Azure Cosmos DB as a key-value store

This is a quick example illustrating how to use Azure Cosmos DB as a key-value store

## Highlights

- Deploys an API for NoSQL and API for Table account with indexing disabled
- Inserts an item with a unique identifier and a partition key value
- Performs a point read of the item using the composite of the unique identifer and partition key value

## Supports

- [Azure Cosmos DB for NoSQL](https://learn.microsoft.com/azure/cosmos-db/nosql)
- [Azure Cosmos DB for Table](https://learn.microsoft.com/azure/cosmos-db/table)

## Get started

1. Run `azd init`

1. Set environment name

1. Run `azd up`

1. Select subscription, region, and resource group

1. In the `src/*` folder\[s\], run either console project using `dotnet run`
