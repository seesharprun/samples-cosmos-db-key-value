using Azure;
using Azure.Data.Tables;

public record Entity : ITableEntity
{
    public required string RowKey { get; set; }

    public required string PartitionKey { get; set; }

    public required string Repository { get; set; }

    public ETag ETag { get; set; } = ETag.All;

    public DateTimeOffset? Timestamp { get; set; }
}