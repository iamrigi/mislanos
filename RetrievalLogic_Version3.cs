using System;
using System.IO;
using System.Text.Json;
using System.IO.Compression;
using Microsoft.Azure.Cosmos;
using Azure.Storage.Blobs;

public class RetrievalService
{
    private readonly CosmosClient _cosmosClient;
    private readonly BlobServiceClient _blobServiceClient;

    public RetrievalService(string cosmosEndpoint, string cosmosKey, string blobConnectionString)
    {
        _cosmosClient = new CosmosClient(cosmosEndpoint, cosmosKey);
        _blobServiceClient = new BlobServiceClient(blobConnectionString);
    }

    public async Task<dynamic> FetchBillingRecordAsync(string databaseName, string containerName, string blobContainerName, string recordId, string partitionKey)
    {
        var database = _cosmosClient.GetDatabase(databaseName);
        var container = database.GetContainer(containerName);
        var blobContainer = _blobServiceClient.GetBlobContainerClient(blobContainerName);

        try
        {
            // Try fetching from Cosmos DB
            var response = await container.ReadItemAsync<dynamic>(recordId, new PartitionKey(partitionKey));
            return response.Resource;
        }
        catch
        {
            // Fall back to Blob Storage
            var blobClient = blobContainer.GetBlobClient($"{recordId}.gz");
            var blobStream = await blobClient.OpenReadAsync();

            using var decompressedStream = new GZipStream(blobStream, CompressionMode.Decompress);
            using var reader = new StreamReader(decompressedStream);
            var recordData = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<dynamic>(recordData);
        }
    }
}