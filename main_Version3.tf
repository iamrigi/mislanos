resource "azurerm_cosmosdb_account" "example" {
  name                = "example-cosmosdb"
  location            = "East US"
  resource_group_name = "example-resources"
  offer_type          = "Standard"
}

resource "azurerm_storage_account" "example" {
  name                     = "examplestorage"
  resource_group_name      = "example-resources"
  location                 = "East US"
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_function_app" "example" {
  name                = "example-function"
  location            = "East US"
  resource_group_name = "example-resources"
  storage_account_name = azurerm_storage_account.example.name
}