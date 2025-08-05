using System;
using System.IO;
using System.Text.Json;
using System.IO.Compression;
using Microsoft.Azure.Cosmos;
using Azure.Storage.Blobs;

public class ArchivalService
{
    private readonly CosmosClient _cosmosClient;
    private readonly BlobServiceClient _blobServiceClient;

    public ArchivalService(string cosmosEndpoint, string cosmosKey, string blobConnectionString)
    {
        _cosmosClient = new CosmosClient(cosmosEndpoint, cosmosKey);
        _blobServiceClient = new BlobServiceClient(blobConnectionString);
    }

    public async Task ArchiveOldRecordsAsync(string databaseName, string containerName, string blobContainerName)
    {
        var database = _cosmosClient.GetDatabase(databaseName);
        var container = database.GetContainer(containerName);
        var blobContainer = _blobServiceClient.GetBlobContainerClient(blobContainerName);

        var cutoffDate = DateTime.UtcNow.AddMonths(-3);

        var query = new QueryDefinition("SELECT * FROM c WHERE c.timestamp < @cutoffDate")
            .WithParameter("@cutoffDate", cutoffDate.ToString("o"));

        var iterator = container.GetItemQueryIterator<dynamic>(query);

        while (iterator.HasMoreResults)
        {
            foreach (var record in await iterator.ReadNextAsync())
            {
                // Compress the record
                var recordData = JsonSerializer.Serialize(record);
                var compressedData = CompressData(recordData);

                // Save to Blob Storage
                var blobName = $"{record.id}.gz";
                var blobClient = blobContainer.GetBlobClient(blobName);
                await blobClient.UploadAsync(new MemoryStream(compressedData));

                // Delete record from Cosmos DB
                await container.DeleteItemAsync<dynamic>(record.id.ToString(), new PartitionKey(record.partitionKey.ToString()));
            }
        }
    }

    private byte[] CompressData(string data)
    {
        using var memoryStream = new MemoryStream();
        using var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal);
        using var writer = new StreamWriter(gzipStream);
        writer.Write(data);
        writer.Close();
        return memoryStream.ToArray();
    }
}