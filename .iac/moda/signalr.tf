resource "azurerm_signalr_service" "moda_signalr" {
  name                = var.signalr_name
  resource_group_name = azurerm_resource_group.moda_dev_rg.name
  location            = azurerm_resource_group.moda_dev_rg.location

  sku {
    name     = var.signalr_sku
    capacity = 1
  }

  service_mode = "Default"

  cors {
    allowed_origins = [
      "https://moda-client.${azurerm_container_app_environment.moda_cae.default_domain}",
      "https://${azurerm_static_site.moda_swa.default_host_name}",
    ]
  }
}
