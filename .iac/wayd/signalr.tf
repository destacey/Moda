resource "azurerm_signalr_service" "wayd_signalr" {
  name                = var.signalr_name
  resource_group_name = azurerm_resource_group.wayd_dev_rg.name
  location            = azurerm_resource_group.wayd_dev_rg.location

  sku {
    name     = var.signalr_sku
    capacity = 1
  }

  service_mode = "Default"

  cors {
    allowed_origins = [
      "https://wayd-client.${azurerm_container_app_environment.wayd_cae.default_domain}",
      "https://${azurerm_static_web_app.wayd_swa.default_host_name}",
    ]
  }

  tags = local.common_tags
}
