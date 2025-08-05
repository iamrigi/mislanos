import datetime
from azure.cosmos import CosmosClient
from azure.storage.blob import BlobServiceClient
import gzip
import json

# Cosmos DB client setup
cosmos_client = CosmosClient("<COSMOS_ENDPOINT>", "<COSMOS_KEY>")
database = cosmos_client.get_database_client("<DATABASE_NAME>")
container = database.get_container_client("<CONTAINER_NAME>")

# Azure Blob Storage client setup
blob_service_client = BlobServiceClient("<BLOB_STORAGE_CONNECTION_STRING>")
archive_container = blob_service_client.get_container_client("<ARCHIVE_CONTAINER_NAME>")

def archive_old_records():
    current_date = datetime.datetime.utcnow()
    cutoff_date = current_date - datetime.timedelta(days=90)  # 3 months cut-off

    # Query Cosmos DB for records older than cutoff_date
    query = "SELECT * FROM c WHERE c.timestamp < @cutoff_date"
    records_to_archive = container.query_items(
        query=query,
        parameters=[{"name": "@cutoff_date", "value": cutoff_date.isoformat()}],
        enable_cross_partition_query=True
    )

    for record in records_to_archive:
        # Compress the record
        compressed_record = gzip.compress(json.dumps(record).encode('utf-8'))

        # Save to Blob Storage
        blob_name = f"{record['id']}.gz"
        blob_client = archive_container.get_blob_client(blob_name)
        blob_client.upload_blob(compressed_record)

        # Delete record from Cosmos DB
        container.delete_item(record, partition_key=record['partition_key'])

if __name__ == "__main__":
    archive_old_records()