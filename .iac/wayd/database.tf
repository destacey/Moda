resource "azurerm_mssql_server" "wayd_sql_server" {
  name                         = "sql-${local.name_stem}"
  resource_group_name          = azurerm_resource_group.wayd_dev_rg.name
  location                     = azurerm_resource_group.wayd_dev_rg.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_login
  administrator_login_password = var.sql_admin_pass
  azuread_administrator {
    tenant_id      = data.azurerm_client_config.current.tenant_id
    object_id      = var.sql_ad_admin_object_id
    login_username = var.sql_ad_admin_login_username
  }

  tags = local.common_tags
}

resource "azurerm_mssql_database" "wayd_db" {
  name                        = "sqldb-${local.name_stem}"
  server_id                   = azurerm_mssql_server.wayd_sql_server.id
  auto_pause_delay_in_minutes = var.sql_auto_pause_delay_in_minutes
  max_size_gb                 = var.sql_max_size_gb
  sku_name                    = var.sql_sku_name
  min_capacity                = var.sql_min_capacity
  read_replica_count          = 0
  read_scale                  = false

  tags = local.common_tags
}

resource "azurerm_mssql_firewall_rule" "allow_azure" {
  count            = var.allow_azure_services_sql_access ? 1 : 0
  name             = "Allow Azure Services"
  server_id        = azurerm_mssql_server.wayd_sql_server.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}
