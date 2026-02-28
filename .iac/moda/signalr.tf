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
      "https://${azurerm_container_app.moda_frontend.ingress.0.fqdn}",
      "https://${azurerm_static_site.moda_swa.default_host_name}",
    ]
  }
}
