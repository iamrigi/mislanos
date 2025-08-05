from azure.storage.blob import BlobServiceClient
from azure.cosmos import CosmosClient
import gzip
import json

# Blob Storage client setup
blob_service_client = BlobServiceClient("<BLOB_STORAGE_CONNECTION_STRING>")
archive_container = blob_service_client.get_container_client("<ARCHIVE_CONTAINER_NAME>")

# Cosmos DB client setup
cosmos_client = CosmosClient("<COSMOS_ENDPOINT>", "<COSMOS_KEY>")
database = cosmos_client.get_database_client("<DATABASE_NAME>")
container = database.get_container_client("<CONTAINER_NAME>")

def fetch_billing_record(record_id):
    try:
        # Try fetching from Cosmos DB
        record = container.read_item(item=record_id, partition_key=record_id)
        return record
    except:
        # Fall back to Blob Storage
        blob_client = archive_container.get_blob_client(f"{record_id}.gz")
        compressed_record = blob_client.download_blob().readall()
        record = json.loads(gzip.decompress(compressed_record).decode('utf-8'))
        return record