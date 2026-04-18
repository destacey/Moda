# resource "azurerm_storage_account" "moda_storage" {
#   name                     = "stmodadev"
#   resource_group_name      = azurerm_resource_group.moda_dev_rg.name
#   location                 = azurerm_resource_group.moda_dev_rg.location
#   account_tier             = "Standard"
#   account_replication_type = "LRS"
# }

resource "azurerm_mssql_server" "moda_sql_server" {
  name                         = "sql-moda-dev"
  resource_group_name          = azurerm_resource_group.moda_dev_rg.name
  location                     = azurerm_resource_group.moda_dev_rg.location
  version                      = "12.0"
  administrator_login          = "modaadmin"
  administrator_login_password = var.sql_admin_pass
  azuread_administrator {
    tenant_id      = data.azurerm_client_config.current.tenant_id
    object_id      = "aca6c8fd-ba48-4931-b0c8-006550378db4"
    login_username = "Moda SQL Admins"
  }
}

resource "azurerm_mssql_database" "moda_db" {
  name                        = "sqldb-moda-dev"
  server_id                   = azurerm_mssql_server.moda_sql_server.id
  auto_pause_delay_in_minutes = 60
  max_size_gb                 = 10
  sku_name                    = "GP_S_Gen5_1"
  min_capacity                = 0.5
  read_replica_count          = 0
  read_scale                  = false
}

resource "azurerm_mssql_firewall_rule" "allow_azure" {
  name             = "Allow Azure Services"
  server_id        = azurerm_mssql_server.moda_sql_server.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}
